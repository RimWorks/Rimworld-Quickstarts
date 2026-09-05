using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using RimWorks.Quickstarts.Verification;

namespace RimWorks.Quickstarts.Tests;

[TestClass]
public class LogSummaryTests {
  private static readonly CapturedError NullRef =
      new CapturedError("Tried to tick a null hediff", 4, "at Verse.Pawn.Tick");

  private static readonly CapturedError Missing =
      new CapturedError("Could not resolve cross-reference to Thing named Steel", 1, null);

  [TestMethod]
  public void CountsEveryErrorWhenNothingIsIgnored() {
    LogSummary log = new LogSummary([NullRef, Missing], 0, false, 0);

    Assert.AreEqual(2, log.CountAgainstBudget(null));
    Assert.AreEqual(2, log.CountAgainstBudget([]));
  }

  [TestMethod]
  public void SkipsErrorsMatchingAnIgnorePattern() {
    LogSummary log = new LogSummary([NullRef, Missing], 0, false, 0);

    Assert.AreEqual(1, log.CountAgainstBudget(["null hediff"]));
    Assert.AreEqual(0, log.CountAgainstBudget(["null hediff", "cross-reference"]));
  }

  [TestMethod]
  public void MatchesIgnorePatternsWithoutCase() {
    Assert.IsTrue(NullRef.IsIgnoredBy(["NULL HEDIFF"]));
    Assert.IsTrue(NullRef.IsIgnoredBy(["tried to tick"]));
  }

  [TestMethod]
  public void TreatsBlankPatternsAsNoPattern() {
    Assert.IsFalse(NullRef.IsIgnoredBy(["   "]));
    Assert.IsFalse(NullRef.IsIgnoredBy([string.Empty]));
  }

  [TestMethod]
  public void DoesNotMatchAnUnrelatedPattern() {
    Assert.IsFalse(NullRef.IsIgnoredBy(["something else entirely"]));
  }

  [TestMethod]
  public void CountsDistinctMessagesNotRepeats() {
    // NullRef repeated four times is still one error against the budget.
    LogSummary log = new LogSummary([NullRef], 0, false, 0);

    Assert.AreEqual(1, log.CountAgainstBudget(null));
  }

  [TestMethod]
  public void AnEmptySummaryCountsNothing() {
    Assert.AreEqual(0, LogSummary.None.CountAgainstBudget(null));
    Assert.AreEqual(0, LogSummary.None.Errors.Count);
    Assert.IsFalse(LogSummary.None.Truncated);
  }

  [TestMethod]
  public void KeepsTheFieldsItWasGiven() {
    List<CapturedError> errors = [NullRef];
    LogSummary log = new LogSummary(errors, 12, true, 3);

    Assert.AreEqual(12, log.Warnings);
    Assert.IsTrue(log.Truncated);
    Assert.AreEqual(3, log.PreLaunchErrors);
    Assert.AreEqual(4, log.Errors[0].Repeats);
  }
}
