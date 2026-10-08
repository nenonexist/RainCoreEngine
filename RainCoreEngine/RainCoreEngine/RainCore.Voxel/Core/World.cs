using System.Text.Json;
using OpenTK.Mathematics;

namespace RainCore;

             
                                                                
                                                                            
                                                                       
                                                                         
                                                
                                                                          
                                                                     
                                                              
                                                               
   
                                                                    
                                                                            
                                                                        
                                                                           
                                                                                   
              
public partial class World
{
    public bool IsNearWorkbench(Vector3 position, int radius = 3)
    {
        var center = new Vector3i((int)MathF.Floor(position.X), (int)MathF.Floor(position.Y), (int)MathF.Floor(position.Z));
        for (int x = -radius; x <= radius; x++)
            for (int y = -radius; y <= radius; y++)
                for (int z = -radius; z <= radius; z++)
                    if (GetBlock(center + new Vector3i(x, y, z)) == BlockType.Workbench)
                        return true;
        return false;
    }
                                                                            
                                                                              
                                                                           
                                                                        
                                                                           
                                                                         
                                                                         
                                                         
    private readonly Dictionary<Vector2i, Chunk> _chunks = new();
    private readonly HashSet<Vector3i> _structureBlocks = new();

                                                                                        
                                                                           
                                                                                         
                                                                            
                                                                              
                                                                                     
                                                                                     
                                                                                   
                                                                                 
                                                                               
                                                                                  
                                                                               
                                                                                    
                                                                                  
                                               
    private readonly object _worldDataLock = new();

                                                                                 
                                                                                  
                                                                                  
                                                                              
                                                                                   
                                                                                   
                                                                             
                                                                               
                                                                                   
                                                                                 
                                                                                 
                                                                                     
                                                                                
                                                
                                                                                  
                                                                               
                                                                                     
                                                                               
                                                                                    
                                                                                
                                                                              
                                                                                    
                                                                                 
                                                                              
                                                                                     
                                                                  
                                                                                        
    private readonly object _generationLock = new();

                                                                                              
                                                                                              
                                                                                            
                                                                                          
                                                                                          
                                                                                       
                                                                                          
                                                                                        
                                                                                              
    private readonly Dictionary<Vector2i, int> _surfaceHeightCache = new();

    private readonly WorldEntityStore _entities = new();

                                                                                                
                                                                                           
                                                                                                 
    public int BlockCount { get { lock (_worldDataLock) return _chunks.Values.Sum(c => c.BlockCount); } }

                                                                                                              
    public int VertexCount { get { lock (_worldDataLock) return _chunks.Values.Sum(c => c.VertexCount); } }

    public IReadOnlyList<Npc> Npcs => _entities.Npcs;
    public IReadOnlyList<InteractableObject> Interactables => _entities.Interactables;

                                                                                  
                                                                     
                                                                              
                                                                              
                                                                             
                                                   
    public IReadOnlyList<ModelInstance> Models => _entities.Models;

    public Vector3i PortalPosition { get; private set; }
    public string DreamName { get; }

    public IReadOnlyList<TriggerZone> Triggers => _entities.Triggers;

                                                                                       
                                                                            
                                                                                    
                                                         
    public IReadOnlyList<MoodEvent> MoodEvents => _entities.MoodEvents;

                                                                                        
                                                                                    
                                                             
    public IReadOnlyList<MoodZone> MoodZones => _entities.MoodZones;

                                                                         
    public IReadOnlyList<ItemDefinition> Items => _entities.Items;

    public const float WorldRadius = 110f;
    public const int SeaLevel = 0;

                                                                                      
                                                                                    
    private const int CityGroundLevel = 2;

                                                                                        
                                                                             
                                                                                    
                                                                                   
                                                                                    
                                                          
    private const int BedrockLevel = -48;

                                                                                        
                                                                                
                                                                                 
                                                                                   
                                                                               
                                                                                   
                                        
    public const int MaxBuildHeight = 160;
                                                                                                                     
    public const int ChunkSize = 16;

    private readonly int _seed;
    private readonly Vector3 _tint;
    private readonly WorldTheme _theme;
    private readonly WorldKind _kind;

                                                                                
                                                                                  
                                                                                    
                                                                                         
