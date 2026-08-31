using Verse;

namespace RimWorks.Quickstarts;

/// <summary>Persisted mod settings.</summary>
public sealed class QuickstartsSettings : ModSettings {
  /// <summary>Quickstart the game boots into in dev mode. Null or empty starts normally.</summary>
  public string? defaultQuickstart;

  /// <summary>ScenarioDef name that ScenarioTestQuickstart boots. Null falls back to Crashlanded.</summary>
  public string? testScenarioDefName;

  /// <summary>Whether the main menu's dev quicktest opens the picker instead of vanilla's map.</summary>
  public bool replaceQuicktestButton = true;

  /// <inheritdoc/>
  public override void ExposeData() {
    base.ExposeData();
    Scribe_Values.Look(ref defaultQuickstart, "defaultQuickstart");
    Scribe_Values.Look(ref testScenarioDefName, "testScenarioDefName");
    Scribe_Values.Look(ref replaceQuicktestButton, "replaceQuicktestButton", true);
  }
}
