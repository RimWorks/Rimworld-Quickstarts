using Verse;

namespace RimWorks.Quickstarts.Builtin;

/// <summary>The Sanguophage colony with RimWorld's performance overlays already switched on.</summary>
public class ProfilingQuickstart : SanguophageQuickstart {
  /// <inheritdoc/>
  public override TaggedString description =>
      "The Sanguophage start with the TPS, FPS and memory overlays on, for profiling runs.";

  /// <summary>A profiling run wants ticks flowing the moment the map is ready.</summary>
  public override bool pauseAfterLoad => false;

  /// <inheritdoc/>
  public override void PostLoaded() {
    base.PostLoaded();
    ShowPerformanceOverlays();
  }

  // PostLoaded is an instance override, so the static writes live in their own method (S2696).
  private static void ShowPerformanceOverlays() {
    DebugViewSettings.showTpsCounter = true;
    DebugViewSettings.showFpsCounter = true;
    DebugViewSettings.showMemoryInfo = true;
  }
}
