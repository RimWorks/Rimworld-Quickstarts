using System;
using Verse;

namespace RimWorks.Quickstarts;

/// <summary>The command line and environment switches that drive a quickstart run.</summary>
public static class QuickstartArgs {
  /// <summary>Selects the quickstart to launch: <c>-quickstart=SampleQuickstart</c>.</summary>
  public const string SelectArg = "quickstart";

  /// <summary>Runs the quickstart's assertions and exits: <c>-quickstartverify</c>.</summary>
  public const string VerifyArg = "quickstartverify";

  /// <summary>Where the JSON report goes: <c>-quickstartreport=/tmp/report.json</c>. Implies verify.</summary>
  public const string ReportArg = "quickstartreport";

  /// <summary>Overrides the world seed: <c>-quickstartseed=abc123</c>.</summary>
  public const string SeedArg = "quickstartseed";

  /// <summary>Wall-clock limit for the run: <c>-quickstarttimeout=120</c>. Implies verify.</summary>
  public const string TimeoutArg = "quickstarttimeout";

  /// <summary>Where the JUnit XML goes: <c>-quickstartjunit=/tmp/junit.xml</c>. Implies verify.</summary>
  public const string JUnitArg = "quickstartjunit";

  /// <summary>Environment fallback for <see cref="SelectArg"/>, for shells that mangle game args.</summary>
  public const string SelectEnvVar = "RIMWORLD_QUICKSTART";

  private static bool parsed;
  private static string? selectedName;
  private static string? reportPath;
  private static string? seed;
  private static string? junitPath;
  private static int timeoutSeconds;
  private static bool verifyMode;

  /// <summary>Quickstart name the run asked for, or null when none was given.</summary>
  public static string? SelectedName {
    get {
      Parse();
      return selectedName;
    }
  }

  /// <summary>Whether the run should assert and exit instead of staying in the game.</summary>
  public static bool VerifyMode {
    get {
      Parse();
      return verifyMode;
    }
  }

  /// <summary>Explicit report path, or null to use the default under the save data folder.</summary>
  public static string? ReportPath {
    get {
      Parse();
      return reportPath;
    }
  }

  /// <summary>World seed the run asked for, or null when it did not ask for one.</summary>
  public static string? Seed {
    get {
      Parse();
      return seed;
    }
  }

  /// <summary>Seconds before the run gives up, or zero when no limit was asked for.</summary>
  public static int TimeoutSeconds {
    get {
      Parse();
      return timeoutSeconds;
    }
  }

  /// <summary>Where to write JUnit XML, or null when the run did not ask for it.</summary>
  public static string? JUnitPath {
    get {
      Parse();
      return junitPath;
    }
  }

  private static void Parse() {
    if (parsed) {
      return;
    }

    parsed = true;

    if (GenCommandLine.TryGetCommandLineArg(SelectArg, out string value)) {
      selectedName = Clean(value);
    } else {
      selectedName = Clean(SafeEnv(SelectEnvVar));
    }

    if (GenCommandLine.TryGetCommandLineArg(ReportArg, out string path)) {
      reportPath = Clean(path);
    }

    if (GenCommandLine.TryGetCommandLineArg(SeedArg, out string requestedSeed)) {
      seed = Clean(requestedSeed);
    }

    if (GenCommandLine.TryGetCommandLineArg(TimeoutArg, out string limit)
        && int.TryParse(Clean(limit), out int seconds)
        && seconds > 0) {
      timeoutSeconds = seconds;
    }

    if (GenCommandLine.TryGetCommandLineArg(JUnitArg, out string junit)) {
      junitPath = Clean(junit);
    }

    // A report is only ever written by a verify run, so asking for one turns verify on.
    verifyMode = GenCommandLine.CommandLineArgPassed(VerifyArg)
        || !string.IsNullOrEmpty(reportPath)
        || !string.IsNullOrEmpty(junitPath)
        || timeoutSeconds > 0;
  }

  private static string? Clean(string? raw) {
    string trimmed = raw?.Trim().Trim('"', '\'') ?? string.Empty;
    return trimmed.Length == 0 ? null : trimmed;
  }

  private static string? SafeEnv(string name) {
    try {
      return Environment.GetEnvironmentVariable(name);
    } catch (Exception) {
      return null;
    }
  }
}
