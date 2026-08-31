using Verse;

namespace RimWorks.Quickstarts;

/// <summary>
/// Log wrapper that tags every line, so a quickstart run is one grep away in Player.log.
/// </summary>
public static class Logger {
  private const string Prefix = "[Quickstarts] ";

  /// <summary>Writes an informational line.</summary>
  /// <param name="message">Text to log.</param>
  public static void Message(string message) {
    Log.Message(Prefix + message);
  }

  /// <summary>Writes a warning line.</summary>
  /// <param name="message">Text to log.</param>
  public static void Warning(string message) {
    Log.Warning(Prefix + message);
  }

  /// <summary>Writes an error line.</summary>
  /// <param name="message">Text to log.</param>
  public static void Error(string message) {
    Log.Error(Prefix + message);
  }
}
