using OpenTK.Mathematics;

namespace RainCore;

                                                                 
                                                                           
                                                                            
                  

public partial class World
{

    public Vector3i FindGrassNear(int x, int z)
    {
        int maxRadius = (int)WorldRadius + 10;
        for (int radius = 0; radius < maxRadius; radius++)
        {
            for (int dx = -radius; dx <= radius; dx++)
            {
                for (int dz = -radius; dz <= radius; dz++)
                {
                    int gx = x + dx, gz = z + dz;
                    int top = GetSurfaceHeight(gx, gz);
                    if (IsWalkableGround(GetBlock(new Vector3i(gx, top, gz))))
                        return new Vector3i(gx, top, gz);
                }
            }
        }
        return new Vector3i(0, GetSurfaceHeight(0, 0), 0);
    }


    private static bool IsWalkableGround(BlockType t)
    {
        return t is BlockType.Grass or BlockType.GrassAutumn or BlockType.Snow;
    }


    public BlockType GetGroundSurfaceBlock()
    {
        return _theme switch
        {
            WorldTheme.Autumn => BlockType.GrassAutumn,
            WorldTheme.Winter => BlockType.Snow,
            WorldTheme.Summer => BlockType.Grass,
            _ => BlockType.Grass
        };
    }

                                                                                      
                                                                                
                                                                             
                                                                           
       
                                                                           
                                                                           
                                                                           
                                                                            
                                                                              
                                                                            
                                                       
    private BlockType GetGroundSurfaceBlockForBiome(Biome biome, Random rng, int topY)
    {
        switch (biome)
        {
            case Biome.Desert:
                return rng.NextDouble() < 0.04 ? BlockType.MossStone : BlockType.Sand;
            case Biome.BirchForest:
                return BlockType.Grass;
            case Biome.Jungle:
                return BlockType.Grass;
            case Biome.Savanna:
                return rng.NextDouble() < 0.08 ? BlockType.Sand : BlockType.Grass;
            case Biome.DarkForest:
                return rng.NextDouble() < 0.08 ? BlockType.MossStone : BlockType.Grass;
            case Biome.Mountains:
                                                                       
                                                                                 
                if (topY >= SeaLevel + MountainSnowLevel)
                {
                    if (rng.NextDouble() < 0.03) return BlockType.CrystalShard;                         
                    return BlockType.Snow;
                }
                                                                               
                                                                                   
                                                                                    
                                                                           
                                                                    
                if (rng.NextDouble() < 0.02) return BlockType.CrystalShard;
                if (rng.NextDouble() < 0.22) return BlockType.MossStone;
                return BlockType.Stone;
            case Biome.Winter:
            case Biome.TundraForest:
                                                                                  
                                                                                 
                                                                                    
                                                                                    
                                                                      
                if (rng.NextDouble() < 0.02) return BlockType.CrystalShard;
                if (rng.NextDouble() < 0.05) return BlockType.MossStone;
                return BlockType.Snow;
            case Biome.AutumnForest:
                if (rng.NextDouble() < 0.04) return BlockType.MossStone;
                if (rng.NextDouble() < 0.025 && BlockRegistry.TryGetByName("BerryBush", out var autumnBerryBush)) return autumnBerryBush;
                return BlockType.GrassAutumn;
            case Biome.AutumnMeadow:
                return BlockType.Grass;
            case Biome.Swamp:
                if (rng.NextDouble() < 0.05) return BlockType.MossStone;
                if (rng.NextDouble() < 0.04 && BlockRegistry.TryGetByName("DeadBush", out var deadBush)) return deadBush;
                return BlockType.Mud;
            case Biome.SummerMeadow:
                return BlockType.Grass;
            case Biome.FlowerMeadow:
                if (rng.NextDouble() < 0.009 && BlockRegistry.TryGetByName("TallGrass", out var tallGrass)) return tallGrass;
                if (rng.NextDouble() < 0.006 && BlockRegistry.TryGetByName("BlueFlower", out var blueFlower)) return blueFlower;
                if (rng.NextDouble() < 0.009) return BlockType.WildFlowers;
                if (rng.NextDouble() < 0.006 && BlockRegistry.TryGetByName("RedTulip", out var tulip)) return tulip;
                if (rng.NextDouble() < 0.002 && BlockRegistry.TryGetByName("BerryBush", out var summerBerryBush)) return summerBerryBush;
                return BlockType.Grass;
            default:
                return BlockType.Grass;
        }
    }

                                                                              
                                                                             
                                                                        
                                                                          
                                                                         
                                                                          
                                                                             
    private static BlockType GetSoilBlockForBiome(Biome biome)
    {
                                                                               
                                                                              
                                                                           
                                             
        if (biome == Biome.Mountains) return BlockType.Stone;

        string name = biome switch
        {
            Biome.Winter or Biome.TundraForest => "Permafrost",
            Biome.Swamp => "PeatSoil",
            Biome.AutumnForest or Biome.AutumnMeadow => "ForestLoam",
            Biome.SummerMeadow or Biome.BirchForest or Biome.Savanna or Biome.DarkForest => "MeadowSoil",
            Biome.Desert => "MeadowSoil",
            _ => "MeadowSoil"
        };
        return BlockRegistry.TryGetByName(name, out var soil) ? soil : BlockType.Dirt;
    }

                                                                                  
                                                                 
                                                                             
                                                                              
                                                                         
                                                                           
                                                                         
                                                                         
                                                                            
                                                                        
                                                                           
                                                                              
                                                                        
                                                                               
                                                                           
                                                                           
                                                                       
    private static BlockType GetStoneOrOreBlock(float t01, Random rng)
    {
        double coalChance = 0.015 + t01 * 0.015;                                                
        double ironChance = 0.006 + t01 * 0.02;                                                 
        double copperChance = 0.014 - t01 * 0.01;                                                          
        double goldChance = 0.003 + t01 * 0.012;                                                        
        const double mossChance = 0.01;                                                    

        BlockType result = BlockType.Stone;
        if (rng.NextDouble() < coalChance && BlockRegistry.TryGetByName("CoalOre", out var coal)) result = coal;
        if (rng.NextDouble() < ironChance && BlockRegistry.TryGetByName("IronOre", out var iron)) result = iron;
        if (rng.NextDouble() < copperChance && BlockRegistry.TryGetByName("CopperOre", out var copper)) result = copper;
        if (rng.NextDouble() < goldChance && BlockRegistry.TryGetByName("GoldOre", out var gold)) result = gold;
        if (result == BlockType.Stone && rng.NextDouble() < mossChance) result = BlockType.MossStone;
        return result;
    }


                                                                              
                                                                                
                                                                            
                                                                              
                                                                              
                                                                                              
