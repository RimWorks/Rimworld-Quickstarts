using System;
using System.Reflection;
using RimWorks.Quickstarts.UI;
using RimWorld;
using Verse;

namespace RimWorks.Quickstarts;

/// <summary>
/// RimWorld's own <c>-quicktest</c> path, borrowed so the picker can offer it as one choice
/// among the quickstarts.
/// </summary>
public static class VanillaQuicktest {
  private static bool pickerPending;

  /// <summary>
  /// Runs inside the launch, after the quicktest game and world exist and before the map page
  /// starts. A mod that has to touch the game before the map generates subscribes here.
  /// </summary>
  public static event Action? Configuring;

  /// <summary>
  /// What the picker's vanilla row does. Replace it to insert a step of your own, such as a
  /// second dialog; call <see cref="Start"/> when that step is done.
  /// </summary>
  public static Action RowAction { get; set; } = Start;

  /// <summary>
  /// Claims the <c>-quicktest</c> arg so the picker can be shown on the main menu instead of
  /// the game dropping straight into a map.
  /// </summary>
  /// <remarks>
  /// Done by flipping QuickStarter's own guard rather than by patching CheckQuickStart:
  /// UIRoot_Entry.Init runs in the InitializingInterface long event, which Root.Start queues
  /// before the mod constructor ever queues the event that applies this mod's patches. A patch
  /// on that method would be composed several events too late to matter.
  /// </remarks>
  public static void ClaimCommandLineArg() {
    if (!QuickstartsMod.Settings.replaceQuicktestButton || !GenCommandLine.CommandLineArgPassed("quicktest")) {
      return;
    }

    FieldInfo? guard = typeof(QuickStarter).GetField("quickStarted", BindingFlags.Static | BindingFlags.NonPublic);
    if (guard == null) {
      Logger.Warning("QuickStarter.quickStarted is gone; -quicktest will load vanilla's map, not the picker.");
      return;
    }

    guard.SetValue(null, true);
    pickerPending = true;
  }

  /// <summary>
  /// Opens the picker on the first main menu frame after the arg was claimed. Deferred this far
  /// because the window stack does not exist yet when a static constructor runs.
  /// </summary>
  public static void ShowPickerIfPending() {
    if (!pickerPending || Find.WindowStack == null) {
      return;
    }

    pickerPending = false;
    Dialog_QuicktestPicker.Open();
  }

  /// <summary>Runs vanilla's quicktest setup, firing <see cref="Configuring"/> along the way.</summary>
  public static void Start() {
    LongEventHandler.QueueLongEvent(
        () => {
          Root_Play.SetupForQuickTestPlay();

          // SetupForQuickTestPlay builds the game and generates the world in one call, so this
          // is the first point a subscriber can see a complete game.
          try {
            Configuring?.Invoke();
          } catch (Exception ex) {
            Logger.Error($"A vanilla quicktest subscriber threw: {ex}");
          }

          PageUtility.InitGameStart();
        },
        "GeneratingMap",
        true,
        GameAndMapInitExceptionHandlers.ErrorWhileGeneratingMap);
  }
}
