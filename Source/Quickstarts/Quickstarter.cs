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

  private readonly StatusBox? statusBox;

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

  private static string Seed => GenText.RandomSeedString();

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
        Logger.Warning($"-{QuickstartArgs.SelectArg} was passed, but dev mode is off, so no quickstart will run.");
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
      Logger.Error($"-{QuickstartArgs.SelectArg}: {error}");
      return null;
    }

    Logger.Message($"Command line picked the {type.Name} quickstart.");
    return type;
  }

  private static Type? FromSettings() {
    string? name = QuickstartsMod.Settings.defaultQuickstart;
    if (string.IsNullOrEmpty(name)) {
      return null;
    }

    Type? type = QuickstartLookup.Resolve(name, QuickstartRegistry.AllTypes, out string? error);
    if (type == null) {
      Logger.Error($"Default quickstart in mod settings: {error}");
    }

    return type;
  }

  private static void Restart(AbstractQuickstart? quickstart) {
    LongEventHandler.QueueLongEvent(
        () => {
          Current.ProgramState = ProgramState.Entry;
          Current.Game = null;
          started = false;
          Instance = new Quickstarter(quickstart);
        },
        "Quickstarts_Reload",
        true,
        GameAndMapInitExceptionHandlers.ErrorWhileGeneratingMap);
  }

  private static void RunVerification(AbstractQuickstart quickstart) {
    string name = quickstart.GetType().Name;
    QuickstartVerification? verification = quickstart.Verify();

    if (verification == null) {
      Logger.Message($"Verify mode requested but '{name}' has no Verify(); exiting 0.");
      VerificationReport.Write(name, null, true);
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
        Logger.Message(line);
      } else {
        Logger.Error(line);
      }
    }

    Logger.Message($"Verification {(passed ? "PASSED" : "FAILED")} for '{name}'.");
    VerificationReport.Write(name, verification, passed);
    Exit(passed ? 0 : 1);
  }

  private static void Exit(int code) {
    Logger.Message($"Exiting with code {code}.");

    // Application.Quit, not Environment.Exit: an AppDomain unload from a Unity callback hangs.
    Application.Quit(code);
  }

  private void StartGame() {
    LongEventHandler.QueueLongEvent(
        () => {
          MemoryUtility.ClearAllMapsAndWorld();
          ApplyConfiguration();
          PageUtility.InitGameStart();

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
        Seed,
        OverallRainfall.Normal,
        OverallTemperature.Normal,
        OverallPopulation.Normal,
        LandmarkDensity.Normal);

    Find.GameInitData.ChooseRandomStartingTile();
    Find.GameInitData.mapSize = quickstart.mapSize;
    quickstart.PostApplyConfiguration();

    Find.Scenario.PostIdeoChosen();

    // After PostIdeoChosen: the scenario's own setup runs in there and would undo this.
    quickstart.PostConfigured();
  }

  private void OnLoaded() {
    AbstractQuickstart quickstart = Quickstart!;

    try {
      List<Pawn> pawns = Find.World.PlayerPawnsForStoryteller
          .Where(p => p is { Spawned: true, Map: not null, story: not null, needs: not null })
          .ToList();

      // An empty list means PrepareColonists silently does nothing and the run still looks fine.
      if (pawns.Count == 0) {
        Logger.Warning(
            $"No colonists passed the readiness filter. World has {Find.World.PlayerPawnsForStoryteller.Count()}"
            + $" player pawns; map has {Find.CurrentMap?.mapPawns?.FreeColonistsSpawnedCount ?? -1} spawned colonists.");
      }

      // Counted before the handoff: a quickstart may drain the list, so Count reads zero after.
      int count = pawns.Count;
      quickstart.PrepareColonists(pawns);
      quickstart.PostLoaded();

      if (quickstart.pauseAfterLoad) {
        Find.TickManager.Pause();
      }

      Logger.Message($"Loaded '{quickstart.GetType().Name}' with {count} colonists.");
    } catch (Exception ex) {
      Logger.Error($"Post-load setup failed: {ex}");
    }

    if (QuickstartArgs.VerifyMode) {
      RunVerification(quickstart);
    }
  }
}
