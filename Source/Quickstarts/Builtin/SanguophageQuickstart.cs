using System.Collections.Generic;
using RimWorks.Quickstarts.Verification;
using RimWorld;
using Verse;

namespace RimWorks.Quickstarts.Builtin;

/// <summary>Biotech's Sanguophage start, and the sample to copy for a scenario-pinned quickstart.</summary>
public class SanguophageQuickstart : AbstractQuickstart {
  private const string ScenarioName = "Sanguophage";
  private const string XenotypeName = "Sanguophage";

  /// <inheritdoc/>
  public override TaggedString description =>
      "Biotech's Sanguophage start: one sanguophage and one baseliner, dropped in on a rough map.";

  /// <inheritdoc/>
  public override int mapSize => 150;

  /// <inheritdoc/>
  public override DifficultyDef difficulty => DifficultyDefOf.Rough;

  /// <inheritdoc/>
  public override ScenarioDef scenario {
    get {
      ScenarioDef? sanguophage = DefDatabase<ScenarioDef>.GetNamedSilentFail(ScenarioName);
      if (sanguophage != null) {
        return sanguophage;
      }

      Logger.Warning($"SanguophageQuickstart: no '{ScenarioName}' scenario (Biotech off?); using Crashlanded.");
      return ScenarioDefOf.Crashlanded;
    }
  }

  /// <inheritdoc/>
  public override QuickstartVerification Verify() {
    QuickstartVerification verification = new QuickstartVerification();
    verification.Assert("a player home map exists", () => Find.CurrentMap is { IsPlayerHome: true });
    verification.AssertEqual("two colonists spawned", 2, () => Find.CurrentMap.mapPawns.FreeColonistsSpawnedCount);
    verification.AssertEqual("one of them is a sanguophage", 1, CountSanguophages);
    return verification;
  }

  // Compares defName instead of XenotypeDefOf, which cannot resolve without Biotech loaded.
  private static int CountSanguophages() {
    List<Pawn> colonists = Find.CurrentMap.mapPawns.FreeColonistsSpawned;
    int found = 0;
    for (int i = 0; i < colonists.Count; i++) {
      if (colonists[i].genes?.Xenotype?.defName == XenotypeName) {
        found++;
      }
    }

    return found;
  }
}
