using System.Collections.Generic;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace RimWorks.Quickstarts.UI;

/// <summary>
/// Replaces vanilla's straight-to-Crashlanded dev quicktest with a choice between that and
/// every quickstart the loaded mods define.
/// </summary>
public class Dialog_QuicktestPicker : Window {
  private const float WindowWidth = 520f;
  private const float WindowHeight = 560f;
  private const float TitleHeight = 36f;
  private const float RowHeight = 58f;
  private const float RowGap = 4f;
  private const float Pad = 8f;
  private const float ScrollBarWidth = 16f;

  private static readonly Color RowColor = new Color(0.16f, 0.16f, 0.18f, 0.65f);
  private static readonly Color BlurbColor = new Color(0.72f, 0.72f, 0.7f);

  private readonly List<AbstractQuickstart> quickstarts = QuickstartRegistry.Instantiate();

  private Vector2 scrollPos;

  /// <summary>Creates the picker.</summary>
  public Dialog_QuicktestPicker() {
    forcePause = true;
    absorbInputAroundWindow = true;
    closeOnClickedOutside = true;
    doCloseX = true;
    draggable = true;
  }

  /// <inheritdoc/>
  public override Vector2 InitialSize => new Vector2(WindowWidth, WindowHeight);

  /// <summary>Adds the picker to the window stack.</summary>
  public static void Open() {
    Find.WindowStack.Add(new Dialog_QuicktestPicker());
  }

  /// <inheritdoc/>
  public override void DoWindowContents(Rect inRect) {
    using (new TextBlock(GameFont.Medium)) {
      Widgets.Label(inRect.TopPartPixels(TitleHeight), "Quickstarts_Picker_Title".Translate());
    }

    Rect scrollArea = new Rect(
        inRect.x,
        inRect.y + TitleHeight + Pad,
        inRect.width,
        inRect.height - TitleHeight - Pad);
    float totalHeight = (quickstarts.Count + 1) * (RowHeight + RowGap);
    Rect viewRect = new Rect(0f, 0f, scrollArea.width - ScrollBarWidth, totalHeight);

    Widgets.BeginScrollView(scrollArea, ref scrollPos, viewRect);
    float y = 0f;

    if (DrawRow(
        new Rect(0f, y, viewRect.width, RowHeight),
        "Quickstarts_Picker_Vanilla".Translate(),
        "Quickstarts_Picker_VanillaBlurb".Translate())) {
      Close();
      Widgets.EndScrollView();
      VanillaQuicktest.RowAction();
      return;
    }

    y += RowHeight + RowGap;

    for (int i = 0; i < quickstarts.Count; i++) {
      AbstractQuickstart quickstart = quickstarts[i];
      Rect row = new Rect(0f, y, viewRect.width, RowHeight);
      if (DrawRow(row, quickstart.GetType().Name, quickstart.description, quickstart.GetDescription())) {
        Close();
        Widgets.EndScrollView();
        Quickstarter.Launch(quickstart);
        return;
      }

      y += RowHeight + RowGap;
    }

    Widgets.EndScrollView();
  }

  private static bool DrawRow(Rect row, string label, string blurb, string? tooltip = null) {
    Widgets.DrawBoxSolid(row, RowColor);
    Widgets.DrawHighlightIfMouseover(row);
    TooltipHandler.TipRegion(row, tooltip ?? blurb);
    MouseoverSounds.DoRegion(row);

    Rect inner = row.ContractedBy(Pad);
    Rect labelRect = inner.TopPartPixels(inner.height / 2f);
    Rect blurbRect = new Rect(inner.x, labelRect.yMax, inner.width, inner.height / 2f);

    using (new TextBlock(GameFont.Small, TextAnchor.MiddleLeft, Color.white)) {
      Widgets.Label(labelRect, label);
    }

    using (new TextBlock(GameFont.Tiny, TextAnchor.MiddleLeft, BlurbColor)) {
      Widgets.Label(blurbRect, blurb.Truncate(blurbRect.width));
    }

    return Widgets.ButtonInvisible(row, false);
  }
}
