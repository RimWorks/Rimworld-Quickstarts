using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace RimWorks.Quickstarts.Verification;

/// <summary>
/// Writes the run as JUnit XML, the format every CI surface reads natively. Hand-built for the
/// same reason the JSON is: RimWorld ships no serializer and the shape is four elements.
/// </summary>
public static class JUnitReport {
  /// <summary>Writes the XML when the run asked for it.</summary>
  /// <param name="quickstartName">Class name of the quickstart that ran.</param>
  /// <param name="verification">Assertions that ran, or null when the quickstart had none.</param>
  /// <param name="log">What the game logged during the run.</param>
  /// <param name="logCheck">The error budget as one assertion, or null when the check is off.</param>
  /// <param name="timedOutStage">Stage the watchdog fired in, or null when the run finished.</param>
  public static void Write(
      string quickstartName,
      QuickstartVerification? verification,
      LogSummary log,
      AssertResult? logCheck,
      string? timedOutStage) {
    string? path = QuickstartArgs.JUnitPath;
    if (string.IsNullOrEmpty(path)) {
      return;
    }

    try {
      File.WriteAllText(
          path!, Build(quickstartName, verification, log, logCheck, timedOutStage));
      Logger.Info("Wrote JUnit report to {Path}", new object?[] { path });
    } catch (Exception ex) {
      Logger.Error(ex, $"Failed to write JUnit report to {path}");
    }
  }

  internal static string Build(
      string quickstartName,
      QuickstartVerification? verification,
      LogSummary log,
      AssertResult? logCheck,
      string? timedOutStage) {
    List<AssertResult> cases = [];
    if (verification != null) {
      cases.AddRange(verification.Results);
    }

    if (logCheck.HasValue) {
      cases.Add(logCheck.Value);
    }

    int failures = 0;
    for (int i = 0; i < cases.Count; i++) {
      if (!cases[i].Passed) {
        failures++;
      }
    }

    int errors = timedOutStage != null ? 1 : 0;
    int tests = cases.Count + errors;
    string suite = Escape(quickstartName);

    StringBuilder sb = new StringBuilder();
    sb.Append("<?xml version=\"1.0\" encoding=\"utf-8\"?>\n");
    Append(sb, "testsuites", tests, failures, errors, 0);
    Append(sb, "testsuite", tests, failures, errors, 1, suite);

    for (int i = 0; i < cases.Count; i++) {
      AppendCase(sb, suite, cases[i]);
    }

    if (timedOutStage != null) {
      AppendTimeout(sb, suite, timedOutStage);
    }

    AppendSystemErr(sb, log);
    sb.Append("  </testsuite>\n</testsuites>\n");
    return sb.ToString();
  }

  private static void Append(
      StringBuilder sb, string element, int tests, int failures, int errors, int depth,
      string? name = null) {
    sb.Append(depth == 0 ? "<" : "  <").Append(element);
    if (name != null) {
      sb.Append(" name=\"").Append(name).Append('"');
    }

    sb.Append(" tests=\"").Append(tests);
    sb.Append("\" failures=\"").Append(failures);
    sb.Append("\" errors=\"").Append(errors).Append("\">\n");
  }

  private static void AppendCase(StringBuilder sb, string suite, AssertResult result) {
    sb.Append("    <testcase classname=\"").Append(suite);
    sb.Append("\" name=\"").Append(Escape(result.Label)).Append('"');

    if (result.Passed) {
      sb.Append(" />\n");
      return;
    }

    sb.Append(">\n      <failure message=\"").Append(Escape(result.Detail)).Append("\" />\n");
    sb.Append("    </testcase>\n");
  }

  private static void AppendTimeout(StringBuilder sb, string suite, string stage) {
    sb.Append("    <testcase classname=\"").Append(suite).Append("\" name=\"run completed\">\n");
    sb.Append("      <error message=\"timed out during ").Append(Escape(stage)).Append("\" />\n");
    sb.Append("    </testcase>\n");
  }

  private static void AppendSystemErr(StringBuilder sb, LogSummary log) {
    if (log.Errors.Count == 0) {
      return;
    }

    sb.Append("    <system-err>");
    for (int i = 0; i < log.Errors.Count; i++) {
      CapturedError error = log.Errors[i];
      sb.Append('\n').Append(Escape(error.Text));
      if (error.Repeats > 1) {
        sb.Append(" (x").Append(error.Repeats).Append(')');
      }
    }

    sb.Append("\n    </system-err>\n");
  }

  private static string Escape(string? value) {
    if (string.IsNullOrEmpty(value)) {
      return string.Empty;
    }

    StringBuilder sb = new StringBuilder(value!.Length);
    foreach (char c in value) {
      switch (c) {
        case '&':
          sb.Append("&amp;");
          break;
        case '<':
          sb.Append("&lt;");
          break;
        case '>':
          sb.Append("&gt;");
          break;
        case '"':
          sb.Append("&quot;");
          break;
        case '\'':
          sb.Append("&apos;");
          break;
        case '\t':
          // Attribute values normalise raw whitespace to a space, so these go as references.
          sb.Append("&#x9;");
          break;
        case '\n':
          sb.Append("&#xA;");
          break;
        case '\r':
          sb.Append("&#xD;");
          break;
        default:
          // Dropped, not encoded: XML 1.0 has no escape for these, so &#x1; is a parse error.
          if (c >= ' ' && c != '\uFFFE' && c != '\uFFFF') {
            sb.Append(c);
          }

          break;
      }
    }

    return sb.ToString();
  }
}
