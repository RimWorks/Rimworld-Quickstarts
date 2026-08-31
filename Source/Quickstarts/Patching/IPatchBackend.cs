namespace RimWorks.Quickstarts.Patching;

/// <summary>
/// One patching library. A backend expresses the mod's hooks in its own terms, so the core
/// never references Harmony or Concord.
/// </summary>
public interface IPatchBackend {
  /// <summary>Name used in the log line that reports which backend won.</summary>
  string Name { get; }

  /// <summary>Applies every hook. Called once, on the winning backend only.</summary>
  void Apply();
}
