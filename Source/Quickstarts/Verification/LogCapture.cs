using System.Collections.Generic;
using Verse;

namespace RimWorks.Quickstarts.Verification;

/// <summary>
/// Reads the game's own log so a run that boots fine but spews red errors still fails. Verse.Log
/// exposes the queue directly, so nothing here patches anything.
/// </summary>
public static class LogCapture {
  // LogMessageQueue.maxMessages. Past this the oldest line is dropped and the count is a floor.
  private const int QueueCapacity = 1000;

  private static int preLaunchErrors;
  private static bool armed;

  /// <summary>Clears the log so everything after this point belongs to the run.</summary>
  public static void Arm() {
    preLaunchErrors = CountErrors();
    Log.Clear();
    armed = true;
  }

  /// <summary>Reads back everything logged since <see cref="Arm"/>.</summary>
  /// <returns>The run's errors and warnings, or an empty summary when nothing armed the capture.</returns>
  public static LogSummary Collect() {
    if (!armed) {
      return LogSummary.None;
    }

    List<CapturedError> errors = [];
    int warnings = 0;
    int total = 0;

    foreach (LogMessage message in Log.Messages) {
      total++;
      if (message.type == LogMessageType.Error) {
        errors.Add(new CapturedError(message.text, message.repeats, message.StackTrace));
      } else if (message.type == LogMessageType.Warning) {
        warnings++;
      }
    }

    return new LogSummary(errors, warnings, total >= QueueCapacity, preLaunchErrors);
  }

  private static int CountErrors() {
    int count = 0;
    foreach (LogMessage message in Log.Messages) {
      if (message.type == LogMessageType.Error) {
        count++;
      }
    }

    return count;
  }
}
