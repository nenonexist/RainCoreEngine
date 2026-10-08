using OpenTK.Mathematics;

namespace RainCore;

internal sealed class WorldEditStore : IEnumerable<KeyValuePair<Vector3i, BlockType>>
{
    private readonly Dictionary<Vector3i, BlockType> _blocks = new();

    public int Count => _blocks.Count;

    public BlockType this[Vector3i position]
    {
        get => _blocks[position];
        set => _blocks[position] = value;
    }

    public bool TryGetValue(Vector3i position, out BlockType type) => _blocks.TryGetValue(position, out type);

    public void Clear() => _blocks.Clear();

    public IEnumerator<KeyValuePair<Vector3i, BlockType>> GetEnumerator() => _blocks.GetEnumerator();

    System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() => GetEnumerator();
}
