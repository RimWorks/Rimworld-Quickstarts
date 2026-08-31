namespace RimWorks.Quickstarts.Verification;

/// <summary>One assertion's outcome.</summary>
public readonly struct AssertResult {
  /// <summary>What the assertion checked.</summary>
  public readonly string Label;

  /// <summary>Whether it held.</summary>
  public readonly bool Passed;

  /// <summary>Why it failed, or null when it passed.</summary>
  public readonly string? Detail;

  /// <summary>Records one outcome.</summary>
  /// <param name="label">What the assertion checked.</param>
  /// <param name="passed">Whether it held.</param>
  /// <param name="detail">Why it failed, or null when it passed.</param>
  public AssertResult(string label, bool passed, string? detail) {
    Label = label;
    Passed = passed;
    Detail = detail;
  }
}
