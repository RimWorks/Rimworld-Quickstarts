using System.Collections.Generic;
using Concord;
using RimWorks.Quickstarts.Patching;
using UnityEngine;
using Verse;

namespace RimWorks.Quickstarts.Patches.Concord;

/// <summary>
/// Swaps the main menu's dev quicktest entry for the picker. Rewrites the option's action
/// instead of transpiling MainMenuDrawer, so a menu layout change cannot drop the hook.
/// </summary>
[Patch(typeof(OptionListingUtility))]
public static class MainMenuOptionPatch {
  [Inject(At.Head, nameof(OptionListingUtility.DrawOptionListing))]
  private static void BeforeDrawOptionListing(Rect rect, List<ListableOption> optList) {
    QuickstartHooks.BeforeDrawOptionListing(optList);
  }
}
