using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using RimWorks.Quickstarts.Verification;

namespace RimWorks.Quickstarts.Tests;

[TestClass]
public class QuickstartVerificationTests {
  [TestMethod]
  public void AFreshVerificationHasNothingToFail() {
    QuickstartVerification verification = new QuickstartVerification();

    Assert.IsTrue(verification.AllPassed);
    Assert.IsEmpty(verification.Results);
  }

  [TestMethod]
  public void PassingChecksKeepTheRunGreen() {
    QuickstartVerification verification = new QuickstartVerification();
    verification.Assert("map exists", () => true);
    verification.AssertEqual("colonists", 3, () => 3);

    Assert.IsTrue(verification.AllPassed);
    Assert.AreEqual(2, verification.Results.Count);
    Assert.IsNull(verification.Results[0].Detail);
  }

  [TestMethod]
  public void AConditionThatReturnsFalseFailsTheRun() {
    QuickstartVerification verification = new QuickstartVerification();
    verification.Assert("map exists", () => true);
    verification.Assert("colonists spawned", () => false);

    Assert.IsFalse(verification.AllPassed);
    Assert.AreEqual("condition returned false", verification.Results[1].Detail);
  }

  [TestMethod]
  public void AThrowingConditionReportsInsteadOfTakingTheRunDown() {
    QuickstartVerification verification = new QuickstartVerification();
    verification.Assert("map exists", () => throw new InvalidOperationException("no map"));

    Assert.IsFalse(verification.AllPassed);
    Assert.AreEqual("InvalidOperationException: no map", verification.Results[0].Detail);
  }

  [TestMethod]
  public void AWrongValueReportsBothSides() {
    QuickstartVerification verification = new QuickstartVerification();
    verification.AssertEqual("colonists", 3, () => 1);

    Assert.IsFalse(verification.AllPassed);
    Assert.AreEqual("expected 3, got 1", verification.Results[0].Detail);
  }

  [TestMethod]
  public void AThrowingValueReportsInsteadOfTakingTheRunDown() {
    QuickstartVerification verification = new QuickstartVerification();
    verification.AssertEqual<int>(
        "colonists", 3, () => throw new InvalidOperationException("no map"));

    Assert.IsFalse(verification.AllPassed);
    Assert.AreEqual("InvalidOperationException: no map", verification.Results[0].Detail);
  }
}
