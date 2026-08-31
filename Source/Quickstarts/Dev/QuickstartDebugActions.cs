using LudeonTK;
using RimWorks.Quickstarts.UI;
using Verse;

namespace RimWorks.Quickstarts.Dev;

/// <summary>Dev-menu entries for relaunching without restarting the game.</summary>
public static class QuickstartDebugActions {
  /// <summary>Rebuilds the game from the configured quickstart.</summary>
  [DebugAction("Quickstarts", "Reload quickstart", allowedGameStates = AllowedGameStates.Entry)]
  public static void ReloadQuickstart() {
    Quickstarter.ReloadQuickstart();
  }

  /// <summary>Opens the picker so another quickstart can be launched.</summary>
  [DebugAction("Quickstarts", "Pick quickstart", allowedGameStates = AllowedGameStates.Entry)]
  public static void PickQuickstart() {
    Dialog_QuicktestPicker.Open();
  }
}