    private readonly float _flatAreaRadius;
    private readonly Dictionary<Vector2i, Vector3> _waterColorCache = new();

    private float _noiseOffsetX, _noiseOffsetZ;

    public WorldTheme Theme => _theme;
    public WorldKind Kind => _kind;

                                                                                    
                                                                                     
                                                                                      
                                                                                      
                                                                                       
                                                                                          
                                                                                   
                                                                                     
                                                                                   
                                                                                
    public const float RandomDreamWorldRadius = 3000f;

                                                                                   
                                                                                      
                                                                              
                                                                                    
                                                                        
    private readonly float _effectiveRadius;
    public float EffectiveRadius => _effectiveRadius;

                                                                                      
                                                                                  
                                                                           
    private readonly bool _streaming;

                                                                                       
                                                                              
                                                                                      
                                                                                             
    private readonly IDreamContent _content;

                                                                                    
                                                                               
                                                                                     
                                                                                   
                                                                                
                                                                                 
                                                                                  
                                                                                   
    private float _weatherCeiling;
    public float WeatherCeiling => _weatherCeiling;
    public void SetWeatherCeiling(float value) => _weatherCeiling = Math.Clamp(value, 0f, 1f);

                                                                                  
                                                                                
                                                                              
                                                                                
                                                                                
                                                                                
                                     
    private float _weatherFloor = 0.55f;
    public float WeatherFloor => _weatherFloor;
    public void SetWeatherFloor(float value) => _weatherFloor = Math.Clamp(value, 0f, 1f);

                 
                                                                                
                                                                                 
                                                                                 
                                                                             
                                                                        
                                                                                   
                                                                                    
                                                                                      
                                                                                      
                                                                              
                  
    public HashSet<string> DisabledMusicTracks { get; } = new(StringComparer.OrdinalIgnoreCase);

                                                                                   
                                                                                
                                                                                     
                                                                                       
                                                                                      
                                                          
    public List<string> MusicTrackOrder { get; } = new();

                                                                                   
                                                                                        
                                                                                       
                                                                         
    public HashSet<string> DisabledAmbientTracks { get; } = new(StringComparer.OrdinalIgnoreCase);

                                                                                
    public List<string> AmbientTrackOrder { get; } = new();

                                                                                
                                                                                   
                                                                                     
                                                                                      
                                                                                   
    public string? InteriorId { get; }

                                                                                       
                                                                                     
                                     
    public int Seed => _seed;

                                                                                 
                                                                           
                                                                                  
    public WorldBounds Bounds { get; private set; }

                                                                                      
                                                                                 
                                                                                           
    private WorldBounds? _boundsOverride;

                                                                                
                                                                                  
                                                                                 
                                                                                   
                                                                                  
                                                                                   
                                                                         
    public void SetBoundsOverride(WorldBounds bounds) => _boundsOverride = bounds;

                                                                                      
                                                                                  
                                        
    public Vector3? SpawnOverride { get; private set; }

                                                                                        
                                                                                 
                                                                                                   
    public void SetSpawnOverride(Vector3 pos) => SpawnOverride = pos;

                                                                                      
                                                                                     
                                                                                    
                                                                                    
                                    
                                                                                     
                                                                                  
                                                                                  
                                                                                            
    public Vector3 ResolveSpawnPosition(float eyeHeight) => SpawnOverride ?? new Vector3(0, GetSurfaceHeight(0, 0) + 1 + eyeHeight, 0);

                                                                                 
    private readonly WorldEditStore _savedEdits = new();

                                                                         
                                                                             
                                                                     
                                                                        
                                                                          
                                             
    private string SaveDirectory => InteriorId != null
        ? Path.Combine(AppContext.BaseDirectory, "Saves", "interiors", SanitizeForPath(InteriorId))
        : Path.Combine(AppContext.BaseDirectory, "Saves", SanitizeForPath(DreamName));

    private string ChunkSaveFilePath(Vector2i coord)
    {
        return Path.Combine(SaveDirectory, $"chunk_{coord.X}_{coord.Y}.json");
    }

                                                                      
                                                                                        

