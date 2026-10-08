namespace RainCore;

public enum UiDirection
{
    Horizontal,
    Vertical,
}

public sealed class UiBar : IUiElement
{
    public UiDirection Direction { get; set; } = UiDirection.Vertical;
    public float Gap { get; set; } = 0.006f;
    public int ScrollOffset { get; set; }
    public int VisibleCount { get; set; }
    public List<IUiElement> Items { get; } = new();

    public void Render(UiRect rect)
    {
        if (Items.Count == 0) return;
        int start = Math.Clamp(ScrollOffset, 0, Math.Max(0, Items.Count - 1));
        int count = VisibleCount > 0
            ? Math.Min(VisibleCount, Items.Count - start)
            : Items.Count - start;
        for (int i = 0; i < count; i++)
        {
            var cell = Direction == UiDirection.Horizontal
                ? rect.SliceHorizontal(i, count, Gap)
                : rect.SliceVertical(i, count, Gap);
            Items[start + i].Render(cell);
        }
    }

                                                                             
                                                                               
                                                                                
                                                                               
                                                                          
                         
    public int HitTest(UiRect rect, float ndcX, float ndcY)
    {
        if (Items.Count == 0) return -1;
        int start = Math.Clamp(ScrollOffset, 0, Math.Max(0, Items.Count - 1));
        int count = VisibleCount > 0
            ? Math.Min(VisibleCount, Items.Count - start)
            : Items.Count - start;
        for (int i = 0; i < count; i++)
        {
            var cell = Direction == UiDirection.Horizontal
                ? rect.SliceHorizontal(i, count, Gap)
                : rect.SliceVertical(i, count, Gap);
            if (cell.Contains(ndcX, ndcY)) return start + i;
        }
        return -1;
    }
}
