namespace RainCore;

public readonly record struct UiRect(float Left, float Right, float Bottom, float Top)
{
    public float Width => Right - Left;
    public float Height => Top - Bottom;

    public UiRect Inset(float horizontal, float vertical) =>
        new(Left + horizontal, Right - horizontal, Bottom + vertical, Top - vertical);

                                                                               
                                                                         
                                                                               
                           
    public bool Contains(float x, float y) => x >= Left && x <= Right && y >= Bottom && y <= Top;

    public UiRect SliceHorizontal(int index, int count, float gap = 0f)
    {
        if (count <= 0) return this;
        float width = (Width - gap * Math.Max(0, count - 1)) / count;
        float left = Left + index * (width + gap);
        return new(left, left + width, Bottom, Top);
    }

    public UiRect SliceVertical(int index, int count, float gap = 0f)
    {
        if (count <= 0) return this;
        float height = (Height - gap * Math.Max(0, count - 1)) / count;
        float top = Top - index * (height + gap);
        return new(Left, Right, top - height, top);
    }
}

public interface IUiElement
{
    void Render(UiRect rect);
}
