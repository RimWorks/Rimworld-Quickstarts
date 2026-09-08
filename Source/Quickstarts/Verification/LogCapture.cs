using System;
using System.Collections.Generic;
using RimWorks.RimLogging;
using RimWorks.RimLogging.Sinks;
using Verse;

namespace RimWorks.Quickstarts.Verification;

/// <summary>
/// Reads the game's own log so a run that boots fine but spews red errors still fails. A run
/// that cannot read the log says so, because no errors and no way to see them are not the same.
/// </summary>
public static class LogCapture {
  // Ring capacity. Past this the oldest entry is dropped and the count is a floor.
  private const int SinkCapacity = 1000;

  private static MemoryLogSink? sink;
  private static int preLaunchErrors;
  private static bool captureLive;
  private static DateTime armedAt;

  /// <summary>Starts a fresh sink so everything after this point belongs to the run.</summary>
  public static void Arm() {
    preLaunchErrors = CountErrors();
    if (sink != null) {
      Logging.RemoveSink(sink);
    }

    captureLive = CanSeeErrors();
    if (!captureLive) {
      // Verse.Log still works when the hijack is not ours, so the warning gets out this way.
#pragma warning disable RIMLOG002 // RimLogging is the thing that is deaf; routing through it loses this.
      Verse.Log.Error(
          "[Quickstarts] Log capture is blind: RimLogging cannot deliver errors to this mod. This"
          + " run cannot prove the game logged nothing, so the log check will fail.");
#pragma warning restore RIMLOG002
    }

    armedAt = DateTime.UtcNow;
    sink = new MemoryLogSink(SinkCapacity, LogLevel.Warn);
    Logging.RegisterSink(sink);
  }

  /// <summary>Reads back everything logged since <see cref="Arm"/>.</summary>
  /// <returns>The run's errors and warnings. A summary nothing armed reads as blind, not clean.</returns>
  public static LogSummary Collect() {
    if (sink == null) {
      return LogSummary.None;
    }

    List<CapturedError> errors = [];
    int warnings = 0;
    int total = 0;

    foreach (RimLogging.LogEntry entry in sink.Entries) {
      // Registering a sink replays the log so far into it, so anything older is not this run's.
      if (entry.Timestamp < armedAt) {
        continue;
      }

      total++;
      if (entry.Level >= LogLevel.Error) {
        errors.Add(new CapturedError(entry.RenderedMessage, entry.Repeats, entry.StackTrace));
      } else if (entry.Level == LogLevel.Warn) {
        warnings++;
      }
    }

    return new LogSummary(errors, warnings, total >= SinkCapacity, preLaunchErrors, captureLive);
  }

  // Three ways to go deaf: another copy owns the hijack, nothing patched Verse.Log at all, or a
  // min level above Error drops the entries the gate reads before any sink sees them.
  private static bool CanSeeErrors() {
    return Logging.IsPrimary
        && Logging.CaptureBackend != null
        && Logging.GlobalMinLevel <= LogLevel.Error;
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
