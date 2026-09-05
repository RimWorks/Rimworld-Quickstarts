using System;
using System.Collections.Generic;
using System.Text;
using RimWorks.Quickstarts.Verification;
using RimWorld;
using Verse;

namespace RimWorks.Quickstarts;

/// <summary>
/// One boot-into-game scenario. Subclass it, override what the test needs, and the mod finds
/// it: every non-abstract subclass with a parameterless constructor is discovered automatically.
/// </summary>
public abstract class AbstractQuickstart {
  private const string ClassSuffix = "Quickstart";

  private TaggedString? cachedDescription;
  private TaggedString? cachedLabel;

  /// <summary>One line shown in the picker and the launch status box.</summary>
  public abstract TaggedString description { get; }

  /// <summary>Human title for the picker. Defaults to the class name, de-suffixed and split.</summary>
  public virtual TaggedString label {
    get {
      cachedLabel ??= DefaultLabel();
      return cachedLabel.Value;
    }
  }

  /// <summary>Whether the game pauses once the map is ready. On by default so nothing moves before you look.</summary>
  public virtual bool pauseAfterLoad => true;

  /// <summary>Ticks to run before Verify(). Zero asserts against a map nothing has happened on.</summary>
  public virtual int ticksBeforeVerify => 0;

  /// <summary>Square map edge length in cells.</summary>
  public virtual int mapSize => 75;

  /// <summary>World size passed to the generator. Small keeps generation under a second.</summary>
  public virtual float planetCoverage => 0.05f;

  /// <summary>Fixed world seed, so a failed CI run can be replayed. Null picks a fresh one every launch.</summary>
  public virtual string? seed => null;

  /// <summary>Storyteller the game starts with.</summary>
  public virtual StorytellerDef storyteller => StorytellerDefOf.Cassandra;

  /// <summary>Difficulty the game starts with.</summary>
  public virtual DifficultyDef difficulty => DifficultyDefOf.Easy;

  /// <summary>Scenario the game starts with.</summary>
  public virtual ScenarioDef scenario => ScenarioDefOf.Crashlanded;

  /// <summary>
  /// Runs before the long event that builds the game, while the menu is still up. Toggling
  /// dev settings here means they are on for generation itself.
  /// </summary>
  public virtual void PostStart() { }

  /// <summary>
  /// Runs after the scenario is configured, before the world is generated. Put anything a
  /// world generator step has to read here, because it sees null otherwise.
  /// </summary>
  public virtual void PreGenerateWorld() { }

  /// <summary>
  /// Runs after the world exists and the scenario is configured, but before pawn generation.
  /// Change starting pawn count or scenario parts here.
  /// </summary>
  public virtual void PostApplyConfiguration() { }

  /// <summary>
  /// Runs last in configuration, after the scenario has finished its own setup. This is the
  /// only point where scenario post-processing cannot undo what you set.
  /// </summary>
  public virtual void PostConfigured() { }

  /// <summary>Runs once the map is live, with every spawned player pawn.</summary>
  /// <param name="pawns">Spawned colonists, in map order.</param>
  public virtual void PrepareColonists(List<Pawn> pawns) { }

  /// <summary>Runs after <see cref="PrepareColonists"/>, just before the pause.</summary>
  public virtual void PostLoaded() { }

  /// <summary>Assertions to run in CI mode. Return null to skip verification and exit 0.</summary>
  /// <returns>The assertions, or null when this quickstart has none.</returns>
  public virtual QuickstartVerification? Verify() => null;

  /// <summary>Builds the tooltip block shown for this quickstart in the picker.</summary>
  /// <returns>A cached multi-line summary of the quickstart's configuration.</returns>
  public virtual TaggedString GetDescription() {
    if (cachedDescription.HasValue) {
      return cachedDescription.Value;
    }

    StringBuilder builder = new StringBuilder();

    // The type name, not the label: this is what -quickstart= on the command line takes.
    builder.AppendLine(
        "Quickstarts_Field_Type".Translate().Colorize(ColoredText.DateTimeColor) + GetType().Name);
    builder.AppendLine();
    builder.AppendLine(
        "Quickstarts_Field_MapSize".Translate().Colorize(ColoredText.TipSectionTitleColor) + $"{mapSize}x{mapSize}");
    builder.AppendLine(
        "Quickstarts_Field_Difficulty".Translate().Colorize(ColoredText.TipSectionTitleColor)
        + difficulty.LabelCap.ToString());
    builder.AppendLine(
        "Quickstarts_Field_Scenario".Translate().Colorize(ColoredText.TipSectionTitleColor)
        + (scenario?.LabelCap.ToString() ?? "None"));
    builder.AppendLine(
        "Quickstarts_Field_PauseAfterLoad".Translate().Colorize(ColoredText.TipSectionTitleColor)
        + (pauseAfterLoad ? "Yes" : "No"));
    builder.AppendLine();
    builder.AppendLine(description.Resolve());

    cachedDescription = builder.ToString();
    return cachedDescription.Value;
  }

  private TaggedString DefaultLabel() {
    string name = GetType().Name;
    if (name.Length > ClassSuffix.Length && name.EndsWith(ClassSuffix, StringComparison.Ordinal)) {
      name = name.Substring(0, name.Length - ClassSuffix.Length);
    }

    return GenText.SplitCamelCase(name);
  }
}
