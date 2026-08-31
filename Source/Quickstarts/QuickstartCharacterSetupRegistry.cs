using System;
using System.Collections.Generic;
using Verse;

namespace RimWorks.Quickstarts;

/// <summary>
/// Per-colonist setup other mods contribute by index. A quickstart calls <see cref="Apply"/>
/// from PrepareColonists, so the mod that owns a power does not have to own the quickstart.
/// </summary>
public static class QuickstartCharacterSetupRegistry {
  private static readonly Dictionary<int, List<Action<Pawn>>> Setups = [];

  /// <summary>Adds setup that runs on the colonist at the given index.</summary>
  /// <param name="pawnIndex">Zero-based index into the colonist list.</param>
  /// <param name="setup">What to do to that colonist.</param>
  public static void Register(int pawnIndex, Action<Pawn> setup) {
    if (!Setups.TryGetValue(pawnIndex, out List<Action<Pawn>>? list)) {
      list = [];
      Setups[pawnIndex] = list;
    }

    list.Add(setup);
  }

  /// <summary>Runs every registered setup for one colonist.</summary>
  /// <param name="pawnIndex">Zero-based index into the colonist list.</param>
  /// <param name="pawn">The colonist to set up.</param>
  public static void Apply(int pawnIndex, Pawn pawn) {
    if (!Setups.TryGetValue(pawnIndex, out List<Action<Pawn>>? list)) {
      return;
    }

    for (int i = 0; i < list.Count; i++) {
      try {
        list[i](pawn);
      } catch (Exception ex) {
        Logger.Error($"Character setup {i} for pawn {pawnIndex} threw: {ex}");
      }
    }
  }
}
