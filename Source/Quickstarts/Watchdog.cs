using System.Diagnostics;

namespace RimWorks.Quickstarts;

/// <summary>
/// Wall-clock deadline for a CI run. Checked from Root.OnGUI, so it catches a slow or stuck run
/// as long as the main thread still draws. A real deadlock needs the timeout in the shell script.
/// </summary>
internal static class Watchdog {
  private static readonly Stopwatch Clock = new Stopwatch();

  private static int limitSeconds;
  private static bool fired;

  internal static string Stage { get; set; } = "starting";

  internal static void Arm(int seconds) {
    if (seconds <= 0) {
      return;
    }

    limitSeconds = seconds;
    fired = false;
    Clock.Restart();
  }

  // True exactly once, so the caller can quit without racing the next frame.
  internal static bool Expired() {
    if (fired || limitSeconds <= 0 || Clock.Elapsed.TotalSeconds < limitSeconds) {
      return false;
    }

    fired = true;
    return true;
  }
}