    private static string LegacySaveFilePath => Path.Combine(AppContext.BaseDirectory, "Saves", "world_edits.json");

                                                                                      
                                                                                          
                                                                           
    public static string SanitizeForPath(string name)
    {
        return PathUtils.SanitizeForPath(name);
    }

                 
                                                                               
                                                                 
                                                                         
                                                                            
                                                                        
                                                                     
                                                                         
                  
    private record struct SavedBlockEdit(int X, int Y, int Z, BlockType Type, BlockShape Shape = BlockShape.Cube, byte LiquidLevel = 0);
    private record struct SavedInteractableState(string Id, bool IsActive);

    public World(int seed, string dreamName, Vector3 tint, WorldTheme theme, IDreamContent content,
        WorldKind kind = WorldKind.ProceduralIsland, float? flatAreaRadius = null, string? interiorId = null,
        float weatherCeiling = 1f, float? proceduralRadius = null)
    {
        _seed = seed;
        DreamName = dreamName;
        _tint = tint;
        _theme = theme;
        _kind = kind;
        _flatAreaRadius = flatAreaRadius ?? WorldRadius;
        InteriorId = interiorId;
        _weatherCeiling = Math.Clamp(weatherCeiling, 0f, 1f);
        _content = content;
        _streaming = kind == WorldKind.ProceduralIsland;
                                                                              
                                                                                   
                                                                              
        _effectiveRadius = _streaming ? (proceduralRadius ?? RandomDreamWorldRadius) : WorldRadius;

        GenerateTerrain();
        content.BuildStructures(this);
        content.BuildNpcsAndPortal(this);
        LoadEdits();                                                                                        
        RelightAll();                                                                            
        SeedLiquidSourcesFromSavedEdits();                                                                                                    
        Bounds = _boundsOverride ?? ComputeBounds();
        LoadInteractableStates();
        LoadModelPlacements();
        LoadPosterPlacements();                                                                                                
        LoadWorldSettings();
        LoadItemCatalog();
        _entities.Triggers.AddRange(content.BuildTriggers(this));

                                                                                    
                                                                                        
                                                                                       
                                                                                      
                                                                                            
                                                                             
        StartBackgroundGeneration();
    }

                 
                                                                             
                                                                           
                                                                           
                                                                         
                                                                   
                                                                            
                                     
                  
    private WorldBounds ComputeBounds()
    {
                                                                                     
                                                                                      
                                                                                      
                                                                                        
                                                                                       
                                                                       
        if (_streaming)
        {
            float r = _effectiveRadius;
            return new WorldBounds(-r, r, -r, r);
        }

        int minCx = int.MaxValue, maxCx = int.MinValue;
        int minCz = int.MaxValue, maxCz = int.MinValue;

                                                                                   
                                                                                     
                                                                                
        lock (_worldDataLock)
        {
            if (_chunks.Count == 0)
                return new WorldBounds(-ChunkSize, ChunkSize, -ChunkSize, ChunkSize);

            foreach (var coord in _chunks.Keys)
            {
                if (coord.X < minCx) minCx = coord.X;
                if (coord.X > maxCx) maxCx = coord.X;
                if (coord.Y < minCz) minCz = coord.Y;
                if (coord.Y > maxCz) maxCz = coord.Y;
            }
        }

        return new WorldBounds(
            minCx * ChunkSize,
            (maxCx + 1) * ChunkSize,
            minCz * ChunkSize,
            (maxCz + 1) * ChunkSize);
    }



                                                                                                                                                    
    public void AddTrigger(TriggerZone trigger)
    {
        _entities.Triggers.Add(trigger);
    }

                                                                                   
                                                                                   
                                                                                     
                                                                                                 
    public bool RemoveTrigger(TriggerZone trigger)
    {
        return _entities.Triggers.Remove(trigger);
    }

                                                                                          
                                                                                                     
    public void AddMoodEvent(MoodEvent moodEvent)
    {
        _entities.MoodEvents.Add(moodEvent);
    }

                                                                                     
                                                                            
    public void RemoveMoodEvent(MoodEvent moodEvent)
    {
        _entities.MoodEvents.Remove(moodEvent);
    }

                                                                                   
                                                                                     
                                    
    public void AddMoodZone(MoodZone moodZone)
    {
        _entities.MoodZones.Add(moodZone);
    }

                                                                                  
    public void RemoveMoodZone(MoodZone moodZone)
    {
        _entities.MoodZones.Remove(moodZone);
    }

                                                                                            
                                                  
