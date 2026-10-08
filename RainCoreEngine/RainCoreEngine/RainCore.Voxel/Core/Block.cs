using OpenTK.Mathematics;

namespace RainCore;

             
                                                                              
                                                                   
                                                                           
                                                                   
                                                                     
                                                                          
                                                                                    
              
public enum BlockType : byte
{
    Air = 0,
    Grass,
    Dirt,
    Stone,
    Wood,
    Leaves,
    Sand,
    Water,
    Portal,                                                                  
    NpcBody,                                                                          
                                                                                             
                                                                                            
                                                                                               
    NpcHead,

                                        
    GrassAutumn,                                    
    Snow,                                              
    LeavesAutumnOrange,                               
    LeavesAutumnYellow,                            
    LeavesSnowy,                                     

                                                                    
    Path,                          
    Window,                            
    LampLight,                                      

                                                                
                                                                                   
                                                                                   
                                                                                  
                                                                               
                                                                                      
                                                                              
                                                                                      
                                                                                 
                                                                                 
                                                                                  
                                                                                    
    DreamGate,

                                                                                
                                                                                                
                                                                                      
                                                                
    MossStone,
                                                                                        
                                                                                         
    WildFlowers,
                                                                                     
                                                                                       
                                                                                  
    CrystalShard,

                                                                               
                                                                             
                                                                                 
                            
    Mud,
    Workbench,
    StoneSlab,
    Sandstone,
    Torch,
    DarkWood,
    LeavesDark,
    CherryWood,
    CherryLeaves,
    WillowLeaves,
    GlassPane,
    Lantern,
    OakLeaves,
    AspenLeaves,
    MapleLeaves,
    AspenWood,
    MapleWood

                                                                                     
                                                                            
                                                                        
                                                                         
}

public static class BlockData
{
                 
                                                                           
                                                                            
                                                                       
                  
    public static Vector3 GetColor(BlockType type)
    {
        return type switch
        {
            BlockType.Grass => new Vector3(0.36f, 0.68f, 0.29f),
            BlockType.Dirt => new Vector3(0.55f, 0.38f, 0.24f),
            BlockType.Stone => new Vector3(0.55f, 0.55f, 0.55f),
            BlockType.Wood => new Vector3(0.45f, 0.30f, 0.16f),
            BlockType.Leaves => new Vector3(0.18f, 0.5f, 0.18f),
            BlockType.Sand => new Vector3(0.86f, 0.79f, 0.55f),
            BlockType.Water => new Vector3(0.20f, 0.45f, 0.85f),
            BlockType.Portal => new Vector3(0.75f, 0.35f, 0.95f),
            BlockType.NpcBody => new Vector3(0.85f, 0.55f, 0.25f),
            BlockType.NpcHead => new Vector3(0.93f, 0.78f, 0.60f),
            BlockType.GrassAutumn => new Vector3(0.85f, 0.45f, 0.10f),
            BlockType.Snow => new Vector3(0.95f, 0.95f, 0.98f),
            BlockType.LeavesAutumnOrange => new Vector3(0.80f, 0.40f, 0.08f),
            BlockType.LeavesAutumnYellow => new Vector3(0.90f, 0.75f, 0.15f),
            BlockType.LeavesSnowy => new Vector3(0.88f, 0.92f, 0.97f),
            BlockType.Path => new Vector3(0.42f, 0.40f, 0.38f),
            BlockType.Window => new Vector3(1f, 0.85f, 0.45f),
            BlockType.LampLight => new Vector3(1f, 0.92f, 0.65f),
            BlockType.DreamGate => new Vector3(0.30f, 0.85f, 0.80f),                                             
            BlockType.MossStone => new Vector3(0.45f, 0.50f, 0.40f),
            BlockType.WildFlowers => new Vector3(0.45f, 0.75f, 0.30f),
            BlockType.CrystalShard => new Vector3(0.60f, 0.90f, 0.95f),
            BlockType.Mud => new Vector3(0.30f, 0.26f, 0.16f),
            BlockType.Workbench => new Vector3(0.52f, 0.32f, 0.14f),
            BlockType.StoneSlab => new Vector3(0.52f, 0.51f, 0.49f),
            BlockType.Sandstone => new Vector3(0.78f, 0.65f, 0.38f),
            BlockType.Torch => new Vector3(1.0f, 0.62f, 0.16f),
            BlockType.DarkWood => new Vector3(0.16f, 0.10f, 0.08f),
            BlockType.LeavesDark => new Vector3(0.08f, 0.12f, 0.10f),
            BlockType.CherryWood => new Vector3(0.55f, 0.26f, 0.24f),
            BlockType.CherryLeaves => new Vector3(0.95f, 0.45f, 0.66f),
            BlockType.WillowLeaves => new Vector3(0.42f, 0.68f, 0.36f),
            BlockType.GlassPane => new Vector3(0.78f, 0.92f, 0.98f),
            BlockType.Lantern => new Vector3(1.0f, 0.68f, 0.22f),
            BlockType.OakLeaves => new Vector3(0.16f, 0.48f, 0.16f),
            BlockType.AspenLeaves => new Vector3(0.92f, 0.62f, 0.12f),
            BlockType.MapleLeaves => new Vector3(0.78f, 0.18f, 0.08f),
            BlockType.AspenWood => new Vector3(0.68f, 0.54f, 0.34f),
            BlockType.MapleWood => new Vector3(0.42f, 0.16f, 0.10f),
            _ => BlockRegistry.TryGetDefinition(type, out var def) ? def.Color : Vector3.Zero
        };
    }

                                                                            
                                                                                
                                                                          
                                                                         
                                                                      
                                                                         
