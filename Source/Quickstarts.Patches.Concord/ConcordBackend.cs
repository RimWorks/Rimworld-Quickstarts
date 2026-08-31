using Concord;
using RimWorks.Quickstarts.Patching;
using Verse;

namespace RimWorks.Quickstarts.Patches.Concord;

/// <summary>
/// The mod's hooks expressed as Concord injections. Registered above Harmony, so Concord is
/// used whenever it is available.
/// </summary>
[StaticConstructorOnStartup]
public class ConcordBackend : IPatchBackend {
  static ConcordBackend() {
    PatchBackends.Register(new ConcordBackend(), PatchBackends.ConcordPriority);
  }

  /// <inheritdoc/>
  public string Name => "Concord";

  /// <inheritdoc/>
  public void Apply() {
    Patcher.Apply(typeof(ConcordBackend).Assembly);
  }
}
