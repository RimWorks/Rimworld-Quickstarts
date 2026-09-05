using System;
using System.Collections.Generic;
using System.Linq;
using RimWorks.Quickstarts.Patching;
using RimWorks.Quickstarts.UI;
using RimWorks.Quickstarts.Verification;
using RimWorld;
using RimWorld.Planet;
using UnityEngine;
using Verse;
using Verse.Profile;

namespace RimWorks.Quickstarts;

/// <summary>
/// Builds a game from a quickstart and drops the player into it. One instance owns one launch;
/// relaunching replaces the instance.
/// </summary>
[StaticConstructorOnStartup]
public class Quickstarter {
  private static bool started;
  private static bool finished;
  private static bool seedHeld;

  private readonly StatusBox? statusBox;

  private string seedUsed = string.Empty;

  static Quickstarter() {
    Instance = new Quickstarter(ConfiguredQuickstart());

    // A configured quickstart owns the launch, so the picker would be torn down a moment later.
    if (Instance.Quickstart == null) {
      VanillaQuicktest.ClaimCommandLineArg();
    }
  }

  private Quickstarter(AbstractQuickstart? quickstart) {
    Quickstart = quickstart;
    if (quickstart == null) {
      return;
    }

    statusBox = new StatusBox(this);

    LongEventHandler.ExecuteWhenFinished(() => {
      if (started) {
        return;
      }

      // Idempotent. Static ctor order is undefined, so hooks may not be on when the box draws.
      PatchBackends.ApplyBest();

      finished = false;
      started = true;
      StartGame();
      finished = true;
    });
  }

  /// <summary>The launcher for the current run, or null before startup finishes.</summary>
  public static Quickstarter? Instance { get; private set; }

  /// <summary>What is being launched, or null when nothing is configured.</summary>
  public AbstractQuickstart? Quickstart { get; }

  /// <summary>Rebuilds the game from whatever the command line or settings currently name.</summary>
  public static void ReloadQuickstart() {
    Restart(ConfiguredQuickstart());
  }

  /// <summary>Starts a quickstart the player picked by hand, ignoring whatever the settings say.</summary>
  /// <param name="quickstart">The quickstart to launch.</param>
  public static void Launch(AbstractQuickstart quickstart) {
    Restart(quickstart);
  }

  /// <summary>Draws the launch status box while the game is being built.</summary>
  public void OnGUI() {
    if (Quickstart == null || finished) {
      return;
    }

    statusBox?.OnGUI();
  }

  private static AbstractQuickstart? ConfiguredQuickstart() {
    string? requested = QuickstartArgs.SelectedName;

    if (!Prefs.DevMode) {
      if (requested != null) {
        Logger.Warn(
            "-{Arg} was passed, but dev mode is off, so no quickstart will run.",
            new object?[] { QuickstartArgs.SelectArg });
      }

      return null;
    }

    // A name that cannot resolve stops the launch instead of quietly booting a different colony.
    Type? type = requested != null ? FromName(requested) : FromSettings();

    return type == null ? null : QuickstartRegistry.Create(type);
  }

  private static Type? FromName(string requested) {
    Type? type = QuickstartLookup.Resolve(requested, QuickstartRegistry.AllTypes, out string? error);
    if (type == null) {
      Logger.Error("-{Arg}: {Reason}", new object?[] { QuickstartArgs.SelectArg, error });
      return null;
    }

    Logger.Info("Command line picked the {Quickstart} quickstart.", new object?[] { type.Name });
    return type;
  }

  private static Type? FromSettings() {
    string? name = QuickstartsMod.Settings.defaultQuickstart;
    if (string.IsNullOrEmpty(name)) {
      return null;
    }

    Type? type = QuickstartLookup.Resolve(name, QuickstartRegistry.AllTypes, out string? error);
    if (type == null) {
      Logger.Error("Default quickstart in mod settings: {Reason}", new object?[] { error });
    }

    return type;
  }

  private static void Restart(AbstractQuickstart? quickstart) {
    LongEventHandler.QueueLongEvent(
        () => {
          ReleaseSeed();
          Current.ProgramState = ProgramState.Entry;
          Current.Game = null;
          started = false;
          Instance = new Quickstarter(quickstart);
        },
        "Quickstarts_Reload",
        true,
        GameAndMapInitExceptionHandlers.ErrorWhileGeneratingMap);
  }

  private static void RunVerification(AbstractQuickstart quickstart, string seed) {
    string name = quickstart.GetType().Name;
    QuickstartVerification? verification = quickstart.Verify();

    if (verification == null) {
      Logger.Info(
          "Verify mode requested but '{Quickstart}' has no Verify(); exiting 0.",
          new object?[] { name });
      VerificationReport.Write(name, seed, null, true);
      Exit(0);
      return;
    }

    bool passed = verification.AllPassed;
    for (int i = 0; i < verification.Results.Count; i++) {
      AssertResult result = verification.Results[i];
      string line = $"  {(result.Passed ? "PASS" : "FAIL")} {result.Label}";
      if (!result.Passed && result.Detail != null) {
        line += " -- " + result.Detail;
      }

      if (result.Passed) {
        Logger.Info(line);
      } else {
        Logger.Error(line);
      }
    }

    Logger.Info(
        "Verification {Outcome} for '{Quickstart}'.",
        new object?[] { passed ? "PASSED" : "FAILED", name });
    VerificationReport.Write(name, seed, verification, passed);
    Exit(passed ? 0 : 1);
  }

