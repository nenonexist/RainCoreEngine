using OpenTK.Mathematics;

namespace RainCore;

             
                                                                              
                                                                              
                                                                         
                                                                             
                                                     
   
                             
                                                                          
                                                                           
                                                                           
                                                                            
                                                                              
                                                               
                                                                              
                                                                              
                                                                              
                                                                          
   
                                                                   
                                                                            
                                                                             
                                                                          
                                                                        
                                                                    
                                                                   
   
                                                                      
                                                                              
                                                                           
                                                                              
                                                                   
                                                                              
                                                                        
                                                                            
                                                                          
                                                                          
                                                                           
                                                                            
                                                                                
                                                                               
                                                                       
                                                                         
                                            
                                                                     
                                                                      
                                                                     
                                                                            
                                                                           
                                                                       
                                                                                 
              
public partial class World
{
                                                                                  
                                                                            
                                                                              
                                                                                    
    private const int StreamingLoadRadiusChunks = 9;

                                                                        
                                                                             
                                                                               
    private const int StreamingUnloadRadiusChunks = 16;

                                                                                    
                                                                                
    private readonly WorldStreamingState _streamingState = new();
    private HashSet<Vector2i> _generatedChunks => _streamingState.GeneratedChunks;
    private ThreadLocal<int> _generationDepth => _streamingState.GenerationDepth;
    private Queue<Vector2i> _pendingChunkLoads => _streamingState.PendingChunkLoads;
    private HashSet<Vector2i> _queuedChunkLoads => _streamingState.QueuedChunkLoads;
    private Vector2i? _lastStreamedChunk
    {
        get => _streamingState.LastStreamedChunk;
        set => _streamingState.LastStreamedChunk = value;
    }
    private ManualResetEventSlim _chunkGenSignal => _streamingState.ChunkGenSignal;
    private Thread? _chunkGenThread
    {
        get => _streamingState.ChunkGenThread;
        set => _streamingState.ChunkGenThread = value;
    }
    private bool _chunkGenShuttingDown
    {
        get => _streamingState.ChunkGenShuttingDown;
        set => _streamingState.ChunkGenShuttingDown = value;
    }

                                                                                       
                                                                                  
                                                                                  
                                                                                  
                                                                                       
                                                                                 
                                                                            
                                                                                  
                                                                                   
                                                                               
                                                                                 
                                                                                  
                                                                                 
                                                                                          
                                                                                     
                                                                          
                                                                                      
                                                                             
                                                                                   
                                                                                    
                                                                              
                                                                                     
                                                                                     
                                                                                   
                                                                                    
                                                                                      
                                                                                
                                                                            
                                                                       
                                                                                    
                                                                                      
                                                                                            
                                                                                            
                                                                                    
                                                                                       
                                                                    
    private const int StreamingChunksPerFrameBudget = 3;

                                                                                  
                                                                                   
                                                                                   
                                                                                 
                                                                                   
                                                                              
                                                                                  
                                                                               
                                                                                   
                                                                                   
                                                                                 
                                                                               
                                                                                
                                                                                 
                                                                                
                                                                                
                                                      
    private const int ImmediateStreamingChunkBudget = 25;

                                                                                 
                                                                            
                                                                             
                                                                     
    private void StartBackgroundGeneration()
    {
        if (!_streaming) return;

        _chunkGenThread = new Thread(BackgroundGenerationLoop)
        {
            IsBackground = true,                                                       
                                                                                           
            Name = $"ChunkGen:{DreamName}",
            Priority = ThreadPriority.BelowNormal,
        };
        _chunkGenThread.Start();
    }

                                                                                   
                                                                                    
                                                                                
                                                                                    
                                                                                  
                                                     
