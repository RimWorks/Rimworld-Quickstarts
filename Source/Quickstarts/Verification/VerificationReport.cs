using System;
using System.IO;
using System.Text;
using Verse;

namespace RimWorks.Quickstarts.Verification;

/// <summary>
/// Writes the CI report. JSON is hand-built rather than serialized: RimWorld ships no JSON
/// writer, and the shape is four fields.
/// </summary>
public static class VerificationReport {
  private const string DefaultFileName = "Quickstarts_report.json";

  /// <summary>Writes the report for one run.</summary>
  /// <param name="quickstartName">Class name of the quickstart that ran.</param>
  /// <param name="seed">World seed the run used, so a failure can be replayed.</param>
  /// <param name="ticksRun">Ticks driven before the assertions ran.</param>
  /// <param name="verification">Assertions that ran, or null when the quickstart had none.</param>
  /// <param name="log">What the game logged during the run.</param>
  /// <param name="passed">Overall outcome.</param>
  public static void Write(
      string quickstartName,
      string seed,
      int ticksRun,
      QuickstartVerification? verification,
      LogSummary log,
      bool passed) {
    string? path = ResolvePath();
    if (path == null) {
      return;
    }

    try {
      File.WriteAllText(path, Build(quickstartName, seed, ticksRun, verification, log, passed));
      Logger.Info("Wrote report to {Path}", new object?[] { path });
    } catch (Exception ex) {
      Logger.Error(ex, $"Failed to write report to {path}");
    }
  }

  private static string? ResolvePath() {
    string? requested = QuickstartArgs.ReportPath;
    if (!string.IsNullOrEmpty(requested)) {
      return requested;
    }

    try {
      return Path.Combine(GenFilePaths.SaveDataFolderPath, DefaultFileName);
    } catch (Exception ex) {
      Logger.Error(ex, "Could not resolve a default report path");
      return null;
    }
  }

  private static string Build(
      string quickstartName,
      string seed,
      int ticksRun,
      QuickstartVerification? verification,
      LogSummary log,
      bool passed) {
    StringBuilder sb = new StringBuilder();
    sb.Append("{\n");
    sb.Append("  \"quickstart\": ").Append(JsonString(quickstartName)).Append(",\n");
    sb.Append("  \"seed\": ").Append(JsonString(seed)).Append(",\n");
    sb.Append("  \"ticksRun\": ").Append(ticksRun).Append(",\n");
    sb.Append("  \"passed\": ").Append(passed ? "true" : "false").Append(",\n");
    sb.Append("  \"logErrors\": ").Append(log.Errors.Count).Append(",\n");
    sb.Append("  \"logWarnings\": ").Append(log.Warnings).Append(",\n");
    sb.Append("  \"logTruncated\": ").Append(log.Truncated ? "true" : "false").Append(",\n");
    sb.Append("  \"preLaunchErrors\": ").Append(log.PreLaunchErrors).Append(",\n");
    sb.Append("  \"total\": ").Append(verification?.Results.Count ?? 0).Append(",\n");
    sb.Append("  \"failed\": ").Append(CountFailed(verification)).Append(",\n");
    sb.Append("  \"results\": [");
    AppendResults(sb, verification);
    sb.Append("],\n");
    sb.Append("  \"errors\": [");
    AppendErrors(sb, log);
    sb.Append("]\n}\n");
    return sb.ToString();
  }

  private static void AppendErrors(StringBuilder sb, LogSummary log) {
    for (int i = 0; i < log.Errors.Count; i++) {
      CapturedError error = log.Errors[i];
      sb.Append(i == 0 ? "\n" : ",\n");
      sb.Append("    { \"text\": ").Append(JsonString(error.Text));
      sb.Append(", \"repeats\": ").Append(error.Repeats);
      sb.Append(", \"stackTrace\": ").Append(JsonString(error.StackTrace));
      sb.Append(" }");
    }

    if (log.Errors.Count > 0) {
      sb.Append("\n  ");
    }
  }

  private static int CountFailed(QuickstartVerification? verification) {
    if (verification == null) {
      return 0;
    }

    int failed = 0;
    for (int i = 0; i < verification.Results.Count; i++) {
      if (!verification.Results[i].Passed) {
        failed++;
      }
    }

    return failed;
  }

  private static void AppendResults(StringBuilder sb, QuickstartVerification? verification) {
    if (verification == null) {
      return;
    }

    for (int i = 0; i < verification.Results.Count; i++) {
      AssertResult result = verification.Results[i];
      sb.Append(i == 0 ? "\n" : ",\n");
      sb.Append("    { \"label\": ").Append(JsonString(result.Label));
      sb.Append(", \"passed\": ").Append(result.Passed ? "true" : "false");
      sb.Append(", \"detail\": ").Append(JsonString(result.Detail));
      sb.Append(" }");
    }

    if (verification.Results.Count > 0) {
      sb.Append("\n  ");
    }
  }

  private static string JsonString(string? value) {
    if (value == null) {
      return "null";
    }

    StringBuilder sb = new StringBuilder(value.Length + 2);
    sb.Append('"');
    foreach (char c in value) {
      switch (c) {
        case '"':
          sb.Append("\\\"");
          break;
        case '\\':
          sb.Append("\\\\");
          break;
        case '\n':
          sb.Append("\\n");
          break;
        case '\r':
          sb.Append("\\r");
          break;
        case '\t':
          sb.Append("\\t");
          break;
        default:
          if (c < ' ') {
            sb.Append("\\u").Append(((int)c).ToString("x4"));
          } else {
            sb.Append(c);
          }

          break;
      }
    }

    sb.Append('"');
    return sb.ToString();
  }
}
