using Concord;
using RimWorks.Quickstarts.Patching;
using Verse;

namespace RimWorks.Quickstarts.Patches.Concord;

/// <summary>Appends the reload button to the dev toolbar.</summary>
[Patch]
public abstract class DebugButtonsPatch : DebugWindowsOpener {
  [Inject(At.Tail, "DrawButtons")]
  private void AfterDrawButtons() {
    QuickstartHooks.AfterDrawDebugButtons(this);
  }
}