  private static void Exit(int code) {
    Logger.Info("Exiting with code {Code}.", new object?[] { code });

    // Application.Quit, not Environment.Exit: an AppDomain unload from a Unity callback hangs.
    Application.Quit(code);
  }

  // Held across the generation long events. Root.Update only calls EnsureStateStackEmpty when no
  // long event is pending, so the state survives until the pop event runs.
  private static void HoldSeed(int seed) {
    // PushState, not Rand.Seed: the bare setter logs a red error when the state stack is empty.
    Rand.PushState(seed);
    seedHeld = true;
  }

  private static void ReleaseSeed() {
    if (!seedHeld) {
      return;
    }

    seedHeld = false;
    Rand.PopState();
  }

  private void StartGame() {
    seedUsed = SeedResolver.Resolve(QuickstartArgs.Seed, Quickstart!.seed) ?? GenText.RandomSeedString();
    Logger.Info(
        "Launching '{Quickstart}' with world seed {Seed}.",
        new object?[] { Quickstart.GetType().Name, seedUsed });

    LongEventHandler.QueueLongEvent(
        () => {
          MemoryUtility.ClearAllMapsAndWorld();
          HoldSeed(GenText.StableStringHash(seedUsed));

          try {
            ApplyConfiguration();
            PageUtility.InitGameStart();
          } catch {
            ReleaseSeed();
            throw;
          }

          // The queue is FIFO, so this pops after the map generation InitGameStart just queued.
          LongEventHandler.QueueLongEvent(ReleaseSeed, "Quickstarts_SeedRelease", false, null);

          // Half a second, not zero: pawns finish spawning over the first few ticks.
          DelayedActionScheduler.Schedule(OnLoaded, GenTicks.TicksPerRealSecond / 2);
        },
        "Quickstarts_StartGame",
        true,
        GameAndMapInitExceptionHandlers.ErrorWhileGeneratingMap);

    Quickstart!.PostStart();
  }

  private void ApplyConfiguration() {
    AbstractQuickstart quickstart = Quickstart!;

    Current.ProgramState = ProgramState.Entry;
    Current.Game = new Game {
      InitData = new GameInitData(),
      Scenario = quickstart.scenario.scenario,
    };
    Find.Scenario.PreConfigure();
    Current.Game.storyteller = new Storyteller(quickstart.storyteller, quickstart.difficulty);

    quickstart.PreGenerateWorld();

    Current.Game.World = WorldGenerator.GenerateWorld(
        quickstart.planetCoverage,
        seedUsed,
        OverallRainfall.Normal,
        OverallTemperature.Normal,
        OverallPopulation.Normal,
        LandmarkDensity.Normal);

    // The seed state StartGame pushed is still held here, so the tile is fixed too.
    Find.GameInitData.ChooseRandomStartingTile();

    Logger.Info(
        "Seed {Seed} picked starting tile {Tile}.",
        new object?[] { seedUsed, Find.GameInitData.startingTile });

    Find.GameInitData.mapSize = quickstart.mapSize;
    quickstart.PostApplyConfiguration();

    Find.Scenario.PostIdeoChosen();

    // After PostIdeoChosen: the scenario's own setup runs in there and would undo this.
    quickstart.PostConfigured();
  }

  private void OnLoaded() {
    // Backstop: the queued pop never runs if map generation threw.
    ReleaseSeed();

    AbstractQuickstart quickstart = Quickstart!;

    try {
      List<Pawn> pawns = Find.World.PlayerPawnsForStoryteller
          .Where(p => p is { Spawned: true, Map: not null, story: not null, needs: not null })
          .ToList();

      // An empty list means PrepareColonists silently does nothing and the run still looks fine.
      if (pawns.Count == 0) {
        Logger.Warn(
            "No colonists passed the readiness filter. World has {WorldPawns} player pawns;"
            + " map has {SpawnedColonists} spawned colonists.",
            new object?[] {
              Find.World.PlayerPawnsForStoryteller.Count(),
              Find.CurrentMap?.mapPawns?.FreeColonistsSpawnedCount ?? -1,
            });
      }

      // Counted before the handoff: a quickstart may drain the list, so Count reads zero after.
      int count = pawns.Count;
      string names = string.Join(", ", pawns.Select(p => p.LabelShort));
      quickstart.PrepareColonists(pawns);
      quickstart.PostLoaded();

      if (quickstart.pauseAfterLoad) {
        Find.TickManager.Pause();
      }

      Logger.Info(
          "Loaded '{Quickstart}' with {Colonists} colonists: {Names}.",
          new object?[] { quickstart.GetType().Name, count, names });
    } catch (Exception ex) {
      Logger.Error(ex, "Post-load setup failed");
    }

    if (QuickstartArgs.VerifyMode) {
      RunVerification(quickstart, seedUsed);
    }
  }
}