    private BlockType GetLeavesBlockForBiome(Biome biome, Random rng)
    {
        return biome switch
        {
            Biome.Winter or Biome.TundraForest => BlockType.LeavesSnowy,
            Biome.AutumnForest or Biome.AutumnMeadow or Biome.BirchForest or Biome.AspenForest or Biome.MapleForest =>
                rng.NextDouble() < 0.5 ? BlockType.LeavesAutumnOrange : BlockType.LeavesAutumnYellow,
            Biome.OakForest => BlockRegistry.TryGetByName("OakLeaves", out var oakLeaves) ? oakLeaves : BlockType.Leaves,
            Biome.WillowForest => BlockType.WillowLeaves,
            Biome.CherryForest => BlockType.CherryLeaves,
            Biome.Jungle => BlockType.Leaves,
            Biome.DarkForest => BlockType.LeavesDark,
            Biome.Savanna => BlockType.Leaves,
            _ => BlockType.Leaves
        };
    }

                                                                                
                                                                          
                                                                        
                                                                            
                                                                            
                                                                       
                                                                                      
    private enum TreeStyle { Round, Pine, Birch, Cherry, Willow, Oak, Aspen, Maple, Tropical }

    private static TreeStyle ChooseTreeStyle(Biome biome, Random rng)
    {
        return biome switch
        {
            Biome.Winter or Biome.TundraForest => rng.NextDouble() < 0.6 ? TreeStyle.Pine : TreeStyle.Round,
            Biome.BirchForest => TreeStyle.Birch,
            Biome.CherryForest => TreeStyle.Cherry,
            Biome.Jungle => TreeStyle.Tropical,
            Biome.Swamp => TreeStyle.Willow,
            Biome.OakForest or Biome.DarkForest => TreeStyle.Oak,
            Biome.WillowForest => TreeStyle.Willow,
            Biome.AspenForest => TreeStyle.Aspen,
            Biome.MapleForest => TreeStyle.Maple,
                                                                       
                                                                                  
                                                                                
            Biome.Mountains => TreeStyle.Pine,
            Biome.SummerMeadow or Biome.FlowerMeadow or Biome.AutumnMeadow => TreeStyle.Oak,
            _ => TreeStyle.Round
        };
    }

                                                                                     
                                                                                 
                                                                             
                                                                               
                                                
    private void PlaceTree(Vector3i baseTrunk, Biome biome, Random rng)
    {
        switch (ChooseTreeStyle(biome, rng))
        {
            case TreeStyle.Pine: PlacePineTree(baseTrunk, rng); break;
            case TreeStyle.Birch: PlaceBirchTree(baseTrunk, biome, rng); break;
            case TreeStyle.Cherry: PlaceCherryTree(baseTrunk, rng); break;
            case TreeStyle.Willow: PlaceWillowTree(baseTrunk, rng); break;
            case TreeStyle.Oak: PlaceVariantTree(baseTrunk, BlockType.Wood, "OakLeaves", rng, 5, 3); break;
            case TreeStyle.Aspen: PlaceVariantTree(baseTrunk, BlockType.AspenWood, "AspenLeaves", rng, 4, 2); break;
            case TreeStyle.Maple: PlaceVariantTree(baseTrunk, BlockType.MapleWood, "MapleLeaves", rng, 4, 2); break;
            case TreeStyle.Tropical: PlaceTropicalTree(baseTrunk, rng); break;
            default: PlaceRoundTree(baseTrunk, biome, rng); break;
        }
    }

