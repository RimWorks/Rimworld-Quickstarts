using System.Text.Json;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using RimWorks.Quickstarts.Verification;

namespace Quickstarts.Tests;

[TestClass]
public class VerificationReportTests {
  private static JsonElement Parse(string json) => JsonDocument.Parse(json).RootElement;

  [TestMethod]
  public void EmptyRunIsValidJson() {
    string json = VerificationReport.Build("Basic", "seed-1", 250, null, LogSummary.None, true, null);

    JsonElement root = Parse(json);
    Assert.AreEqual("Basic", root.GetProperty("quickstart").GetString());
    Assert.AreEqual("seed-1", root.GetProperty("seed").GetString());
    Assert.AreEqual(250, root.GetProperty("ticksRun").GetInt32());
    Assert.IsTrue(root.GetProperty("passed").GetBoolean());
    Assert.IsFalse(root.GetProperty("timedOut").GetBoolean());
    Assert.AreEqual(JsonValueKind.Null, root.GetProperty("stage").ValueKind);
    Assert.AreEqual(0, root.GetProperty("total").GetInt32());
    Assert.AreEqual(0, root.GetProperty("results").GetArrayLength());
    Assert.AreEqual(0, root.GetProperty("errors").GetArrayLength());
  }

  [TestMethod]
  public void TimedOutRunNamesTheStage() {
    string json = VerificationReport.Build("Basic", "s", 0, null, LogSummary.None, false, "map-generation");

    JsonElement root = Parse(json);
    Assert.IsTrue(root.GetProperty("timedOut").GetBoolean());
    Assert.AreEqual("map-generation", root.GetProperty("stage").GetString());
    Assert.IsFalse(root.GetProperty("passed").GetBoolean());
  }

  // The escaper is hand-written, so the values that break it go through a real JSON parser.
  [TestMethod]
  public void HostileTextSurvivesARoundTrip() {
    const string nasty = "quote \" backslash \\ newline \n tab \t return \r bell  unicode é中";
    QuickstartVerification verification = new QuickstartVerification();
    verification.Assert(nasty, () => false);

    LogSummary log = new LogSummary([new CapturedError(nasty, 3, nasty)], 2, true, 1);
    string json = VerificationReport.Build(nasty, nasty, 1, verification, log, false, nasty);

    JsonElement root = Parse(json);
    Assert.AreEqual(nasty, root.GetProperty("quickstart").GetString());
    Assert.AreEqual(nasty, root.GetProperty("seed").GetString());
    Assert.AreEqual(nasty, root.GetProperty("stage").GetString());
    Assert.AreEqual(nasty, root.GetProperty("results")[0].GetProperty("label").GetString());
    Assert.AreEqual(nasty, root.GetProperty("errors")[0].GetProperty("text").GetString());
    Assert.AreEqual(nasty, root.GetProperty("errors")[0].GetProperty("stackTrace").GetString());
  }

  [TestMethod]
  public void CountsMatchTheResultsAndErrorsWritten() {
    QuickstartVerification verification = new QuickstartVerification();
    verification.Assert("passes", () => true);
    verification.Assert("fails", () => false);
    verification.Assert("also fails", () => false);

    LogSummary log = new LogSummary(
        [new CapturedError("boom", 1, null), new CapturedError("bang", 5, "at Foo()")], 7, true, 4);
    string json = VerificationReport.Build("Basic", "s", 12, verification, log, false, null);

    JsonElement root = Parse(json);
    Assert.AreEqual(3, root.GetProperty("total").GetInt32());
    Assert.AreEqual(2, root.GetProperty("failed").GetInt32());
    Assert.AreEqual(3, root.GetProperty("results").GetArrayLength());
    Assert.AreEqual(2, root.GetProperty("logErrors").GetInt32());
    Assert.AreEqual(2, root.GetProperty("errors").GetArrayLength());
    Assert.AreEqual(7, root.GetProperty("logWarnings").GetInt32());
    Assert.IsTrue(root.GetProperty("logTruncated").GetBoolean());
    Assert.AreEqual(4, root.GetProperty("preLaunchErrors").GetInt32());
    Assert.AreEqual(JsonValueKind.Null, root.GetProperty("errors")[0].GetProperty("stackTrace").ValueKind);
    Assert.AreEqual(5, root.GetProperty("errors")[1].GetProperty("repeats").GetInt32());
  }
}
