using OpenTK.Mathematics;

namespace RainCore;

public sealed class UiCover : IUiElement
{
    private readonly UiButton _cover = new();

    public bool Visible { get; set; }

    public void Build(Shader shader)
    {
        _cover.Build(shader);
        _cover.Text = "";
        _cover.UseBackground = false;
    }

    public void Render(UiRect rect)
    {
                                                                               
                                                                         
        if (Visible && _cover.UseBackground) _cover.Render(rect);
    }
}
