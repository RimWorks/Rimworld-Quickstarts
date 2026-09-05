using System;
using System.Collections.Generic;

namespace RimWorks.Quickstarts.Verification;

/// <summary>One red error the game logged during a run.</summary>
public readonly struct CapturedError {
  /// <summary>The message text.</summary>
  public readonly string Text;

  /// <summary>How many times it repeated in a row. RimWorld stops counting at 99.</summary>
  public readonly int Repeats;

  /// <summary>Where it came from, or null when the game did not record one.</summary>
  public readonly string? StackTrace;

  /// <summary>Records one error.</summary>
  /// <param name="text">The message text.</param>
  /// <param name="repeats">How many times it repeated in a row.</param>
  /// <param name="stackTrace">Where it came from, or null.</param>
  public CapturedError(string text, int repeats, string? stackTrace) {
    Text = text;
    Repeats = repeats;
    StackTrace = stackTrace;
  }

  /// <summary>Whether any of a quickstart's ignore patterns appears in this message.</summary>
  /// <param name="patterns">Substrings to match, case insensitive. Blank entries never match.</param>
  /// <returns>True when the error should not count against the budget.</returns>
  public bool IsIgnoredBy(IEnumerable<string>? patterns) {
    if (patterns == null) {
      return false;
    }

    foreach (string pattern in patterns) {
      if (!string.IsNullOrWhiteSpace(pattern)
          && Text.IndexOf(pattern, StringComparison.OrdinalIgnoreCase) >= 0) {
        return true;
      }
    }

    return false;
  }
}
