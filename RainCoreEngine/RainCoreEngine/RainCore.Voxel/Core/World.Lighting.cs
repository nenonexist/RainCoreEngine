using OpenTK.Mathematics;

namespace RainCore;

             
                                                                        
                                                                          
                                                                            
                                                                          
                                                                           
                                       
   
                                                                            
                                                                        
                                                                         
                                                           
   
                                                                       
                                                                          
                                                                        
                                                                           
                                                                          
                                                                            
                                                                       
                                                           
   
                                                                          
                                                                         
                                                                   
                                                                       
                                                                            
                                                                           
                                                                        
                                                             
                                                                         
                                                                         
                                                                    
                                                                         
                                                                      
                                                                     
                                                                        
                     
              
public partial class World
{
                                                                                                                      
    public const int MaxLightLevel = 15;

                                                                           
                                                                                 
                                             
    private const byte PlayerLightLevel = 5;

                                                                                
                                                                                       
    private const int PlayerLightCellSize = 3;

    private static readonly Vector3i[] LightNeighbors6 =
    {
        new(1, 0, 0), new(-1, 0, 0), new(0, 1, 0), new(0, -1, 0), new(0, 0, 1), new(0, 0, -1)
    };

    private readonly Dictionary<Vector3i, byte> _blockLight = new();
    private readonly Dictionary<Vector3i, (int minX, int maxX, int minY, int maxY, int minZ, int maxZ)> _relightBuckets = new();

                                                                                    
                                                                     
                                        
    public bool PlayerLightEnabled { get; private set; } = true;

                                                                        
                                                                     
                                                                              
                                                                         
                                                                           
                                                                            
                                                                               
                                                                           
                                                                           
    public bool PlayerLightSuppressed { get; private set; }

                                                                                     
                                                                                  
                                                                                 
                                   
    private bool EffectivePlayerLight => PlayerLightEnabled && !PlayerLightSuppressed;

                                                                                    
                                                                                 
                                                                                 
                                                            
    private Vector3i? _playerLightSeedPos;

                                                                                  
                                                                                 
    private Vector3i? _playerLightCell;

                 
                                                                           
                                                                        
                                                                            
                                                                         
                                                   
                  
    public byte GetLight(Vector3i pos) { lock (_worldDataLock) return _blockLight.TryGetValue(pos, out var l) ? l : (byte)0; }

                 
                                                                            
                                                                             
                                                                              
                                                                            
                                                                            
                                                                            
                      
                  
    public void RelightAll()
    {
        lock (_worldDataLock) { _blockLight.Clear(); }
        var queue = new Queue<Vector3i>();

                                                                              
                                                                                     
                                                                                     
                                                                               
                                   
        List<Chunk> chunksSnapshot;
        lock (_worldDataLock) { chunksSnapshot = new List<Chunk>(_chunks.Values); }

        foreach (var chunk in chunksSnapshot)
        {
            foreach (var (pos, type) in chunk.SnapshotBlocks())
            {
                int lum = BlockData.GetLuminosity(type);
                if (lum <= 0) continue;
                if (SetLightIfBrighter(pos, (byte)lum)) queue.Enqueue(pos);
            }
        }

                                                                          
                                                                         
                                                                               
                                                                    
        if (EffectivePlayerLight && _playerLightSeedPos is { } seed)
        {
            if (SetLightIfBrighter(seed, PlayerLightLevel)) queue.Enqueue(seed);
        }

        PropagateLight(queue);
        foreach (var chunk in chunksSnapshot) chunk.MarkDirty();
    }

                 
                                                                             
                                                                        
                                                               
                                                                            
                                                                       
                                                                       
                                                                             
                                                               
                                                                           
                                                
                  
