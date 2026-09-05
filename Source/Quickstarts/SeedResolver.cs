namespace RimWorks.Quickstarts;

/// <summary>
/// Picks the world seed for one launch. The command line wins over the quickstart's own seed, and
/// a blank value at either level falls through instead of seeding a world from an empty string.
/// </summary>
internal static class SeedResolver {
  internal static string? Resolve(string? fromCommandLine, string? fromQuickstart) {
    return Clean(fromCommandLine) ?? Clean(fromQuickstart);
  }

  private static string? Clean(string? raw) {
    string trimmed = raw?.Trim() ?? string.Empty;
    return trimmed.Length == 0 ? null : trimmed;
  }
}