    private void PlaceTropicalTree(Vector3i baseTrunk, Random rng)
    {
        int trunkHeight = 7 + rng.Next(3);
        for (int y = 0; y < trunkHeight; y++)
            SetBlockSilent(baseTrunk + new Vector3i(0, y, 0), BlockType.Wood);

        int crownY = trunkHeight;
        for (int y = -1; y <= 2; y++)
        {
            int radius = y <= 0 ? 3 : y == 1 ? 2 : 1;
            for (int x = -radius; x <= radius; x++)
                for (int z = -radius; z <= radius; z++)
                {
                    if (x * x + z * z > radius * radius + 1) continue;
                    var pos = baseTrunk + new Vector3i(x, crownY + y, z);
                    if (GetBlock(pos) == BlockType.Air) SetBlockSilent(pos, BlockType.Leaves);
                }
        }
    }

    private void PlaceVariantTree(Vector3i baseTrunk, BlockType trunk, string leavesName, Random rng, int trunkHeight, int radius)
    {
        if (!BlockRegistry.TryGetByName(leavesName, out var leaves)) leaves = BlockType.Leaves;
        for (int y = 0; y < trunkHeight; y++)
            SetBlockSilent(baseTrunk + new Vector3i(0, y, 0), trunk);
        var center = baseTrunk + new Vector3i(0, trunkHeight, 0);
        for (int y = -1; y <= 1; y++)
            for (int x = -radius; x <= radius; x++)
                for (int z = -radius; z <= radius; z++)
                {
                    if (x * x + z * z > radius * radius + 1) continue;
                    var pos = center + new Vector3i(x, y, z);
                    if (GetBlock(pos) == BlockType.Air) SetBlockSilent(pos, leaves);
                }
    }

    private void PlaceCherryTree(Vector3i baseTrunk, Random rng)
    {
        int trunkHeight = 4 + rng.Next(3);
        for (int y = 0; y < trunkHeight; y++) SetBlockSilent(baseTrunk + new Vector3i(0, y, 0), BlockType.CherryWood);
        int radius = 2 + rng.Next(2);
        for (int y = -1; y <= 2; y++)
            for (int x = -radius; x <= radius; x++)
                for (int z = -radius; z <= radius; z++)
                {
                    if (x * x + z * z + y * y > radius * radius + 2) continue;
                    var pos = baseTrunk + new Vector3i(x, trunkHeight + y, z);
                    if (GetBlock(pos) == BlockType.Air) SetBlockSilent(pos, BlockType.CherryLeaves);
                }
    }

    private void PlaceWillowTree(Vector3i baseTrunk, Random rng)
    {
        int trunkHeight = 4 + rng.Next(2);
        for (int y = 0; y < trunkHeight; y++) SetBlockSilent(baseTrunk + new Vector3i(0, y, 0), BlockType.Wood);
        for (int x = -2; x <= 2; x++)
            for (int z = -2; z <= 2; z++)
                for (int y = 0; y <= 2; y++)
                {
                    if (Math.Abs(x) + Math.Abs(z) > 3) continue;
                    var crown = baseTrunk + new Vector3i(x, trunkHeight - y, z);
                    if (GetBlock(crown) == BlockType.Air) SetBlockSilent(crown, BlockType.WillowLeaves);
                }

        for (int x = -2; x <= 2; x++)
            for (int z = -2; z <= 2; z++)
            {
                if (Math.Abs(x) + Math.Abs(z) > 3 || (x == 0 && z == 0)) continue;
                int drop = 1 + Math.Abs(x + z) % 3;
                for (int y = 1; y <= drop; y++)
                {
                    var hanging = baseTrunk + new Vector3i(x, trunkHeight - 1 - y, z);
                    if (GetBlock(hanging) == BlockType.Air) SetBlockSilent(hanging, BlockType.WillowLeaves);
                }
            }
    }

                                                                            
                                                                          
                                                                              
                                                                               
