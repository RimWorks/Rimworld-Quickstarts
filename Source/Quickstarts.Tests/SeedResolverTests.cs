using Microsoft.VisualStudio.TestTools.UnitTesting;
using RimWorks.Quickstarts;

namespace RimWorks.Quickstarts.Tests;

[TestClass]
public class SeedResolverTests {
  [TestMethod]
  public void PrefersTheCommandLineOverTheQuickstart() {
    Assert.AreEqual("fromArgs", SeedResolver.Resolve("fromArgs", "fromQuickstart"));
  }

  [TestMethod]
  public void FallsBackToTheQuickstartSeed() {
    Assert.AreEqual("fromQuickstart", SeedResolver.Resolve(null, "fromQuickstart"));
  }

  [TestMethod]
  public void ReturnsNullWhenNeitherIsSet() {
    Assert.IsNull(SeedResolver.Resolve(null, null));
  }

  [TestMethod]
  public void TreatsABlankSeedAsUnset() {
    Assert.AreEqual("fromQuickstart", SeedResolver.Resolve("   ", "fromQuickstart"));
    Assert.IsNull(SeedResolver.Resolve(string.Empty, "\t"));
  }

  [TestMethod]
  public void TrimsSurroundingWhitespace() {
    Assert.AreEqual("abc123", SeedResolver.Resolve("  abc123  ", null));
    Assert.AreEqual("abc123", SeedResolver.Resolve(null, "\tabc123\n"));
  }
}
