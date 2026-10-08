using OpenTK.Mathematics;

namespace RainCore;

             
                                                                 
                                                                           
                                                                   
   
                                                                            
                                                                           
                             
                                                                            
                                                              
                                                                      
                                                                           
                                                                        
   
                                                                            
                                                                            
                                                     
                                                                             
                                                                         
                                                     
                                                                         
                                                                         
                            
   
                                                                            
                                                                          
                                                                   
                                                                        
                                                                       
                                                                         
                                                                          
                                                                           
                                              
              
public partial class World
{
                                                                                                                     
    public const int MaxFlowLevel = 7;

    private static readonly Vector3i[] HorizontalNeighbors4 =
    {
        new(1, 0, 0), new(-1, 0, 0), new(0, 0, 1), new(0, 0, -1)
    };

                                                                                        
                                                                                       
                                                                                        
                             
    private readonly Dictionary<Vector3i, byte> _liquidLevel = new();

                                                                                        
                                                                                       
                                                                                       
                              
    private readonly HashSet<Vector3i> _dirtyLiquid = new();
    private readonly List<Vector3i> _liquidTickBatch = new();

    private float _liquidTickAccum;
    private const float LiquidTickInterval = 0.25f;                                                          

                                                                                               
                                                                                     
                                                                                                              
    private readonly List<Vector3i> _liquidRelightBatch = new();

                                                                                          
                                                                                       
                                                                                        
                                                                                             
    public byte GetLiquidLevel(Vector3i pos) => _liquidLevel.TryGetValue(pos, out var l) ? l : (byte)0;

                 
                                                                              
                                                                           
                                                                          
                                                                           
                                                                             
                  
    public void MarkLiquidDirty(Vector3i pos)
    {
        _dirtyLiquid.Add(pos);
        foreach (var n in LightNeighbors6) _dirtyLiquid.Add(pos + n);
    }

                                                                                   
                                                                                    
                                  
    public void UpdateLiquids(float dt)
    {
        if (_dirtyLiquid.Count == 0) return;

        _liquidTickAccum += dt;
        if (_liquidTickAccum < LiquidTickInterval) return;
        _liquidTickAccum = 0f;

        TickLiquids();
    }

    private void TickLiquids()
    {
                                                                             
                                                                               
                                                                               
                           
        _liquidTickBatch.Clear();
        _liquidTickBatch.AddRange(_dirtyLiquid);
        _dirtyLiquid.Clear();

                                                                     
                                                                               
                                                                           
                                                                                
                                                                  
                                                     
        _liquidRelightBatch.Clear();
        foreach (var pos in _liquidTickBatch)
            RecomputeLiquidCell(pos);
        RelightRegions(_liquidRelightBatch);
    }

                 
                                                                           
                                                                       
                                                                           
                                                                        
                                                                          
                                                                            
                                                        
                  
    private void RecomputeLiquidCell(Vector3i pos)
    {
        var currentType = GetBlock(pos);

                                                                             
                                                                            
        byte currentLevel = 0;
        if (currentType == BlockType.Water && !_liquidLevel.TryGetValue(pos, out currentLevel)) return;

        if (BlockData.IsSolid(currentType))
        {
                                                                               
                                                                                  
            if (_liquidLevel.Remove(pos))
                MarkChunkDirtyAt(pos);
            return;
        }

        if (currentType != BlockType.Air && currentType != BlockType.Water)
        {
                                                                               
                                                                                 
                                                                                 
                                                                                
                                                                              
                                                                           
                                                           
            if (_liquidLevel.Remove(pos))
                MarkChunkDirtyAt(pos);
            return;
        }

        byte? best = FindBestSupply(pos);

        if (best == null)
        {
                                                                          
            if (currentType != BlockType.Water) return;
            _liquidLevel.Remove(pos);
            SetBlockSilent(pos, BlockType.Air);                                                                                                   
            MarkChunkDirtyAt(pos);
            _liquidRelightBatch.Add(pos);                                                                                    
            MarkLiquidDirty(pos);                                                              
            return;
        }

        if (currentType == BlockType.Air)
        {
            SetBlockSilent(pos, BlockType.Water);
            if (best.Value > 0) _liquidLevel[pos] = best.Value;
            MarkChunkDirtyAt(pos);
            _liquidRelightBatch.Add(pos);                                                                                             
            MarkLiquidDirty(pos);
            return;
        }

                                                                                  
                                                                 
        byte existing = currentLevel;
        if (best.Value == existing) return;

        if (best.Value > 0) _liquidLevel[pos] = best.Value;
        else _liquidLevel.Remove(pos);                                                                                                                                                                                                                                           
        MarkChunkDirtyAt(pos);
        MarkLiquidDirty(pos);
    }

                 
                                                                             
                                                                                 
                                                                                 
                                                                                   
                                                                                 
                                                                                  
                                                                                  
                                                                                
                                                                              
                                                                               
                                                                                
                                                                                
                                             
                  
    private void SeedLiquidSourcesFromSavedEdits()
    {
        foreach (var (pos, type) in _savedEdits)
            if (type == BlockType.Water) MarkLiquidDirty(pos);
    }

                 
                                                                          
                                                                            
                                                                               
                               
                  
    private byte? FindBestSupply(Vector3i pos)
    {
        byte? best = null;

        var abovePos = pos + new Vector3i(0, 1, 0);
        if (HasWaterAt(abovePos, out var aboveLevel))
            best = aboveLevel;                                                                      

        if (best == 0) return best;                                              

        foreach (var n in HorizontalNeighbors4)
        {
            var neighborPos = pos + n;
            if (!HasWaterAt(neighborPos, out var neighborLevel) || neighborLevel >= MaxFlowLevel) continue;

                                                                       
                                                                       
                                                       
            var belowNeighbor = neighborPos + new Vector3i(0, -1, 0);
            if (!BlockData.IsSolid(GetBlock(belowNeighbor))) continue;

            byte candidate = (byte)(neighborLevel + 1);
            if (best == null || candidate < best.Value) best = candidate;
        }

        return best;
    }

    private bool HasWaterAt(Vector3i pos, out byte level)
    {
        if (GetBlock(pos) == BlockType.Water)
        {
            level = GetLiquidLevel(pos);
            return true;
        }
        level = 0;
        return false;
    }

                                                                                 
                                                                               
                                                                        
                                                                               
                                                                               
                                                                            
                                                          
    public bool IsPositionUnderwater(Vector3 worldPos)
    {
        var block = new Vector3i((int)MathF.Floor(worldPos.X), (int)MathF.Floor(worldPos.Y), (int)MathF.Floor(worldPos.Z));
        return GetBlock(block) == BlockType.Water;
    }

                                                                                         
                                                                                       
                                                                             
                                                                                
    private void MarkChunkDirtyAt(Vector3i pos)
    {
        var coord = ChunkCoordOf(pos.X, pos.Z);
        MarkChunkDirty(coord);
        MarkBoundaryNeighborsDirty(pos, coord);
    }
}
