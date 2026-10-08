namespace RainCore;

public partial class World
{
    private long _meshBuildTicks;
    private int _meshBuildCount;

    public int MeshBuildCount => Volatile.Read(ref _meshBuildCount);
    public double MeshBuildMilliseconds => Volatile.Read(ref _meshBuildTicks) * 1000.0 / System.Diagnostics.Stopwatch.Frequency;

    public int GeneratedChunkCount
    {
        get { lock (_worldDataLock) return _generatedChunks.Count; }
    }

    public int PendingChunkCount
    {
        get { lock (_worldDataLock) return _pendingChunkLoads.Count; }
    }
}