    private void RelightRegion(Vector3i changedPos)
    {
        const int pad = MaxLightLevel;
        RelightBox(changedPos.X - pad, changedPos.X + pad,
                   changedPos.Y - pad, changedPos.Y + pad,
                   changedPos.Z - pad, changedPos.Z + pad);
    }

                 
                                                                            
                                                                              
                                                                             
                                                                               
                                                                             
                                                                                 
                                                                              
                                                         
       
                                                                                 
                                                                                
                                                                                
                                                                              
                                                                       
                                                                             
                                                                      
                                                                               
                                                                                 
                                                                        
                  
    private void RelightRegions(IReadOnlyList<Vector3i> changedPositions)
    {
        if (changedPositions.Count == 0) return;
        if (changedPositions.Count == 1) { RelightRegion(changedPositions[0]); return; }

        const int pad = MaxLightLevel;
        const int bucketSize = MaxLightLevel * 2;                                                                

        _relightBuckets.Clear();
        foreach (var p in changedPositions)
        {
            var key = new Vector3i(FloorDiv(p.X, bucketSize), FloorDiv(p.Y, bucketSize), FloorDiv(p.Z, bucketSize));
            if (_relightBuckets.TryGetValue(key, out var box))
                _relightBuckets[key] = (Math.Min(box.minX, p.X), Math.Max(box.maxX, p.X),
                                Math.Min(box.minY, p.Y), Math.Max(box.maxY, p.Y),
                                Math.Min(box.minZ, p.Z), Math.Max(box.maxZ, p.Z));
            else
                _relightBuckets[key] = (p.X, p.X, p.Y, p.Y, p.Z, p.Z);
        }

        foreach (var box in _relightBuckets.Values)
            RelightBox(box.minX - pad, box.maxX + pad, box.minY - pad, box.maxY + pad, box.minZ - pad, box.maxZ + pad);
    }

                                                                                       
                                                                                    
                                                                                            
    private void RelightBox(int minX, int maxX, int minY, int maxY, int minZ, int maxZ)
    {
                                                                          
                                                                         
                                                                        
                                                                                
          
                                                                                    
                                                                                
                                                                               
                                                                                   
                                                                       
                                                                                    
                                                                               
                                                                              
                                                      
        for (int x = minX; x <= maxX; x++)
        for (int y = minY; y <= maxY; y++)
        for (int z = minZ; z <= maxZ; z++)
            lock (_worldDataLock) { _blockLight.Remove(new Vector3i(x, y, z)); }

                                                                           
                                                                            
                                                                         
                                                                        
                                                                    
                                              
        var queue = new Queue<Vector3i>();
        for (int x = minX; x <= maxX; x++)
        for (int y = minY; y <= maxY; y++)
        for (int z = minZ; z <= maxZ; z++)
        {
            var p = new Vector3i(x, y, z);
            int lum = BlockData.GetLuminosity(GetBlock(p));
            if (EffectivePlayerLight && _playerLightSeedPos.HasValue && p == _playerLightSeedPos.Value)
                lum = Math.Max(lum, PlayerLightLevel);
            if (lum <= 0) continue;
            if (SetLightIfBrighter(p, (byte)lum)) queue.Enqueue(p);
        }

        PropagateLight(queue);
        MarkChunksDirtyInBox(minX, maxX, minZ, maxZ);
    }

                 
                                                                         
                                                                           
                                                                             
                                                                       
                                                                           
                       
                  
    public void UpdatePlayerLight(Vector3i worldBlockPos)
    {
        var cell = new Vector3i(
            FloorDiv(worldBlockPos.X, PlayerLightCellSize),
            FloorDiv(worldBlockPos.Y, PlayerLightCellSize),
            FloorDiv(worldBlockPos.Z, PlayerLightCellSize));

        if (_playerLightCell.HasValue && _playerLightCell.Value == cell)
        {
                                                                            
                                                                        
                                                                          
            _playerLightSeedPos = worldBlockPos;
            return;
        }

        var oldSeed = _playerLightSeedPos;
        _playerLightCell = cell;
        _playerLightSeedPos = worldBlockPos;

        if (!EffectivePlayerLight) return;                                                                                                            

        if (oldSeed.HasValue) RelightRegion(oldSeed.Value);
        RelightRegion(worldBlockPos);
    }

                                                                                   
                                                                                   
                                                                                          
    public void SetPlayerLightEnabled(bool enabled)
    {
        if (PlayerLightEnabled == enabled) return;
        PlayerLightEnabled = enabled;
        if (_playerLightSeedPos is { } seed) RelightRegion(seed);
    }

                                                                               
                                                                                    
                                                                                 
                                                                                 
                                                                              
    public void SetPlayerLightSuppressed(bool suppressed)
    {
        if (PlayerLightSuppressed == suppressed) return;
        PlayerLightSuppressed = suppressed;
        if (_playerLightSeedPos is { } seed) RelightRegion(seed);
    }

                                                                                       

    private bool SetLightIfBrighter(Vector3i pos, byte level)
    {
        if (level == 0) return false;
                                                                                
                                                                                     
                                                                              
                                                                         
        lock (_worldDataLock)
        {
            if (_blockLight.TryGetValue(pos, out var existing) && existing >= level) return false;
            _blockLight[pos] = level;
            return true;
        }
    }

    private void PropagateLight(Queue<Vector3i> queue)
    {
        while (queue.Count > 0)
        {
            var pos = queue.Dequeue();
            byte level = GetLight(pos);
            if (level <= 1) continue;                                                                           

            foreach (var dir in LightNeighbors6)
            {
                var next = pos + dir;
                if (BlockData.BlocksLight(GetBlock(next), dir)) continue;                                                             
                if (SetLightIfBrighter(next, (byte)(level - 1))) queue.Enqueue(next);
            }
        }
    }

    private void MarkChunksDirtyInBox(int minX, int maxX, int minZ, int maxZ)
    {
        var minCoord = ChunkCoordOf(minX, minZ);
        var maxCoord = ChunkCoordOf(maxX, maxZ);
        for (int cx = minCoord.X; cx <= maxCoord.X; cx++)
        for (int cz = minCoord.Y; cz <= maxCoord.Y; cz++)
            MarkChunkDirty(new Vector2i(cx, cz));
    }
}
