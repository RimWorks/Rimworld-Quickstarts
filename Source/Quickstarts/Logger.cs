using System;
using System.Runtime.CompilerServices;
using RimWorks.RimLogging;

namespace RimWorks.Quickstarts;

/// <summary>Puts every Quickstarts line on one RimLogging channel, so a run is one filter away.</summary>
public static class Logger {
  /// <summary>RimLogging channel carrying every line this mod writes.</summary>
  public const string Channel = "Quickstarts";

  /// <summary>Writes an informational line.</summary>
  /// <param name="template">Message text, with {Named} holes for <paramref name="args"/>.</param>
  /// <param name="args">Values for the holes, in the order they appear.</param>
  /// <param name="line">Caller line, filled in by the compiler.</param>
  /// <param name="file">Caller file, filled in by the compiler.</param>
  public static void Info(
      string template,
      object?[]? args = null,
      [CallerLineNumber] int line = 0,
      [CallerFilePath] string file = "") {
    Log.InfoTo(Channel, template, args, line, file);
  }

  /// <summary>Writes a warning line.</summary>
  /// <param name="template">Message text, with {Named} holes for <paramref name="args"/>.</param>
  /// <param name="args">Values for the holes, in the order they appear.</param>
  /// <param name="line">Caller line, filled in by the compiler.</param>
  /// <param name="file">Caller file, filled in by the compiler.</param>
  public static void Warn(
      string template,
      object?[]? args = null,
      [CallerLineNumber] int line = 0,
      [CallerFilePath] string file = "") {
    Log.WarnTo(Channel, template, args, line, file);
  }

  /// <summary>Writes an error line.</summary>
  /// <param name="template">Message text, with {Named} holes for <paramref name="args"/>.</param>
  /// <param name="args">Values for the holes, in the order they appear.</param>
  /// <param name="line">Caller line, filled in by the compiler.</param>
  /// <param name="file">Caller file, filled in by the compiler.</param>
  public static void Error(
      string template,
      object?[]? args = null,
      [CallerLineNumber] int line = 0,
      [CallerFilePath] string file = "") {
    Log.ErrorTo(Channel, template, args, line, file);
  }

  /// <summary>Writes an error line carrying an exception, which RimLogging renders separately.</summary>
  /// <param name="ex">The exception to attach.</param>
  /// <param name="message">What was being attempted.</param>
  /// <param name="line">Caller line, filled in by the compiler.</param>
  /// <param name="file">Caller file, filled in by the compiler.</param>
  public static void Error(
      Exception ex,
      string message,
      [CallerLineNumber] int line = 0,
      [CallerFilePath] string file = "") {
    Log.ErrorTo(Channel, ex, message, line, file);
  }
}