    public ItemDefinition? GetItemDefinition(string name)
    {
        return _entities.Items.FirstOrDefault(item => string.Equals(item.Name, name, StringComparison.OrdinalIgnoreCase));
    }

                                                                                       
                                                                                
                                                                                     
                                                                                        
                                                                                           
    public ItemDefinition EnsureItemDefinition(string name)
    {
        var existing = GetItemDefinition(name);
        if (existing != null) return existing;
        var created = new ItemDefinition(name);
        _entities.Items.Add(created);
        return created;
    }

                                                                                      
                                                                                     
                                                                              
    public ItemDefinition AddItemDefinition(string name)
    {
        return EnsureItemDefinition(name);
    }

                                                                                           
                                                                                     
                                                                                
                                        
    public bool RenameItemDefinition(ItemDefinition item, string newName)
    {
        if (string.IsNullOrWhiteSpace(newName)) return false;
        item.Name = newName.Trim();
        return true;
    }

                                                                                                  
    public void RemoveItemDefinition(ItemDefinition item)
    {
        _entities.Items.Remove(item);
    }

    public BlockType GetBlock(Vector3i pos)
    {
        var coord = ChunkCoordOf(pos.X, pos.Z);
                                                                                        
                                                                                       
                                                                                       
                                                                                      
                                                                                       
                                                                                    
        if (_streaming)
        {
            bool generated;
            lock (_worldDataLock) { generated = _generatedChunks.Contains(coord); }
            if (!generated)
            {
                QueueChunkLoad(coord);
                return BlockType.Air;
            }
        }

                                                                                
                                                                           
                                                                                   
                                                                           
        Chunk? chunk;
        lock (_worldDataLock) { _chunks.TryGetValue(coord, out chunk); }

        if (chunk != null)
        {
            var stored = chunk.GetLocal(pos);
            if (stored != BlockType.Air) return stored;
        }

                                                                            
                                                                              
                                                                                    
        if ((_kind is WorldKind.FlatCity or WorldKind.RoguelikeSnow or WorldKind.RoguelikeDungeon) && IsImplicitStoneColumn(pos) &&
            !(_savedEdits.TryGetValue(pos, out var edited) && edited == BlockType.Air))
        {
            return BlockType.Stone;
        }

        return BlockType.Air;
    }

                 
                                                                       
                                                                        
                                                                
                                                                     
                        
                  
    public void SetBlock(Vector3i pos, BlockType type)
    {
        if (type == BlockType.Air && GetBlock(pos) == BlockType.Snow)
        {
            RemoveSnowLayer(pos);
            return;
        }

        if (type == BlockType.Snow && GetBlock(pos) == BlockType.Snow)
        {
            AddSnowLayer(pos);
            return;
        }

        SetBlockSilent(pos, type);
        var defaultShape = BlockData.GetDefaultShape(type);
        if (defaultShape != BlockShape.Cube) SetBlockShape(pos, defaultShape);
        _savedEdits[pos] = type;
        RelightRegion(pos);                                                                                                     
        MarkLiquidDirty(pos);                                                                                                         
    }

                                                                                                 
    public BlockShape GetBlockShape(Vector3i pos)
    {
        var coord = ChunkCoordOf(pos.X, pos.Z);
        if (_streaming)
        {
            bool generated;
            lock (_worldDataLock) { generated = _generatedChunks.Contains(coord); }
            if (!generated)
            {
                QueueChunkLoad(coord);
                return BlockShape.Cube;
            }
        }
        Chunk? chunk;
        lock (_worldDataLock) { _chunks.TryGetValue(coord, out chunk); }
        return chunk != null ? chunk.GetLocalShape(pos) : BlockShape.Cube;
    }

    public bool AddSnowLayer(Vector3i pos)
    {
        if (GetBlock(pos) != BlockType.Snow) return false;

        var currentShape = GetBlockShape(pos);
        int currentLayer = BlockShapeToSnowLayer(currentShape);
        if (currentLayer >= 8) return false;

        SetBlockShape(pos, SnowLayerToBlockShape(currentLayer + 1));
        _savedEdits[pos] = BlockType.Snow;
        return true;
    }

