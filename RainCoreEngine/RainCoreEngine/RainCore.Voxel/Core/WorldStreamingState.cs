using OpenTK.Mathematics;

namespace RainCore;

internal sealed class WorldStreamingState
{
    public HashSet<Vector2i> GeneratedChunks { get; } = new();
    public ThreadLocal<int> GenerationDepth { get; } = new(() => 0);
    public Queue<Vector2i> PendingChunkLoads { get; } = new();
    public HashSet<Vector2i> QueuedChunkLoads { get; } = new();
    public Vector2i? LastStreamedChunk { get; set; }
    public ManualResetEventSlim ChunkGenSignal { get; } = new(false);
    public Thread? ChunkGenThread { get; set; }
    public volatile bool ChunkGenShuttingDown;
}
