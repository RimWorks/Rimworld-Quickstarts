using System.Text;
using UnityEngine;
using Verse;

namespace RimWorks.Quickstarts.UI;

/// <summary>
/// The panel drawn over the loading screen while a quickstart builds its game, so a long
/// generation is not a blank wait.
/// </summary>
public class StatusBox {
  private static readonly Vector2 MinSize = new Vector2(240f, 75f);
  private static readonly Vector2 Padding = new Vector2(26f, 18f);

  private readonly Quickstarter quickstarter;

  /// <summary>Creates the status box for one launch.</summary>
  /// <param name="quickstarter">The launcher whose quickstart is named in the box.</param>
  public StatusBox(Quickstarter quickstarter) {
    this.quickstarter = quickstarter;
  }

  /// <summary>Draws the box.</summary>
  public void OnGUI() {
    string text = BuildText();
    Draw(BuildRect(text), text);
  }

  private static Rect BuildRect(string text) {
    Vector2 size = Text.CalcSize(text);
    float width = Mathf.Max(MinSize.x, size.x + (Padding.x * 2f));
    float height = Mathf.Max(MinSize.y, size.y + (Padding.y * 2f));
    return new Rect(
        (Verse.UI.screenWidth - width) / 2f,
        ((Verse.UI.screenHeight / 2f) - height) / 2f,
        width,
        height).Rounded();
  }

  private static void Draw(Rect rect, string text) {
    Widgets.DrawShadowAround(rect);
    Widgets.DrawWindowBackground(rect);
    using (new TextBlock(TextAnchor.MiddleCenter)) {
      Widgets.Label(rect, text);
    }
  }

  private string BuildText() {
    AbstractQuickstart quickstart = quickstarter.Quickstart!;
    StringBuilder sb = new StringBuilder("Quickstarts_StatusBox_Launching".Translate());
    sb.AppendLine();
    sb.AppendLine();
    sb.AppendLine(quickstart.GetType().Name.Colorize(ColoredText.DateTimeColor));
    sb.AppendLine();
    sb.AppendLine(quickstart.description.Resolve().Colorize(ColorLibrary.Grey));
    return sb.ToString();
  }
}