    public bool RemoveSnowLayer(Vector3i pos)
    {
        if (GetBlock(pos) != BlockType.Snow) return false;

        int currentLayer = BlockShapeToSnowLayer(GetBlockShape(pos));
        if (currentLayer <= 1)
        {
            SetBlockSilent(pos, BlockType.Air);
            _savedEdits[pos] = BlockType.Air;
            RelightRegion(pos);
            MarkLiquidDirty(pos);
            return true;
        }

        SetBlockShape(pos, SnowLayerToBlockShape(currentLayer - 1));
        _savedEdits[pos] = BlockType.Snow;
        return true;
    }

    public float GetCollisionHeight(Vector3i pos)
    {
        var type = GetBlock(pos);
        if (!BlockData.IsSolid(type)) return 0f;
        if (type != BlockType.Snow) return 1f;

        return BlockShapeToSnowLayer(GetBlockShape(pos)) / 8f;
    }

    private static int BlockShapeToSnowLayer(BlockShape shape) => shape switch
    {
        BlockShape.SnowLayer => 1,
        BlockShape.SnowLayer2 => 2,
        BlockShape.SnowLayer3 => 3,
        BlockShape.SnowLayer4 => 4,
        BlockShape.SnowLayer5 => 5,
        BlockShape.SnowLayer6 => 6,
        BlockShape.SnowLayer7 => 7,
        BlockShape.SnowLayer8 => 8,
        _ => 8
    };

    private static BlockShape SnowLayerToBlockShape(int layer) => layer switch
    {
        1 => BlockShape.SnowLayer,
        2 => BlockShape.SnowLayer2,
        3 => BlockShape.SnowLayer3,
        4 => BlockShape.SnowLayer4,
        5 => BlockShape.SnowLayer5,
        6 => BlockShape.SnowLayer6,
        7 => BlockShape.SnowLayer7,
        _ => BlockShape.SnowLayer8
    };

                 
                                                                             
                                                                            
                                                                              
                                                                         
                                                                          
                                                                            
                                                             
                  
    public void SetBlockShape(Vector3i pos, BlockShape shape)
    {
        var coord = ChunkCoordOf(pos.X, pos.Z);
        var chunk = GetOrCreateChunk(coord);
        chunk.SetLocalShape(pos, shape);
        MarkBoundaryNeighborsDirty(pos, coord);
    }

                                                                                            
    private static Vector2i ChunkCoordOf(int x, int z)
    {
        return new(FloorDiv(x, ChunkSize), FloorDiv(z, ChunkSize));
    }


                 
                                                                           
                                                                            
                                                                          
                                                                            
                                     
                  
    private static int FloorDiv(int a, int b)
    {
        int q = a / b;
        if (a % b != 0 && ((a < 0) != (b < 0))) q--;
        return q;
    }

    private Chunk GetOrCreateChunk(Vector2i coord)
    {
        lock (_worldDataLock)
        {
            if (!_chunks.TryGetValue(coord, out var chunk))
            {
                chunk = new Chunk(coord);
                _chunks[coord] = chunk;
            }
            return chunk;
        }
    }

    private void SetBlockSilent(Vector3i pos, BlockType type)
    {
        if (_streaming && _generationDepth.Value > 0 && _structureBlocks.Count > 0 && _structureBlocks.Contains(pos))
            return;

        var coord = ChunkCoordOf(pos.X, pos.Z);
        var chunk = GetOrCreateChunk(coord);
        chunk.SetLocal(pos, type);
        MarkBoundaryNeighborsDirty(pos, coord);
                                                                                   
                                                                                      
                                                                                
                                                                                      
                                                             
        lock (_worldDataLock) { _surfaceHeightCache.Remove(new Vector2i(pos.X, pos.Z)); }
    }

                 
                                                                            
                                                                             
                                                                             
                                                                         
                                                                        
                  
    private void MarkBoundaryNeighborsDirty(Vector3i pos, Vector2i coord)
    {
        int localX = pos.X - coord.X * ChunkSize;
        int localZ = pos.Z - coord.Y * ChunkSize;

        if (localX == 0) MarkChunkDirty(new Vector2i(coord.X - 1, coord.Y));
        else if (localX == ChunkSize - 1) MarkChunkDirty(new Vector2i(coord.X + 1, coord.Y));

        if (localZ == 0) MarkChunkDirty(new Vector2i(coord.X, coord.Y - 1));
        else if (localZ == ChunkSize - 1) MarkChunkDirty(new Vector2i(coord.X, coord.Y + 1));
    }