    private void BackgroundGenerationLoop()
    {
        while (!_chunkGenShuttingDown)
        {
            Vector2i? coord = null;
            lock (_worldDataLock)
            {
                if (_pendingChunkLoads.Count > 0)
                {
                    coord = _pendingChunkLoads.Dequeue();
                    _queuedChunkLoads.Remove(coord.Value);
                }
                else
                {
                    _chunkGenSignal.Reset();
                }
            }

            if (coord == null)
            {
                                                                                     
                                                                                     
                                                                                   
                                                                                  
                                                                                
                                                                            
                _chunkGenSignal.Wait(100);
                continue;
            }

            EnsureChunkGenerated(coord.Value);
        }
    }

                                                                                  
                                                                                   
                                                                                     
                                                                                      
                                                                              
                                                                                    
                                                                            
                                                                        
                                                                                 
    public void ShutdownBackgroundGeneration()
    {
        if (_chunkGenThread == null) return;

        _chunkGenShuttingDown = true;
        _chunkGenSignal.Set();
        _chunkGenThread.Join(TimeSpan.FromSeconds(2));
        _chunkGenThread = null;
    }

                                                                                   
                                                                                      
                                                                            
                                                
                                                                                    
                                                                                   
                                                                                      
                                                                                    
                                                                               
                                                 
                                                                                     
                                                                                     
                                                                          
                                                                                  
                                                                                    
                                                                   
    public void UpdateStreaming(Vector3 playerPos, bool immediate = false)
    {
        if (!_streaming) return;

        var playerChunk = ChunkCoordOf((int)MathF.Floor(playerPos.X), (int)MathF.Floor(playerPos.Z));

        if (_lastStreamedChunk != playerChunk)
        {
            _lastStreamedChunk = playerChunk;

            lock (_worldDataLock)
            {
                var nearbyCoords = new List<(Vector2i Coord, int Distance)>(
                    (StreamingLoadRadiusChunks * 2 + 1) * (StreamingLoadRadiusChunks * 2 + 1));
                for (int dx = -StreamingLoadRadiusChunks; dx <= StreamingLoadRadiusChunks; dx++)
                    for (int dz = -StreamingLoadRadiusChunks; dz <= StreamingLoadRadiusChunks; dz++)
                        nearbyCoords.Add((new Vector2i(playerChunk.X + dx, playerChunk.Y + dz), dx * dx + dz * dz));

                nearbyCoords.Sort(static (a, b) => a.Distance.CompareTo(b.Distance));
                foreach (var entry in nearbyCoords)
                {
                    if (_generatedChunks.Contains(entry.Coord)) continue;
                    if (_queuedChunkLoads.Add(entry.Coord))
                        _pendingChunkLoads.Enqueue(entry.Coord);
                }

                var prioritizedLoads = new List<(Vector2i Coord, int Distance)>(_pendingChunkLoads.Count);
                while (_pendingChunkLoads.Count > 0)
                {
                    var coord = _pendingChunkLoads.Dequeue();
                    int cdx = coord.X - playerChunk.X;
                    int cdz = coord.Y - playerChunk.Y;
                    int distance = cdx * cdx + cdz * cdz;
                    if (Math.Max(Math.Abs(cdx), Math.Abs(cdz)) <= StreamingUnloadRadiusChunks)
                        prioritizedLoads.Add((coord, distance));
                    else
                        _queuedChunkLoads.Remove(coord);
                }

                prioritizedLoads.Sort(static (a, b) => a.Distance.CompareTo(b.Distance));
                foreach (var entry in prioritizedLoads)
                    _pendingChunkLoads.Enqueue(entry.Coord);

                                                                                 
                                                                                             
                _chunkGenSignal.Set();

                if (_chunks.Count > 0)
                {
                                                                                         
                                                                        
                    var toUnload = new List<Vector2i>();
                    foreach (var coord in _chunks.Keys)
                    {
                        int cdx = Math.Abs(coord.X - playerChunk.X);
                        int cdz = Math.Abs(coord.Y - playerChunk.Y);
                        if (Math.Max(cdx, cdz) > StreamingUnloadRadiusChunks)
                            toUnload.Add(coord);
                    }

                                                                                           
                                                           
                    foreach (var coord in toUnload)
                        UnloadChunk(coord);
                }
            }
        }

                                                                       
                                                                                  
                                                                                 
                                                                                  
                                                                                
                                                   
        if (!immediate) return;

                                                                                
                                                                              
                                                                             
                                                                                    
                                                                     
                                                                                
                                                                                 
                                                                          
        int generated = 0;
        while (generated < ImmediateStreamingChunkBudget)
        {
            Vector2i? coord = null;
            lock (_worldDataLock)
            {
                if (_pendingChunkLoads.Count > 0)
                {
                    coord = _pendingChunkLoads.Dequeue();
                    _queuedChunkLoads.Remove(coord.Value);
                }
            }

                                                                                     
                                                                                    
                             
            if (coord == null) break;

                                                                                         
                                                                                       
                                                                
            int cdx = Math.Abs(coord.Value.X - playerChunk.X);
            int cdz = Math.Abs(coord.Value.Y - playerChunk.Y);
            if (Math.Max(cdx, cdz) > StreamingUnloadRadiusChunks) continue;

            EnsureChunkGenerated(coord.Value);
            generated++;
        }
    }

                                                                                      
                                                                                 
                                                                                  
                                                                                  
                                                                                 
                                                                        
                                                                               
                                                                                 
                                                                   
                                                                               
                                                                                      
                                                                                   
                                                                                    
                                                                                   
                                                                                    
                                                                              
                                                                               
                                                                             
                                                                                        
                                                                              
                                                                                 
                                                                                
                                                                           
                                                                                  
                                                                                     
                                                                                    
                                                                        
    private void EnsureChunkGenerated(Vector2i coord)
    {
        bool alreadyDone;
        lock (_worldDataLock) { alreadyDone = _generatedChunks.Contains(coord); }
        if (alreadyDone) return;

                                                                                      
                                                                               
                                                                                 
                                                                                   
                                                                                    
                                                                           
                                                                                     
                                                                                    
                                          
        if (_generationDepth.Value > 0)
        {
            lock (_worldDataLock)
            {
                if (_queuedChunkLoads.Add(coord))
                    _pendingChunkLoads.Enqueue(coord);
                _chunkGenSignal.Set();
            }
            return;
        }

        lock (_generationLock)
        {
                                                                                  
                                                                                  
                                                                            
                                                                                
            lock (_worldDataLock)
            {
                if (_generatedChunks.Contains(coord)) return;
                _generatedChunks.Add(coord);
            }

            _generationDepth.Value++;
            try
            {
                var rng = new Random(ChunkSeed(coord));
                GenerateIslandChunk(coord, rng);

                                                                                           
                                                                                            
                                                                                            
                                                                                
                var structureRng = new Random(ChunkSeed(coord) ^ 0x5EED);
                _content.PopulateChunk(this, coord.X, coord.Y, structureRng);
            }
            finally
            {
                _generationDepth.Value--;
            }
        }
    }

