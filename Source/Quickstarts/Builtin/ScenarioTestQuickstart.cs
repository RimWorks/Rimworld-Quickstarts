using RimWorks.Quickstarts.Verification;
using RimWorld;
using Verse;

namespace RimWorks.Quickstarts.Builtin;

/// <summary>
/// Boots whichever scenario the mod settings name, with no pawn overrides. The one quickstart
/// that ships here, because "does this scenario start at all" is the check every mod needs.
/// </summary>
public class ScenarioTestQuickstart : AbstractQuickstart {
  /// <inheritdoc/>
  public override TaggedString description =>
      "Starts the scenario selected in Quickstarts mod settings, with default pawn generation.";

  /// <inheritdoc/>
  public override ScenarioDef scenario {
    get {
      string? defName = QuickstartsMod.Settings.testScenarioDefName;
      if (string.IsNullOrEmpty(defName)) {
        return ScenarioDefOf.Crashlanded;
      }

      ScenarioDef? selected = DefDatabase<ScenarioDef>.GetNamedSilentFail(defName);
      if (selected != null) {
        return selected;
      }

      Logger.Warn(
          "ScenarioTestQuickstart: ScenarioDef '{DefName}' not found, falling back to Crashlanded.",
          new object?[] { defName });
      return ScenarioDefOf.Crashlanded;
    }
  }

  /// <inheritdoc/>
  public override QuickstartVerification Verify() {
    QuickstartVerification verification = new QuickstartVerification();
    verification.Assert("a game exists", () => Current.Game != null);
    verification.Assert("a player home map exists", () => Find.CurrentMap is { IsPlayerHome: true });
    verification.Assert(
        "at least one colonist spawned",
        () => Find.CurrentMap?.mapPawns?.FreeColonistsSpawnedCount > 0);
    return verification;
  }
}
