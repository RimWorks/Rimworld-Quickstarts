using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using RimWorks.Quickstarts;

namespace RimWorks.Quickstarts.Tests;

[TestClass]
public class WatchdogTests {
  [TestMethod]
  public void ARunWithNoDeadlineNeverExpires() {
    Watchdog.Arm(0);

    Assert.IsFalse(Watchdog.Expired());
  }

  [TestMethod]
  public void ANegativeDeadlineNeverExpires() {
    Watchdog.Arm(-5);

    Assert.IsFalse(Watchdog.Expired());
  }

  [TestMethod]
  public async Task ExpiresOnceOncePastTheDeadline() {
    Watchdog.Arm(1);
    Assert.IsFalse(Watchdog.Expired());

    await Task.Delay(1100);

    Assert.IsTrue(Watchdog.Expired());
    Assert.IsFalse(Watchdog.Expired(), "the caller quits on the first true, so it must not repeat");
  }

  [TestMethod]
  public void CarriesTheStageTheRunIsIn() {
    Watchdog.Stage = "generating-map";

    Assert.AreEqual("generating-map", Watchdog.Stage);
  }
}