    private void MarkChunkDirty(Vector2i coord)
    {
        Chunk? chunk;
        lock (_worldDataLock) { _chunks.TryGetValue(coord, out chunk); }
        chunk?.MarkDirty();
    }

                                                                                                                         
    public void SetPortal(Vector3i pos)
    {
        PortalPosition = pos;
        SetBlockSilent(pos, BlockType.Portal);
    }

                                                                                                                                                                
    public void AddNpc(Npc npc)
    {
        _entities.Npcs.Add(npc);
    }

    public void RemoveNpc(Npc npc)
    {
        _entities.Npcs.Remove(npc);
    }

                                                                                 
                                                                      
    public bool IsMusicTrackEnabled(string fileName) => !DisabledMusicTracks.Contains(fileName);

                                                                          
                                                                                      
                                                                                        
                                             
    public void SetMusicTrackEnabled(string fileName, bool enabled)
    {
        if (enabled) DisabledMusicTracks.Remove(fileName);
        else DisabledMusicTracks.Add(fileName);
    }

                                                                                   
                                                                        
    public void SetMusicTrackOrder(IEnumerable<string> order)
    {
        MusicTrackOrder.Clear();
        MusicTrackOrder.AddRange(order);
    }

                                                                                                             
    public bool IsAmbientTrackEnabled(string fileName) => !DisabledAmbientTracks.Contains(fileName);

                                                                                  
    public void SetAmbientTrackEnabled(string fileName, bool enabled)
    {
        if (enabled) DisabledAmbientTracks.Remove(fileName);
        else DisabledAmbientTracks.Add(fileName);
    }

                                                                                
    public void SetAmbientTrackOrder(IEnumerable<string> order)
    {
        AmbientTrackOrder.Clear();
        AmbientTrackOrder.AddRange(order);
    }

    public void ReplaceNpc(Npc oldNpc, Npc newNpc)
    {
        int index = _entities.Npcs.IndexOf(oldNpc);
        if (index >= 0)
            _entities.Npcs[index] = newNpc;
        else
            _entities.Npcs.Add(newNpc);
    }

    public void ReplaceNpc(Vector3i origin, Npc oldNpc, Npc newNpc)
    {
        ReplaceNpc(oldNpc, newNpc);
    }

                                                                                                                                                                                                                                                                                                                                          
    internal int GetSurfaceHeight(int x, int z)
    {
                                                                                       
                                                                                          
                                                                                        
                                                                                       
                                                                                       
                                                                                        
                                                                                         
                                                                                     
                                                                                     
                                                                                     
                                                                              
        var key = new Vector2i(x, z);
        lock (_worldDataLock)
        {
            if (_surfaceHeightCache.TryGetValue(key, out var cached))
                return cached;
        }

        if (_streaming)
        {
            var coord = ChunkCoordOf(x, z);
            bool generated;
            lock (_worldDataLock) { generated = _generatedChunks.Contains(coord); }
            if (!generated)
            {
                                                                          
                                                                                
                int predicted = (int)MathF.Floor(ComputeHeightSmoothed(x, z, _noiseOffsetX, _noiseOffsetZ));
                lock (_worldDataLock) { _surfaceHeightCache[key] = predicted; }
                return predicted;
            }
        }

                                                                               
                                                                           
                                                                               
                                                                                   
                                                                                  
                                                                                    
        const int scanTop = MaxBuildHeight;
        int result = BedrockLevel;
        for (int y = scanTop; y >= BedrockLevel; y--)
        {
            if (GetBlock(new Vector3i(x, y, z)) != BlockType.Air)
            {
                result = y;
                break;
            }
        }

        lock (_worldDataLock) { _surfaceHeightCache[key] = result; }
        return result;
    }

                                                                                                                                                                                                                                                                                      
    internal bool IsSolidAt(Vector3i pos)
    {
        return GetCollisionHeight(pos) > 0f;
    }

    internal bool IsPortalAt(Vector3i feetBlock)
    {
        return GetBlock(feetBlock) == BlockType.Portal;
    }

                                                                             
                                                                                     
    internal bool IsDreamGateAt(Vector3i feetBlock)
    {
        return GetBlock(feetBlock) == BlockType.DreamGate;
    }

}
