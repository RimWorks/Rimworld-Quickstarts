using System.Linq;
using System.Xml.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using RimWorks.Quickstarts.Verification;

namespace RimWorks.Quickstarts.Tests;

[TestClass]
public class JUnitReportTests {
  [TestMethod]
  public void APassingRunHasNoFailureElements() {
    string xml = JUnitReport.Build(
        "TinyColony", Verification(("colonists spawned", true), ("map exists", true)),
        LogSummary.None, null, null);

    XElement suite = Suite(xml);
    Assert.AreEqual("2", suite.Attribute("tests")!.Value);
    Assert.AreEqual("0", suite.Attribute("failures")!.Value);
    Assert.AreEqual(0, suite.Descendants("failure").Count());
  }

  [TestMethod]
  public void AFailedAssertionCarriesItsDetail() {
    string xml = JUnitReport.Build(
        "TinyColony", Verification(("colonists spawned", false)), LogSummary.None, null, null);

    XElement failure = Suite(xml).Descendants("failure").Single();
    Assert.AreEqual("condition returned false", failure.Attribute("message")!.Value);
    Assert.AreEqual("1", Suite(xml).Attribute("failures")!.Value);
  }

  [TestMethod]
  public void TheLogBudgetBecomesItsOwnTestcase() {
    AssertResult logCheck = new AssertResult("no log errors", false, "3 errors, budget 0");
    string xml = JUnitReport.Build(
        "TinyColony", Verification(("map exists", true)), LogSummary.None, logCheck, null);

    XElement suite = Suite(xml);
    Assert.AreEqual("2", suite.Attribute("tests")!.Value);
    Assert.AreEqual("1", suite.Attribute("failures")!.Value);
    Assert.AreEqual(
        "3 errors, budget 0", suite.Descendants("failure").Single().Attribute("message")!.Value);
  }

  [TestMethod]
  public void ATimeoutBecomesAnErrorNotAFailure() {
    string xml = JUnitReport.Build("TinyColony", null, LogSummary.None, null, "generating-map");

    XElement suite = Suite(xml);
    Assert.AreEqual("1", suite.Attribute("errors")!.Value);
    Assert.AreEqual("0", suite.Attribute("failures")!.Value);
    Assert.AreEqual(
        "timed out during generating-map",
        suite.Descendants("error").Single().Attribute("message")!.Value);
  }

  [TestMethod]
  public void HostileTextRoundTripsThroughAParser() {
    string label = "pawn <name> is \"alive\" & well, o'clock";
    string xml = JUnitReport.Build(
        "Tiny<&>Colony", Verification((label, true)), LogSummary.None, null, null);

    XElement suite = Suite(xml);
    Assert.AreEqual("Tiny<&>Colony", suite.Attribute("name")!.Value);
    Assert.AreEqual(label, suite.Descendants("testcase").Single().Attribute("name")!.Value);
  }

  [TestMethod]
  public void ControlBytesAreDroppedSoTheFileStillParses() {
    string label = "before\u0001after\u0007\ttabbed\nwrapped";
    string xml = JUnitReport.Build(
        "TinyColony", Verification((label, true)), LogSummary.None, null, null);

    // Parsing at all is the assertion: XML 1.0 rejects these bytes even when encoded.
    Assert.AreEqual(
        "beforeafter\ttabbed\nwrapped",
        Suite(xml).Descendants("testcase").Single().Attribute("name")!.Value);
  }

  [TestMethod]
  public void CapturedErrorsLandInSystemErr() {
    LogSummary log = new LogSummary(
        [new CapturedError("Tried to tick a <null> hediff", 4, null)], 0, false, 0);
    string xml = JUnitReport.Build("TinyColony", null, log, null, null);

    string text = Suite(xml).Element("system-err")!.Value;
    StringAssert.Contains(text, "Tried to tick a <null> hediff");
    StringAssert.Contains(text, "(x4)");
  }

  [TestMethod]
  public void ARunWithNoVerifyStillProducesValidXml() {
    string xml = JUnitReport.Build("TinyColony", null, LogSummary.None, null, null);

    XElement suite = Suite(xml);
    Assert.AreEqual("0", suite.Attribute("tests")!.Value);
    Assert.AreEqual(0, suite.Descendants("testcase").Count());
  }

  private static QuickstartVerification Verification(params (string Label, bool Passed)[] asserts) {
    QuickstartVerification verification = new QuickstartVerification();
    foreach ((string label, bool passed) in asserts) {
      verification.Assert(label, () => passed);
    }

    return verification;
  }

  private static XElement Suite(string xml) {
    return XDocument.Parse(xml).Root!.Element("testsuite")!;
  }
}
