namespace RainCore;

             
                                                                            
                                                                     
                                                                     
                                                                             
                                                            
              
public enum ToolCategory
{
    None,
    Pickaxe,                                                                          
    Axe,                                                         
    Shovel                                            
}

             
                                                                                      
                                                                    
                                                                          
                                                                         
                                                       
   
                                                                         
                                                                         
                                                                           
                                                                                
              
public static class BlockDurability
{
    private static readonly Dictionary<BlockType, float> Hardness = new()
    {
                                                      
        { BlockType.Grass, 0.6f },
        { BlockType.GrassAutumn, 0.6f },
        { BlockType.Dirt, 0.5f },
        { BlockType.Sand, 0.5f },
        { BlockType.Snow, 0.3f },
        { BlockType.Mud, 0.6f },
        { BlockType.Path, 0.7f },
        { BlockType.WildFlowers, 0.15f },
        { BlockType.Torch, 0.2f },
        { BlockType.Lantern, 0.3f },
        { BlockType.Leaves, 0.25f },
        { BlockType.LeavesAutumnOrange, 0.25f },
        { BlockType.LeavesAutumnYellow, 0.25f },
        { BlockType.LeavesSnowy, 0.25f },
        { BlockType.OakLeaves, 0.25f },
        { BlockType.AspenLeaves, 0.25f },
        { BlockType.MapleLeaves, 0.25f },

                            
        { BlockType.Wood, 1.4f },
        { BlockType.StoneSlab, 1.8f },
        { BlockType.Sandstone, 1.2f },

                                                        
        { BlockType.Stone, 2.2f },
        { BlockType.MossStone, 2.4f },
        { BlockType.CrystalShard, 2.8f },

                                                                                 
                                                                           
                                                                 
        { BlockType.Portal, 0f },
        { BlockType.DreamGate, 0f },
        { BlockType.NpcBody, 0f },
        { BlockType.NpcHead, 0f },
        { BlockType.Window, 0f },
        { BlockType.LampLight, 0f },
        { BlockType.Water, 0f },
    };

                                                                                  
                                                                                         
    private const float DefaultHardness = 1.2f;

                                                                           
                                                                             
                                                                               
                             
    private const float ToolSpeedMultiplier = 3f;

    public static float GetHardness(BlockType type)
    {
        if (Hardness.TryGetValue(type, out var h)) return h;
        return DefaultHardness;
    }

                                                                                      
                                                                            
    public static ToolCategory GetPreferredTool(BlockType type) => type switch
    {
        BlockType.Stone or BlockType.StoneSlab or BlockType.Sandstone or BlockType.MossStone or BlockType.CrystalShard => ToolCategory.Pickaxe,
        BlockType.Wood or BlockType.AspenWood or BlockType.MapleWood => ToolCategory.Axe,
        BlockType.Grass or BlockType.GrassAutumn or BlockType.Dirt or BlockType.Sand
            or BlockType.Snow or BlockType.Mud or BlockType.Path => ToolCategory.Shovel,
                                                                                 
                                                                                
                                                                                    
        _ => BlockRegistry.TryGetDefinition(type, out var def)
            ? def.Name switch
            {
                "Brick" or "GlowCrystal" or "CoalOre" or "IronOre" or "CopperOre" or "GoldOre"
                    or "CopperBlock" or "IronBlock" or "GoldBlock" => ToolCategory.Pickaxe,
                "WoodPlank" or "BirchWood" => ToolCategory.Axe,
                _ => ToolCategory.None
            }
            : ToolCategory.None
    };

                                                                                         
                                                                                      
                                                           
    public static bool CanMine(BlockType type) => type != BlockType.Air && GetHardness(type) > 0f;

                                                                                    
                                                                          
                                        
    public static float GetMiningSeconds(BlockType type, ToolCategory heldTool)
    {
        float baseSeconds = GetHardness(type);
        if (baseSeconds <= 0f) return float.PositiveInfinity;              
        bool matchingTool = heldTool != ToolCategory.None && heldTool == GetPreferredTool(type);
        return matchingTool ? baseSeconds / ToolSpeedMultiplier : baseSeconds;
    }
}
