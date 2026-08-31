using System;
using System.Collections.Generic;
using RimWorks.Quickstarts.Patching;
using RimWorld;
using UnityEngine;
using Verse;

namespace RimWorks.Quickstarts;

/// <summary>Mod entry point and settings window.</summary>
public class QuickstartsMod : Mod {
  private static readonly QuickstartsSettings Fallback = new QuickstartsSettings();

  private readonly QuickstartsSettings settings;

  /// <summary>Creates the mod and loads its settings.</summary>
  /// <param name="content">The mod's content pack, supplied by RimWorld.</param>
  public QuickstartsMod(ModContentPack content) : base(content) {
    Instance = this;
    settings = GetSettings<QuickstartsSettings>() ?? Fallback;
  }

  /// <summary>The loaded mod, or null before RimWorld has constructed it.</summary>
  public static QuickstartsMod? Instance { get; private set; }

  /// <summary>Current settings. Falls back to defaults if read before the mod is constructed.</summary>
  public static QuickstartsSettings Settings => Instance?.settings ?? Fallback;

  /// <inheritdoc/>
  public override string SettingsCategory() => "Quickstarts";

  /// <inheritdoc/>
  public override void DoSettingsWindowContents(Rect inRect) {
    Listing_Standard listing = new Listing_Standard();
    listing.Begin(inRect);

    if (!PatchBackends.Ready) {
      using (new TextBlock(ColorLibrary.Yellow)) {
        listing.Label("Quickstarts_MissingBackend".Translate());
      }

      listing.Gap();
    }

    if (listing.ButtonTextLabeled(
        "Quickstarts_Settings_Default".Translate(),
        LabelFor(settings.defaultQuickstart),
        tooltip: "Quickstarts_Settings_DefaultDesc".Translate())) {
      Find.WindowStack.Add(new FloatMenu(QuickstartOptions()));
    }

    if (listing.ButtonTextLabeled(
        "Quickstarts_Settings_TestScenario".Translate(),
        ScenarioLabelFor(settings.testScenarioDefName),
        tooltip: "Quickstarts_Settings_TestScenarioDesc".Translate())) {
      Find.WindowStack.Add(new FloatMenu(ScenarioOptions()));
    }

    listing.CheckboxLabeled(
        "Quickstarts_Settings_ReplacePicker".Translate(),
        ref settings.replaceQuicktestButton,
        "Quickstarts_Settings_ReplacePickerDesc".Translate());

    listing.End();
  }

  private static string LabelFor(string? assemblyQualifiedName) {
    if (string.IsNullOrEmpty(assemblyQualifiedName)) {
      return "Quickstarts_Settings_None".Translate();
    }

    Type? type = QuickstartLookup.Resolve(assemblyQualifiedName, QuickstartRegistry.AllTypes, out string? _);
    return type?.Name ?? assemblyQualifiedName!;
  }

  private static string ScenarioLabelFor(string? defName) {
    if (string.IsNullOrEmpty(defName)) {
      return ScenarioDefOf.Crashlanded.LabelCap;
    }

    ScenarioDef? def = DefDatabase<ScenarioDef>.GetNamedSilentFail(defName);
    return def?.LabelCap.ToString() ?? defName!;
  }

  private List<FloatMenuOption> QuickstartOptions() {
    List<FloatMenuOption> options = [
      new FloatMenuOption("Quickstarts_Settings_None".Translate(), () => settings.defaultQuickstart = null),
    ];

    IReadOnlyList<Type> types = QuickstartRegistry.AllTypes;
    for (int i = 0; i < types.Count; i++) {
      Type type = types[i];
      options.Add(new FloatMenuOption(type.Name, () => settings.defaultQuickstart = type.AssemblyQualifiedName));
    }

    return options;
  }

  private List<FloatMenuOption> ScenarioOptions() {
    List<FloatMenuOption> options = [];
    foreach (ScenarioDef def in DefDatabase<ScenarioDef>.AllDefsListForReading) {
      ScenarioDef captured = def;
      options.Add(new FloatMenuOption(captured.LabelCap, () => settings.testScenarioDefName = captured.defName));
    }

    return options;
  }
}
