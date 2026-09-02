using System;
using System.Collections.Generic;
using Verse;

namespace RimWorks.Quickstarts;

/// <summary>
/// Every quickstart the loaded mods define. A mod adds one by declaring a non-abstract
/// <see cref="AbstractQuickstart"/> with a parameterless constructor, and nothing else.
/// </summary>
public static class QuickstartRegistry {
  private static List<Type>? cachedTypes;

  /// <summary>Discovered quickstart types, sorted by class name.</summary>
  public static IReadOnlyList<Type> AllTypes => cachedTypes ??= Discover();

  /// <summary>Builds one instance of every discovered quickstart.</summary>
  /// <returns>Fresh instances, sorted by class name. Types that throw are skipped.</returns>
  public static List<AbstractQuickstart> Instantiate() {
    List<AbstractQuickstart> built = [];
    IReadOnlyList<Type> types = AllTypes;
    for (int i = 0; i < types.Count; i++) {
      AbstractQuickstart? quickstart = Create(types[i]);
      if (quickstart != null) {
        built.Add(quickstart);
      }
    }

    return built;
  }

  /// <summary>Instantiates one quickstart type, logging instead of throwing on failure.</summary>
  /// <param name="type">Type to instantiate.</param>
  /// <returns>The instance, or null when the constructor threw.</returns>
  public static AbstractQuickstart? Create(Type type) {
    try {
      return (AbstractQuickstart)Activator.CreateInstance(type);
    } catch (Exception ex) {
      Logger.Error(ex, $"Could not instantiate quickstart {type.FullName}");
      return null;
    }
  }

  private static List<Type> Discover() {
    List<Type> found = [];
    foreach (Type type in typeof(AbstractQuickstart).AllSubclassesNonAbstract()) {
      if (type.GetConstructor(Type.EmptyTypes) == null) {
        Logger.Warn(
            "Skipping quickstart {Quickstart}: it has no parameterless constructor.",
            new object?[] { type.FullName });
        continue;
      }

      found.Add(type);
    }

    found.Sort((a, b) => string.Compare(a.Name, b.Name, StringComparison.OrdinalIgnoreCase));
    return found;
  }
}
