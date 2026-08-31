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
  /// <param name="verification">Assertions that ran, or null when the quickstart had none.</param>
  /// <param name="passed">Overall outcome.</param>
  public static void Write(string quickstartName, QuickstartVerification? verification, bool passed) {
    string? path = ResolvePath();
    if (path == null) {
      return;
    }

    try {
      File.WriteAllText(path, Build(quickstartName, verification, passed));
      Logger.Message($"Wrote report to {path}");
    } catch (Exception ex) {
      Logger.Error($"Failed to write report to {path}: {ex}");
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
      Logger.Error($"Could not resolve a default report path: {ex.Message}");
      return null;
    }
  }

  private static string Build(string quickstartName, QuickstartVerification? verification, bool passed) {
    int total = verification?.Results.Count ?? 0;
    int failed = 0;
    if (verification != null) {
      for (int i = 0; i < verification.Results.Count; i++) {
        if (!verification.Results[i].Passed) {
          failed++;
        }
      }
    }

    StringBuilder sb = new StringBuilder();
    sb.Append("{\n");
    sb.Append("  \"quickstart\": ").Append(JsonString(quickstartName)).Append(",\n");
    sb.Append("  \"passed\": ").Append(passed ? "true" : "false").Append(",\n");
    sb.Append("  \"total\": ").Append(total).Append(",\n");
    sb.Append("  \"failed\": ").Append(failed).Append(",\n");
    sb.Append("  \"results\": [");

    if (verification != null) {
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

    sb.Append("]\n}\n");
    return sb.ToString();
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
