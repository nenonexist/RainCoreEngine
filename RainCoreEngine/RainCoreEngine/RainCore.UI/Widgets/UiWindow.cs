using OpenTK.Mathematics;

namespace RainCore;

public sealed class UiWindow : IUiElement
{
    private readonly TextPanel _background = new();
    private Shader? _shader;
    private string _title = "";

    public List<(UiRect Rect, IUiElement Element)> Children { get; } = new();

    public void Build(Shader shader, string backgroundTexture = "editor_menu.png")
    {
        _shader = shader;
        _background.Build(shader, -1f, 1f, -1f, 1f,
            panelColor: new Vector3(0.10f, 0.06f, 0.16f), panelAlpha: 0.94f,
            usePanelBackground: true, panelTextureFileName: backgroundTexture);
    }

    public void Render(UiRect rect)
    {
        if (_shader == null) return;
        _background.SetText(_title);
        _background.Render(rect.Left, rect.Right, rect.Bottom, rect.Top);
        foreach (var child in Children)
            child.Element.Render(child.Rect);
    }
}
