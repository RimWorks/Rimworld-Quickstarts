using System.Collections.Generic;
using Concord;
using RimWorks.Quickstarts.Patching;
using UnityEngine;
using Verse;

namespace RimWorks.Quickstarts.Patches.Concord;

/// <summary>
/// Swaps the main menu's dev quicktest entry for the picker. Done by rewriting the option's
/// action rather than transpiling MainMenuDrawer, so a vanilla layout change to the menu
/// cannot silently drop the hook.
/// </summary>
[Patch(typeof(OptionListingUtility))]
public static class MainMenuOptionPatch {
  [Inject(At.Head, nameof(OptionListingUtility.DrawOptionListing))]
  private static void BeforeDrawOptionListing(Rect rect, List<ListableOption> optList) {
    QuickstartHooks.BeforeDrawOptionListing(optList);
  }
}
