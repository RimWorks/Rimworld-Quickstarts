using Concord;
using RimWorks.Quickstarts.Patching;
using Verse;

namespace RimWorks.Quickstarts.Patches.Concord;

/// <summary>Draws the status box and the pending picker at the end of every root frame.</summary>
[Patch]
public abstract class RootOnGUIPatch : Root {
  [Inject(At.Return, nameof(OnGUI))]
  private void AfterOnGUI() {
    QuickstartHooks.AfterRootOnGUI();
  }
}