    public static readonly Vector3 WaterColorWarm = new(0.10f, 0.55f, 0.58f);

                                                                                             
    public static readonly Vector3 WaterColorCold = new(0.06f, 0.20f, 0.52f);

                                                                                 
                                                                              
                                                                                 
                                                                                   
                                                                                 
                                                                      
    public static Vector3 GetWaterColor(float winterFactor) => Vector3.Lerp(WaterColorWarm, WaterColorCold, winterFactor);


                 
                                                                                
                                                                              
                                                                            
                                                                          
                  
    public static bool IsSolid(BlockType type)
    {
        if (BlockRegistry.TryGetDefinition(type, out var def)) return def.IsSolid;
        if (type == BlockType.WildFlowers) return false;
        return type != BlockType.Air && type != BlockType.Water && type != BlockType.Portal
            && type != BlockType.DreamGate && type != BlockType.Torch && type != BlockType.Lantern;
    }


                 
                                                                            
                                                                             
                                                                          
                                  
                  
    public static bool IsOpaque(BlockType type)
    {
        if (BlockRegistry.TryGetDefinition(type, out var def)) return def.IsOpaque;
        if (type is BlockType.Leaves or BlockType.LeavesAutumnOrange or BlockType.LeavesAutumnYellow
            or BlockType.LeavesSnowy or BlockType.CherryLeaves or BlockType.WillowLeaves)
            return false;
        if (type == BlockType.WildFlowers) return false;
        return type != BlockType.Air && type != BlockType.Torch && type != BlockType.Lantern && type != BlockType.GlassPane;
    }

                 
                                                                           
                                                                             
                                                                             
                  
    public static bool BlocksLight(BlockType type, Vector3i direction)
    {
        if (type == BlockType.Snow)
            return direction == new Vector3i(0, -1, 0);
        return IsOpaque(type);
    }


                 
                                                                           
                                                                       
                                                                      
                                 
                  
    public static bool IsUnlit(BlockType type)
    {
        if (BlockRegistry.TryGetDefinition(type, out var def)) return def.IsUnlit;
        return type is BlockType.Portal or BlockType.NpcBody or BlockType.NpcHead
             or BlockType.Window or BlockType.LampLight or BlockType.DreamGate
             or BlockType.CrystalShard or BlockType.Torch or BlockType.Lantern or BlockType.GlassPane;
    }


                 
                                                                     
                                                                      
                                                                           
                                                                             
                                                                        
                                                                       
                                                                      
                                                                                
                                                                        
                                                                           
                                                                 
       
                                                                       
                                                                               
                  
    public static int GetLuminosity(BlockType type)
    {
        return type switch
        {
            BlockType.LampLight => 15,                                                            
            BlockType.Portal => 12,
            BlockType.DreamGate => 12,
            BlockType.CrystalShard => 15,                                            
            BlockType.Window => 10,
            BlockType.Torch => 14,
            BlockType.Lantern => 15,
            _ => BlockRegistry.TryGetDefinition(type, out var def) ? def.Luminosity : 0
        };
    }

