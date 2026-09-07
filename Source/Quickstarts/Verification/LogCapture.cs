using System.Collections.Generic;
using RimWorks.RimLogging;
using RimWorks.RimLogging.Sinks;
using Verse;

namespace RimWorks.Quickstarts.Verification;

/// <summary>
/// Reads the game's own log so a run that boots fine but spews red errors still fails. Entries
/// come from a RimLogging sink, because RimLogging rewrites what reaches Verse's own buffer.
/// </summary>
public static class LogCapture {
  // Ring capacity. Past this the oldest entry is dropped and the count is a floor.
  private const int SinkCapacity = 1000;

  private static MemoryLogSink? sink;
  private static int preLaunchErrors;

  /// <summary>Starts a fresh sink so everything after this point belongs to the run.</summary>
  public static void Arm() {
    preLaunchErrors = CountErrors();
    if (sink != null) {
      Logging.RemoveSink(sink);
    }

    sink = new MemoryLogSink(SinkCapacity, LogLevel.Warn);
    Logging.RegisterSink(sink);
  }

  /// <summary>Reads back everything logged since <see cref="Arm"/>.</summary>
  /// <returns>The run's errors and warnings, or an empty summary when nothing armed the capture.</returns>
  public static LogSummary Collect() {
    if (sink == null) {
      return LogSummary.None;
    }

    List<CapturedError> errors = [];
    int warnings = 0;
    int total = 0;

    foreach (RimLogging.LogEntry entry in sink.Entries) {
      total++;
      if (entry.Level >= LogLevel.Error) {
        errors.Add(new CapturedError(entry.RenderedMessage, entry.Repeats, entry.StackTrace));
      } else if (entry.Level == LogLevel.Warn) {
        warnings++;
      }
    }

    return new LogSummary(errors, warnings, total >= SinkCapacity, preLaunchErrors);
  }

  // Boot errors land before the sink exists, so this still counts Verse's buffer. Only the
  // count is used, and the rewrite does not change an entry's type.
  private static int CountErrors() {
    int count = 0;
    foreach (LogMessage message in Verse.Log.Messages) {
      if (message.type == LogMessageType.Error) {
        count++;
      }
    }

    return count;
  }
}
