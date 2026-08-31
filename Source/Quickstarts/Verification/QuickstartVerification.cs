using System;
using System.Collections.Generic;

namespace RimWorks.Quickstarts.Verification;

/// <summary>
/// Assertions a quickstart runs against the live game in CI mode. A throwing condition counts
/// as a failure, so a null reference in a check reports instead of taking the run down.
/// </summary>
public class QuickstartVerification {
  private readonly List<AssertResult> results = [];

  /// <summary>Every assertion that has run, in order.</summary>
  public IReadOnlyList<AssertResult> Results => results;

  /// <summary>Whether every assertion held.</summary>
  public bool AllPassed {
    get {
      for (int i = 0; i < results.Count; i++) {
        if (!results[i].Passed) {
          return false;
        }
      }

      return true;
    }
  }

  /// <summary>Asserts a condition is true.</summary>
  /// <param name="label">What is being checked.</param>
  /// <param name="condition">The check. Throwing counts as a failure.</param>
  public void Assert(string label, Func<bool> condition) {
    try {
      bool passed = condition();
      results.Add(new AssertResult(label, passed, passed ? null : "condition returned false"));
    } catch (Exception ex) {
      results.Add(new AssertResult(label, false, ex.GetType().Name + ": " + ex.Message));
    }
  }

  /// <summary>Asserts a value equals what was expected.</summary>
  /// <typeparam name="T">Value type being compared.</typeparam>
  /// <param name="label">What is being checked.</param>
  /// <param name="expected">The value that should come back.</param>
  /// <param name="actual">Produces the value. Throwing counts as a failure.</param>
  public void AssertEqual<T>(string label, T expected, Func<T> actual) {
    try {
      T value = actual();
      bool passed = EqualityComparer<T>.Default.Equals(expected, value);
      results.Add(new AssertResult(label, passed, passed ? null : $"expected {expected}, got {value}"));
    } catch (Exception ex) {
      results.Add(new AssertResult(label, false, ex.GetType().Name + ": " + ex.Message));
    }
  }
}
