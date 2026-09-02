using System.Collections.Generic;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace RimWorks.Quickstarts.UI;

/// <summary>Replaces vanilla's dev quicktest with a pick between it and every quickstart loaded.</summary>
public class Dialog_QuicktestPicker : Window {
  private const float WindowWidth = 720f;
  private const float WindowHeight = 600f;
  private const float TitleHeight = 36f;
  private const float ListWidthFraction = 0.35f;
  private const float PaneGap = 17f;
  private const float RowGap = 6f;
  private const float RowPad = 4f;
  private const float BlurbLines = 4f;
  private const float ButtonHeight = 38f;
  private const float Gap = 10f;
  private const float ScrollBarWidth = 16f;

  private static readonly Color BlurbColor = new Color(0.72f, 0.72f, 0.7f);

  private readonly List<Entry> entries = BuildEntries();

  // TruncateHeight trims a character at a time, so this keeps it off the per-frame path.
  private readonly Dictionary<string, string> blurbCache = new Dictionary<string, string>();

  private Vector2 listScroll;
  private Vector2 infoScroll;
  private int selected;

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

    Rect body = inRect;
    body.yMin += TitleHeight + Gap;
    body.yMax -= ButtonHeight + Gap;

    // The split Page_SelectScenario uses: list, gutter, framed info panel.
    float listWidth = Mathf.Round(body.width * ListWidthFraction);
    DrawList(new Rect(body.x, body.y, listWidth, body.height));
    DrawInfo(new Rect(body.x + listWidth + PaneGap, body.y, body.width - listWidth - PaneGap, body.height));

    Rect launch = new Rect(inRect.x, inRect.yMax - ButtonHeight, inRect.width, ButtonHeight);
    if (Widgets.ButtonText(launch, "Quickstarts_Picker_Launch".Translate(entries[selected].Label))) {
      Launch();
    }
  }

  private static List<Entry> BuildEntries() {
    List<Entry> list = [
      new Entry(
          "Quickstarts_Picker_Vanilla".Translate().Resolve(),
          "Quickstarts_Picker_VanillaBlurb".Translate().Resolve(),
          null),
    ];

    List<AbstractQuickstart> quickstarts = QuickstartRegistry.Instantiate();
    for (int i = 0; i < quickstarts.Count; i++) {
      list.Add(new Entry(
          quickstarts[i].label.Resolve(),
          quickstarts[i].description.Resolve(),
          quickstarts[i]));
    }

    return list;
  }

  private void DrawList(Rect rect) {
    float viewWidth = rect.width - ScrollBarWidth;
    float total = 0f;
    for (int i = 0; i < entries.Count; i++) {
      total += RowHeight(entries[i], viewWidth) + RowGap;
    }

    Widgets.BeginScrollView(rect, ref listScroll, new Rect(0f, 0f, viewWidth, total));
    float y = 0f;

    for (int i = 0; i < entries.Count; i++) {
      float height = RowHeight(entries[i], viewWidth);
      if (DrawRow(new Rect(0f, y, viewWidth, height), entries[i], i == selected)) {
        selected = i;
        infoScroll = Vector2.zero;
      }

      y += height + RowGap;
    }

    Widgets.EndScrollView();
  }

  private void DrawInfo(Rect rect) {
    Widgets.DrawMenuSection(rect);
    Rect inner = rect.GetInnerRect();
    Entry entry = entries[selected];
    string body = entry.Quickstart?.GetDescription().Resolve() ?? entry.Blurb;
    float width = inner.width - ScrollBarWidth;
    float titleHeight;
    float height;

    using (new TextBlock(GameFont.Medium)) {
      titleHeight = Text.LineHeight;
    }

    using (new TextBlock(GameFont.Small)) {
      height = titleHeight + Gap + Text.CalcHeight(body, width);
    }

    Widgets.BeginScrollView(inner, ref infoScroll, new Rect(0f, 0f, width, height));

    using (new TextBlock(GameFont.Medium, TextAnchor.UpperLeft, Color.white)) {
      Widgets.Label(new Rect(0f, 0f, width, titleHeight), entry.Label);
    }

    using (new TextBlock(GameFont.Small)) {
      Widgets.Label(new Rect(0f, titleHeight + Gap, width, height - titleHeight - Gap), body);
    }

    Widgets.EndScrollView();
  }

  private bool DrawRow(Rect row, Entry entry, bool isSelected) {
    Widgets.DrawOptionBackground(row, isSelected);
    MouseoverSounds.DoRegion(row);

    Rect inner = row.ContractedBy(RowPad);
    Rect labelRect = inner;

    using (new TextBlock(GameFont.Small, TextAnchor.UpperLeft, Color.white)) {
      labelRect.height = Text.CalcHeight(entry.Label, inner.width);
      Widgets.Label(labelRect, entry.Label);
    }

    Rect blurbRect = inner;
    blurbRect.yMin = labelRect.yMax;

    using (new TextBlock(GameFont.Tiny, TextAnchor.UpperLeft, BlurbColor)) {
      // Vanilla nudges the same way where the tiny font falls back to the small one.
      if (!Text.TinyFontSupported) {
        blurbRect.yMin -= 6f;
      }

      Widgets.Label(blurbRect, ClampedBlurb(entry, row.width));
    }

    return Widgets.ButtonInvisible(row, false);
  }

  private float RowHeight(Entry entry, float width) {
    float inner = width - (RowPad * 2f);
    float height = RowPad * 2f;

    using (new TextBlock(GameFont.Small)) {
      height += Text.CalcHeight(entry.Label, inner);
    }

    using (new TextBlock(GameFont.Tiny)) {
      height += Text.CalcHeight(ClampedBlurb(entry, width), inner);
    }

    return height;
  }

  private string ClampedBlurb(Entry entry, float width) {
    using (new TextBlock(GameFont.Tiny)) {
      return entry.Blurb.TruncateHeight(width - (RowPad * 2f), Text.LineHeight * BlurbLines, blurbCache);
    }
  }

  private void Launch() {
    Entry entry = entries[selected];
    Close();

    if (entry.Quickstart == null) {
      VanillaQuicktest.RowAction();
      return;
    }

    Quickstarter.Launch(entry.Quickstart);
  }

  private readonly struct Entry {
    public readonly string Label;
    public readonly string Blurb;
    public readonly AbstractQuickstart? Quickstart;

    public Entry(string label, string blurb, AbstractQuickstart? quickstart) {
      Label = label;
      Blurb = blurb;
      Quickstart = quickstart;
    }
  }
}
