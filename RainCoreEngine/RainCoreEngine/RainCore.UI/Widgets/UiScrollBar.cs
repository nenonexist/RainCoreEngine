using OpenTK.Mathematics;

namespace RainCore;

public sealed class UiScrollBar : IUiElement
{
    private readonly TextPanel _track = new();
    private readonly TextPanel _thumb = new();
    private bool _built;

    public int ItemCount { get; set; }
    public int VisibleCount { get; set; }
    public int Offset { get; set; }

    public void Build(Shader shader)
    {
        _track.Build(shader, -1f, 1f, -1f, 1f,
            panelColor: new Vector3(0.12f, 0.08f, 0.18f), panelAlpha: 0.7f,
            usePanelBackground: true, panelTextureFileName: "Uiscrollbar.png");
        _thumb.Build(shader, -1f, 1f, -1f, 1f,
            panelColor: new Vector3(0.7f, 0.7f, 0.7f), panelAlpha: 1f,
            usePanelBackground: true, panelTextureFileName: "UiScrollBar2.png");
        _track.SetText(" ");
        _thumb.SetText(" ");
        _built = true;
    }

    public void Render(UiRect rect)
    {
        if (!_built || ItemCount <= VisibleCount || VisibleCount <= 0) return;
        _track.Render(rect.Left, rect.Right, rect.Bottom, rect.Top);
        float thumbHeight = rect.Height * VisibleCount / ItemCount;
        float maxOffset = Math.Max(1, ItemCount - VisibleCount);
        float travel = rect.Height - thumbHeight;
        float thumbTop = rect.Top - travel * Math.Clamp(Offset / maxOffset, 0f, 1f);
        _thumb.Render(rect.Left, rect.Right, thumbTop - thumbHeight, thumbTop);
    }
}