    public static BlockShape GetDefaultShape(BlockType type) => type switch
    {
        BlockType.Torch => BlockShape.Torch,
        BlockType.Lantern => BlockShape.Lantern,
        BlockType.StoneSlab => BlockShape.HalfBottom,
            BlockType.Snow => BlockShape.SnowLayer,
        BlockType.WildFlowers => BlockShape.Cross,
        _ => BlockRegistry.TryGetDefinition(type, out var def) ? def.DefaultShape : BlockShape.Cube
    };


                 
                                                                               
                                                                             
                                                                              
                                                                             
                                                                              
                                                                               
                                                                      
                                                                          
                                                                            
              
       
                                                                                
                                                                             
                                                                         
                                                                             
                                                                     
                                                                      
                                                                            
                                                                    
                                                   
                  
    public static bool ShouldCullFace(BlockType type, BlockType neighbor) =>
        ShouldCullFace(type, neighbor, default);

                 
                                                                                
                                                                                 
                                                                           
                  
    public static bool ShouldCullFace(BlockType type, BlockType neighbor, Vector3i faceNormal)
    {
        if (type != BlockType.Water && neighbor == BlockType.Water) return false;
        if (neighbor == BlockType.Air) return false;
        if (neighbor == BlockType.Snow && type != BlockType.Snow)
            return faceNormal == new Vector3i(0, 1, 0);
        if (IsOpaque(neighbor)) return true;
        return neighbor == type;
    }

}

             
                                                                           
                                                                          
                                                                             
                                                                       
                   
   
                                                                         
                                                                            
                                                                             
                                                                             
                                                                         
                                   
   
                                                                         
                                                            
                                                                         
                                                                             
                                                                       
                                                                      
                                                
                                                                              
                                                                             
                                                                           
                                                                                
                                                                              
                                                              
                                                              
                                                                       
                                                                         
                             
                                                                                   
                                                                            
                                        
   
                                                                      
                                                     
                                                                              
                                                                      
                                                                       
                                                                             
                                                                      
                                                                    
                                   
                                                                      
                                                                     
              
public readonly struct BlockFaceTextures
{
                                                                                                         
    public readonly (int Col, int Row)? Side;
                                                                                          
    public readonly (int Col, int Row)? Top;
                                                                                          
    public readonly (int Col, int Row)? Bottom;

    public BlockFaceTextures((int Col, int Row)? side, (int Col, int Row)? top = null, (int Col, int Row)? bottom = null)
    {
        Side = side;
        Top = top;
        Bottom = bottom;
    }

                 
                                                                            
                                                                        
                                                                          
                                                                           
                                         
                  
    public static implicit operator BlockFaceTextures((int Col, int Row) side) => new(side);

                                                                                                                  
    public bool TryGetCellFor(Vector3 faceNormal, out (int Col, int Row) cell)
    {
        (int Col, int Row)? chosen =
            faceNormal.Y > 0.5f ? (Top ?? Side) :
            faceNormal.Y < -0.5f ? (Bottom ?? Side) :
            Side;
        if (chosen.HasValue) { cell = chosen.Value; return true; }
        cell = default;
        return false;
    }
}

public static class BlockTextures
{
                                                                                                                                              
    public const int CellPixelSize = 16;

                 
                                                                            
                                                                   
                                                                             
                                                                           
                                                                            
                                                                              
                                                                           
                                                                           
                                                                         
                  
    public static int AtlasGridSize { get; private set; } = 8;

