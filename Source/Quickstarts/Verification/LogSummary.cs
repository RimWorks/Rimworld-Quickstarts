using System.Collections.Generic;

namespace RimWorks.Quickstarts.Verification;

/// <summary>What the game's own log held during one run.</summary>
public readonly struct LogSummary {
  /// <summary>Red errors logged after the quickstart launched.</summary>
  public readonly IReadOnlyList<CapturedError> Errors;

  /// <summary>Warnings logged after the quickstart launched.</summary>
  public readonly int Warnings;

  /// <summary>Whether the message queue filled up, so older lines were dropped.</summary>
  public readonly bool Truncated;

  /// <summary>Red errors already present before launch, from mod and def loading.</summary>
  public readonly int PreLaunchErrors;

  /// <summary>Records one run's log.</summary>
  /// <param name="errors">Red errors logged after launch.</param>
  /// <param name="warnings">Warnings logged after launch.</param>
  /// <param name="truncated">Whether the queue dropped older lines.</param>
  /// <param name="preLaunchErrors">Red errors from before launch.</param>
  public LogSummary(
      IReadOnlyList<CapturedError> errors, int warnings, bool truncated, int preLaunchErrors) {
    Errors = errors;
    Warnings = warnings;
    Truncated = truncated;
    PreLaunchErrors = preLaunchErrors;
  }

  /// <summary>An empty log, for runs that never armed the capture.</summary>
  public static LogSummary None => new LogSummary([], 0, false, 0);

  /// <summary>Counts the errors a quickstart has not asked to ignore.</summary>
  /// <param name="ignored">Substrings the quickstart tolerates.</param>
  /// <returns>How many errors count against the budget.</returns>
  public int CountAgainstBudget(IEnumerable<string>? ignored) {
    int counted = 0;
    for (int i = 0; i < Errors.Count; i++) {
      if (!Errors[i].IsIgnoredBy(ignored)) {
        counted++;
      }
    }

    return counted;
  }
}
