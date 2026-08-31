using System.Collections.Generic;
using HarmonyLib;
using RimWorks.Quickstarts.Patching;
using UnityEngine;
using Verse;

namespace RimWorks.Quickstarts.Patches.Harmony;

/// <summary>
/// The mod's hooks expressed as Harmony patches. Registered at the lower priority, so Concord
/// wins when both libraries are active.
/// </summary>
[StaticConstructorOnStartup]
public class HarmonyBackend : IPatchBackend {
  static HarmonyBackend() {
    PatchBackends.Register(new HarmonyBackend(), PatchBackends.HarmonyPriority);
  }

  /// <inheritdoc/>
  public string Name => "Harmony";

  /// <summary>Postfix for <c>Root.OnGUI</c>.</summary>
  public static void RootOnGUIPostfix() {
    QuickstartHooks.AfterRootOnGUI();
  }

  /// <summary>Postfix for <c>DebugWindowsOpener.DrawButtons</c>.</summary>
  /// <param name="__instance">The toolbar being drawn.</param>
  public static void DrawButtonsPostfix(DebugWindowsOpener __instance) {
    QuickstartHooks.AfterDrawDebugButtons(__instance);
  }

  /// <summary>Prefix for <c>OptionListingUtility.DrawOptionListing</c>.</summary>
  /// <param name="rect">Where the listing draws. Unused, present to match the target.</param>
  /// <param name="optList">The option list, edited in place.</param>
  public static void DrawOptionListingPrefix(Rect rect, List<ListableOption> optList) {
    QuickstartHooks.BeforeDrawOptionListing(optList);
  }

  /// <inheritdoc/>
  public void Apply() {
    HarmonyLib.Harmony harmony = new HarmonyLib.Harmony("rimworks.quickstarts");

    harmony.Patch(
        typeof(Root).GetMethod(nameof(Root.OnGUI)),
        postfix: Handler(nameof(RootOnGUIPostfix)));

    harmony.Patch(
        AccessTools.Method(typeof(DebugWindowsOpener), "DrawButtons"),
        postfix: Handler(nameof(DrawButtonsPostfix)));

    harmony.Patch(
        typeof(OptionListingUtility).GetMethod(nameof(OptionListingUtility.DrawOptionListing)),
        prefix: Handler(nameof(DrawOptionListingPrefix)));
  }

  private static HarmonyMethod Handler(string name) {
    return new HarmonyMethod(typeof(HarmonyBackend).GetMethod(name));
  }
}