    public static void SetGridSizeFromAtlasWidth(int atlasWidthPx)
    {
        AtlasGridSize = Math.Max(1, atlasWidthPx / CellPixelSize);
    }

                                                                                                                 
    private static readonly (int Col, int Row) Blank = (0, 0);

                 
                                                                        
                                                                        
                                                                            
                                                                         
                                                                       
                                                                           
                                                                        
                                                                           
                                                                                
                  
    private static readonly Dictionary<string, BlockFaceTextures> AutoCells = new(StringComparer.OrdinalIgnoreCase);

                                                                                                      
    public static void SetAutoCells(Dictionary<string, BlockFaceTextures> cells)
    {
        AutoCells.Clear();
        foreach (var kv in cells) AutoCells[kv.Key] = kv.Value;
    }

                                                                      
                                                                              
                                                                            
                                                                          
                                                                         
                                                                               
                                                                              
                                                                        
                                                                        
                                                                             
                                                                                             
    private static readonly Dictionary<BlockType, BlockFaceTextures> Cells = new()
    {
        { BlockType.Grass, new BlockFaceTextures(side: (2, 3), top: (1, 0), bottom: (2, 0)) },
        { BlockType.Dirt,  (2, 0) },
        { BlockType.Stone, (3, 0) },
        { BlockType.Wood,  new BlockFaceTextures(side: (4, 0), top: (3, 3), bottom: (3, 3)) },
        { BlockType.Sand,  (5, 0) },

                                                                                    
                                                                         
        { BlockType.Leaves, (0, 1) },
        { BlockType.Water, (1, 1) },
        { BlockType.GrassAutumn, (2, 1) },
        { BlockType.Snow, (3, 1) },
        { BlockType.LeavesAutumnOrange, (4, 1) },
        { BlockType.LeavesAutumnYellow, (5, 1) },
        { BlockType.LeavesSnowy, (6, 1) },
        { BlockType.Path, (7, 1) },

                
        { BlockType.Window, (0, 2) },
        { BlockType.LampLight, (1, 2) },
        { BlockType.DreamGate, (2, 2) },                                    

                                                                                
                                                                              
        { BlockType.MossStone, (3, 2) },
        { BlockType.WildFlowers, (4, 2) },
        { BlockType.CrystalShard, (5, 2) },
        { BlockType.Mud, (6, 2) },

                                                                          
                                                       
        { BlockType.Portal, (7, 2) },
        { BlockType.NpcBody, (0, 3) },
        { BlockType.NpcHead, (1, 3) },
        { BlockType.GlassPane, (2, 3) },
    };

                 
                                                                       
                                                                           
                                                                            
                                                                  
                                                                                       
                                                                             
                                                           
                                                                                
                                                                             
                                                                         
                                                                        
                                
                                                                             
                                                                        
                                 
                                           
                  
    public static (float UMin, float VMin, float UMax, float VMax) GetUvRect(BlockType type, Vector3 faceNormal)
    {
        (int Col, int Row) cell;
        string name = BlockRegistry.NameFor(type);
        if (AutoCells.TryGetValue(name, out var autoFaces) && autoFaces.TryGetCellFor(faceNormal, out var autoCell))
            cell = autoCell;
        else if (AtlasBindings.TryGet(name, out var bound))
            cell = bound;
        else if (Cells.TryGetValue(type, out var knownFaces) && knownFaces.TryGetCellFor(faceNormal, out var knownCell))
            cell = knownCell;
        else if (BlockRegistry.TryGetDefinition(type, out var def))
            cell = (def.AtlasCol, def.AtlasRow);
        else
            cell = Blank;

        float cellSize = 1f / AtlasGridSize;
        float uMin = cell.Col * cellSize;
        float vMin = cell.Row * cellSize;
        return (uMin, vMin, uMin + cellSize, vMin + cellSize);
    }

                 
                                                                             
                                                                         
                                                                      
                                                                        
                                                                          
                  
    public static (float UMin, float VMin, float UMax, float VMax) GetUvRect(BlockType type) =>
        GetUvRect(type, Vector3.UnitZ);                                                             

                 
                                                                          
                                                                       
                                                                              
                                                                       
                                                                             
                                                             
                  
    public static bool IsCellOccupied(int col, int row, out string? occupantName)
    {
        occupantName = null;
        if (col == Blank.Col && row == Blank.Row) return false;

        bool Matches(BlockFaceTextures faces) =>
            faces.Side == (col, row) || faces.Top == (col, row) || faces.Bottom == (col, row);

        foreach (var (name, faces) in AutoCells)
        {
            if (!Matches(faces)) continue;
            occupantName = name;
            return true;
        }
        if (AtlasBindings.TryGetOccupant(col, row, out var boundName))
        {
            occupantName = boundName;
            return true;
        }
        foreach (var (type, faces) in Cells)
        {
            if (!Matches(faces)) continue;
            occupantName = BlockRegistry.NameFor(type);
            return true;
        }
        foreach (var def in BlockRegistry.AllCustomBlocks)
        {
            if (def.AtlasCol != col || def.AtlasRow != row) continue;
            occupantName = def.Name;
            return true;
        }
        return false;
    }
}
