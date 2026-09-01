using System;
using System.Collections.Generic;
using Verse;

namespace RimWorks.Quickstarts.Patching;

/// <summary>
/// Picks one backend and applies it. [StaticConstructorOnStartup] order is undefined, so
/// nothing applies until all have registered. Highest priority wins, so Concord beats Harmony.
/// </summary>
[StaticConstructorOnStartup]
public static class PatchBackends {
  /// <summary>Priority the Concord backend registers at.</summary>
  public const int ConcordPriority = 100;

  /// <summary>Priority the Harmony backend registers at.</summary>
  public const int HarmonyPriority = 0;

  private static readonly List<Registration> Registered = [];

  private static bool applied;

  static PatchBackends() {
    // Runs after every static constructor, so the registry is complete by now.
    LongEventHandler.ExecuteWhenFinished(ApplyBest);
  }

  /// <summary>Whether a backend applied its hooks. False means nothing will launch.</summary>
  public static bool Ready { get; private set; }

  /// <summary>Adds a backend to the pool. Called from the backend's static constructor.</summary>
  /// <param name="backend">The backend.</param>
  /// <param name="priority">Higher wins. Use the constants on this class.</param>
  public static void Register(IPatchBackend backend, int priority) {
    Registered.Add(new Registration(backend, priority));
  }

  /// <summary>Applies the highest-priority backend that does not throw.</summary>
  public static void ApplyBest() {
    if (applied) {
      return;
    }

    applied = true;

    if (Registered.Count == 0) {
      Logger.Error("No patching backend loaded; Quickstarts needs Harmony or Concord active.");
      return;
    }

    Registered.Sort((a, b) => b.Priority.CompareTo(a.Priority));

    // A backend that throws hands over to the next, rather than leaving the game unpatched.
    for (int i = 0; i < Registered.Count; i++) {
      Registration registration = Registered[i];
      try {
        registration.Backend.Apply();
      } catch (Exception ex) {
        Logger.Error($"{registration.Backend.Name} backend failed to apply patches: {ex}");
        continue;
      }

      Ready = true;
      Logger.Message($"Patched via {registration.Backend.Name} (priority {registration.Priority}).");
      return;
    }

    Logger.Error("Every patching backend failed to apply; Quickstarts cannot launch anything.");
  }

  private readonly struct Registration {
    public readonly IPatchBackend Backend;
    public readonly int Priority;

    public Registration(IPatchBackend backend, int priority) {
      Backend = backend;
      Priority = priority;
    }
  }
}
