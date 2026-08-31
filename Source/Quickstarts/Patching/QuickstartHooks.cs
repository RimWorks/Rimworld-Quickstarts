using System;
using System.Collections.Generic;
using System.Reflection;
using RimWorks.Quickstarts.UI;
using Verse;

namespace RimWorks.Quickstarts.Patching;

/// <summary>
/// What the hooks actually do. Backends are thin wrappers over these, so behaviour cannot
/// drift between Harmony and Concord.
/// </summary>
public static class QuickstartHooks {
  private const string ReloadTooltip = "Reload the configured quickstart.";

  private static readonly FieldInfo? WidgetRowField =
      typeof(DebugWindowsOpener).GetField("widgetRow", BindingFlags.Instance | BindingFlags.NonPublic);

  private static readonly FieldInfo? WidgetRowFinalXField =
      typeof(DebugWindowsOpener).GetField("widgetRowFinalX", BindingFlags.Instance | BindingFlags.NonPublic);

  // One instance, so the already-swapped check below is a reference comparison and the
  // main menu does not allocate a delegate every frame it draws.
  private static readonly Action OpenPicker = Dialog_QuicktestPicker.Open;

  private static bool loggedMissingField;

  /// <summary>Runs at the end of <c>Root.OnGUI</c>: draws the launch status box and the picker.</summary>
  public static void AfterRootOnGUI() {
    Quickstarter.Instance?.OnGUI();
    VanillaQuicktest.ShowPickerIfPending();
  }

  /// <summary>
  /// Runs at the end of <c>DebugWindowsOpener.DrawButtons</c>: appends a reload button to the
  /// dev toolbar.
  /// </summary>
  /// <param name="opener">The toolbar being drawn.</param>
  public static void AfterDrawDebugButtons(DebugWindowsOpener opener) {
    if (WidgetRowField?.GetValue(opener) is not WidgetRow row) {
      WarnOnce("DebugWindowsOpener.widgetRow is gone; the toolbar reload button is off this run.");
      return;
    }

    if (row.ButtonIcon(TexButton.Reload, ReloadTooltip)) {
      Quickstarter.ReloadQuickstart();
    }

    // DrawButtons publishes the row's end for click-through blocking, and it already ran.
    WidgetRowFinalXField?.SetValue(opener, row.FinalX);
  }

  /// <summary>
  /// Runs before the main menu draws its options: points the dev quicktest entry at the picker.
  /// </summary>
  /// <param name="options">The menu's option list, edited in place.</param>
  public static void BeforeDrawOptionListing(List<ListableOption> options) {
    if (!Prefs.DevMode || Current.ProgramState != ProgramState.Entry) {
      return;
    }

    if (!QuickstartsMod.Settings.replaceQuicktestButton) {
      return;
    }

    string label = "DevQuickTest".Translate();
    for (int i = 0; i < options.Count; i++) {
      if (options[i].label == label && options[i].action != OpenPicker) {
        options[i].action = OpenPicker;
      }
    }
  }

  private static void WarnOnce(string message) {
    if (loggedMissingField) {
      return;
    }

    loggedMissingField = true;
    Logger.Warning(message);
  }
}