    private void PlaceRoundTree(Vector3i baseTrunk, Biome biome, Random rng)
    {
        int trunkHeight = 3 + rng.Next(2);
        for (int i = 0; i < trunkHeight; i++)
            SetBlockSilent(baseTrunk + new Vector3i(0, i, 0), BlockType.Wood);

        var leavesCenter = baseTrunk + new Vector3i(0, trunkHeight, 0);
        for (int ly = -1; ly <= 1; ly++)
        {
            int radius = ly == 1 ? 1 : 2;
            for (int lx = -radius; lx <= radius; lx++)
            {
                for (int lz = -radius; lz <= radius; lz++)
                {
                    if (lx == 0 && lz == 0 && ly < 1) continue;
                    if (MathF.Sqrt(lx * lx + lz * lz) > radius + 0.4f) continue;
                    var pos = leavesCenter + new Vector3i(lx, ly, lz);
                    if (GetBlock(pos) == BlockType.Air)
                        SetBlockSilent(pos, GetLeavesBlockForBiome(biome, rng));
                }
            }
        }
    }

                                                                                 
                                                                               
                                                                             
                                                                             
                                                                                     
    private void PlacePineTree(Vector3i baseTrunk, Random rng)
    {
        int trunkHeight = 5 + rng.Next(3);
        for (int i = 0; i < trunkHeight; i++)
            SetBlockSilent(baseTrunk + new Vector3i(0, i, 0), BlockType.Wood);

        if (!BlockRegistry.TryGetByName("PineNeedles", out var needles))
            needles = BlockType.Leaves;                                               

        int crownHeight = 4 + rng.Next(2);
        int crownBaseY = trunkHeight - crownHeight + 1;                                             
        for (int layer = 0; layer < crownHeight; layer++)
        {
            int y = crownBaseY + layer;
                                                                            
                                                                         
                                           
            int radius = (crownHeight - 1 - layer) / 2;
            if (layer == crownHeight - 1) radius = 0;

            for (int lx = -radius; lx <= radius; lx++)
            {
                for (int lz = -radius; lz <= radius; lz++)
                {
                    if (MathF.Sqrt(lx * lx + lz * lz) > radius + 0.4f) continue;
                    var pos = baseTrunk + new Vector3i(lx, y, lz);
                    if (GetBlock(pos) == BlockType.Air)
                        SetBlockSilent(pos, needles);
                }
            }
        }
    }

                                                                             
                                                                              
                                                                        
                                                                              
                                                                           
                                                                      
                                          
    private void PlaceBirchTree(Vector3i baseTrunk, Biome biome, Random rng)
    {
        if (!BlockRegistry.TryGetByName("BirchWood", out var trunkBlock))
            trunkBlock = BlockType.Wood;                                               

        int trunkHeight = 4 + rng.Next(2);
        for (int i = 0; i < trunkHeight; i++)
            SetBlockSilent(baseTrunk + new Vector3i(0, i, 0), trunkBlock);

        var leavesCenter = baseTrunk + new Vector3i(0, trunkHeight, 0);
        for (int ly = -1; ly <= 1; ly++)
        {
            int radius = ly == 1 ? 1 : 2;
            for (int lx = -radius; lx <= radius; lx++)
            {
                for (int lz = -radius; lz <= radius; lz++)
                {
                    if (lx == 0 && lz == 0 && ly < 1) continue;
                    if (MathF.Sqrt(lx * lx + lz * lz) > radius + 0.4f) continue;
                    if (rng.NextDouble() < 0.25) continue;                                                
                    var pos = leavesCenter + new Vector3i(lx, ly, lz);
                    if (GetBlock(pos) == BlockType.Air)
                        SetBlockSilent(pos, GetLeavesBlockForBiome(biome, rng));
                }
            }
        }
    }


    public bool Raycast(Vector3 origin, Vector3 direction, float maxDistance, out Vector3i hitBlock, out Vector3i placeBlock)
        => Raycast(origin, direction, maxDistance, out hitBlock, out placeBlock, out _);

                 
                                                                                   
                                                                                  
                                                                                    
                                                                                  
                                                                                 
                                                                              
                                                                              
                  
    public bool Raycast(Vector3 origin, Vector3 direction, float maxDistance, out Vector3i hitBlock, out Vector3i placeBlock, out Vector3 exactHitPoint)
    {
        hitBlock = default;
        placeBlock = default;
        exactHitPoint = default;

        direction = Vector3.Normalize(direction);
        const float step = 0.02f;
        var lastEmpty = new Vector3i(
            (int)MathF.Floor(origin.X),
            (int)MathF.Floor(origin.Y),
            (int)MathF.Floor(origin.Z));

        for (float t = 0; t < maxDistance; t += step)
        {
            var p = origin + direction * t;
            var block = new Vector3i((int)MathF.Floor(p.X), (int)MathF.Floor(p.Y), (int)MathF.Floor(p.Z));

            if (GetBlock(block) != BlockType.Air)
            {
                hitBlock = block;
                placeBlock = lastEmpty;
                exactHitPoint = p;
                return true;
            }
            lastEmpty = block;
        }
        return false;
    }
}