    private void QueueChunkLoad(Vector2i coord)
    {
        lock (_worldDataLock)
        {
            if (_generatedChunks.Contains(coord) || !_queuedChunkLoads.Add(coord)) return;
            _pendingChunkLoads.Enqueue(coord);
            _chunkGenSignal.Set();
        }
    }

                                                                                      
                                                                                  
                                                
    private int ChunkSeed(Vector2i coord)
    {
        unchecked
        {
            int h = _seed;
            h = h * 486187739 + coord.X;
            h = h * 486187739 + coord.Y;
            return h;
        }
    }

                                                                                  
                                                                                    
                                                                                  
                                                                                    
                                                                      
    private void UnloadChunk(Vector2i coord)
    {
        if (_chunks.TryGetValue(coord, out var chunk))
        {
            chunk.ReleaseGpu();
            _chunks.Remove(coord);
        }
        _generatedChunks.Remove(coord);

                                                                                       
                                                                                        
                                                                                          
                                                                                        
                                                                                         
        int baseX = coord.X * ChunkSize;
        int baseZ = coord.Y * ChunkSize;
        _structureBlocks.RemoveWhere(pos =>
            pos.X >= baseX && pos.X < baseX + ChunkSize &&
            pos.Z >= baseZ && pos.Z < baseZ + ChunkSize);
        for (int lx = 0; lx < ChunkSize; lx++)
            for (int lz = 0; lz < ChunkSize; lz++)
                _surfaceHeightCache.Remove(new Vector2i(baseX + lx, baseZ + lz));
    }

                                                                                    
                                                                                    
                                                                                  
                                                                                   
                                       
    public void RelightArea(Vector3i center) => RelightRegion(center);
}
