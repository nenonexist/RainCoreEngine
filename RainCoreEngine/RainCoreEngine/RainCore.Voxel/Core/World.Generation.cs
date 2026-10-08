using OpenTK.Mathematics;

namespace RainCore;

             
                                                                     
                                                                        
                                                                              
                                                                          
   
                                                                              
                                                                           
                                                                              
                                                                         
                                                                     
                                                                         
                                                                  
                                                                         
                                                      
              
public partial class World
{
    private void GenerateTerrain()
    {
        if (_kind is WorldKind.FlatCity or WorldKind.RoguelikeSnow or WorldKind.RoguelikeDungeon)
        {
            GenerateFlatCityPlatform();
            return;
        }

                                                                                  
                                                                                   
                                                                                  
        if (_kind == WorldKind.Interior)
            return;

        (_noiseOffsetX, _noiseOffsetZ) = NoiseOffsetsForSeed(_seed);

                                                                                    
                                                                                   
                                                                                 
                                                                                  
                                                                                       
        PrecomputeLandmassSpecs();
    }

                                                                                           
                                                                               
                                                                                 
                                                                           
                                                                            
                                                                                    
                                                                                     
    private void GenerateIslandChunk(Vector2i coord, Random rng)
    {
        int baseX = coord.X * ChunkSize;
        int baseZ = coord.Y * ChunkSize;

                                                                                    
                                                                                           
                                           
        _heightRawCache.Value!.Clear();

                                                                                   
                                                                                    
                                                                                    
                                                                                 
                                                                                    
                                                                
        var chunkTreeSpots = new List<Vector2i>();

        for (int lx = 0; lx < ChunkSize; lx++)
        {
            for (int lz = 0; lz < ChunkSize; lz++)
            {
                int x = baseX + lx;
                int z = baseZ + lz;

                int topY = ComputeHeightSmoothed(x, z, _noiseOffsetX, _noiseOffsetZ);
                var biome = ResolveBiome(x, z, topY);
                FillColumn(x, z, topY, biome, rng, chunkTreeSpots);
            }
        }

        ApplyLandmassSpecsToChunk(coord);
    }

                                                                               
                                                                                      
                                                                                  
                                                                               
                                                                               
                                                                          
                                                                                        
                                                                                       
                                                                                     
                                                                                      
                                                     
    private const int TreeMinSpacingSq = 6 * 6;

    private void FillColumn(int x, int z, int topY, Biome biome, Random rng, List<Vector2i> chunkTreeSpots)
    {
                                                                         
                                                                             
                                                                                 
                                                                        
                                                                        
                                                                     
                                                                       
                                                                            
                                                                   
        bool isBeach = topY <= SeaLevel;
        var selectedSurface = isBeach ? BlockType.Sand : GetGroundSurfaceBlockForBiome(biome, rng, topY);
        if (!isBeach && IsForestBiome(biome))
        {
            double forestDecorationRoll = rng.NextDouble();
            if (forestDecorationRoll < 0.004 && BlockRegistry.TryGetByName("TallGrass", out var forestGrass))
                selectedSurface = forestGrass;
            else if (forestDecorationRoll < 0.006 && BlockRegistry.TryGetByName("BlueFlower", out var forestFlower))
                selectedSurface = forestFlower;
            else if (forestDecorationRoll < 0.008)
                selectedSurface = BlockType.WildFlowers;
        }
        if (!isBeach && (biome == Biome.SummerMeadow || biome == Biome.FlowerMeadow) && IsFlowerMeadow(x, z)
            && rng.NextDouble() < (biome == Biome.FlowerMeadow ? 0.42 : 0.20))
        {
            if (rng.NextDouble() < 0.5 && BlockRegistry.TryGetByName("BlueFlower", out var meadowFlower))
                selectedSurface = meadowFlower;
            else
                selectedSurface = BlockType.WildFlowers;
        }
        bool isSurfaceDecoration = BlockRegistry.TryGetDefinition(selectedSurface, out var decorationDef)
            ? decorationDef.DefaultShape == BlockShape.Cross
            : selectedSurface == BlockType.WildFlowers;
        var surface = isSurfaceDecoration ? GetBaseSurfaceForDecoration(biome, isBeach) : selectedSurface;

        var surfacePos = new Vector3i(x, topY, z);
        SetBlockSilent(surfacePos, surface);
        if (surface == BlockType.Snow && biome != Biome.Mountains)
            SetBlockShape(surfacePos, BlockShape.SnowLayer);

                                                                               
                                                                                  
                                                                        
                                                                                     
                                                                               
                                                                                 
                                                                             
                                                                          
                                                                                  
                                                     
                                                              
                                                                                
                                                                      
        const int dirtDepth = 3;
        int dirtBottom = topY - dirtDepth;
        var soil = isBeach ? BlockType.Sand : GetSoilBlockForBiome(biome);
        for (int y = topY - 1; y >= dirtBottom; y--)
            SetBlockSilent(new Vector3i(x, y, z), soil);

                                                                               
                                                                                
                                                                                 
                                                                                  
                                                                      
        int stoneTop = dirtBottom - 1;
        float stoneSpan = MathF.Max(1f, stoneTop - BedrockLevel);
        for (int y = stoneTop; y >= BedrockLevel; y--)
        {
            float t01 = Math.Clamp((stoneTop - y) / stoneSpan, 0f, 1f);
            SetBlockSilent(new Vector3i(x, y, z), GetStoneOrOreBlock(t01, rng));
        }

                                                                                     
                                                                                     
                                                                                      
                                                                                    
                                                       
                                                                          
                                                                     
        CarveColumnOverhangs(x, z, topY, dirtBottom);

                                                                              
                                                                                    
                                                                         
                                                                                 
                                                                              
                                                                               
                                                  
        if (topY < SeaLevel)
            FillSeaColumn(x, z, topY);

        if (isSurfaceDecoration && topY >= SeaLevel)
        {
            var decorationPos = new Vector3i(x, topY + 1, z);
            SetBlockSilent(decorationPos, selectedSurface);
            SetBlockShape(decorationPos, BlockData.GetDefaultShape(selectedSurface));
        }

        bool mountainStreamSource = biome == Biome.Mountains && IsMountainStreamSource(x, z, topY, rng);
        if (mountainStreamSource)
        {
            SetBlockSilent(surfacePos, BlockType.Water);
            MarkLiquidDirty(surfacePos);
        }

                                                                                   
                                                                                    
                                                                                     
                                                                                      
                                                                                        
        bool aboveTreeline = biome == Biome.Mountains && topY >= SeaLevel + MountainSnowLevel;
        bool supportedSurface = BlockData.IsSolid(GetBlock(new Vector3i(x, topY - 1, z)));
        bool surfaceIsWater = GetBlock(surfacePos) == BlockType.Water;
        bool willowNearWater = biome == Biome.WillowForest && IsNearWaterAt(x, z);
        bool cherryOnHills = biome != Biome.CherryForest || topY >= SeaLevel + 8;
        bool treeAllowed = (biome != Biome.WillowForest || willowNearWater) && cherryOnHills;
        if (!isBeach && !surfaceIsWater && supportedSurface && !aboveTreeline && treeAllowed && rng.NextDouble() < TreeChanceForBiome(biome)
            && HasTreeSpacing(x, z, chunkTreeSpots))
        {
            chunkTreeSpots.Add(new Vector2i(x, z));
            PlaceTree(new Vector3i(x, topY + 1, z), biome, rng);
        }
    }

    private bool IsMountainStreamSource(int x, int z, int topY, Random rng)
    {
        if (topY < SeaLevel + MountainRockLevel + 8) return false;

        int east = ComputeHeightSmoothed(x + 1, z, _noiseOffsetX, _noiseOffsetZ);
        int west = ComputeHeightSmoothed(x - 1, z, _noiseOffsetX, _noiseOffsetZ);
        int south = ComputeHeightSmoothed(x, z + 1, _noiseOffsetX, _noiseOffsetZ);
        int north = ComputeHeightSmoothed(x, z - 1, _noiseOffsetX, _noiseOffsetZ);
        int downhill = Math.Max(Math.Abs(topY - east), Math.Max(Math.Abs(topY - west), Math.Max(Math.Abs(topY - south), Math.Abs(topY - north))));
        if (downhill < 3) return false;

        float streamNoise = Fbm(
            (x + _noiseOffsetX + 2400f) * 0.035f,
            (z + _noiseOffsetZ + 2400f) * 0.035f);
        return streamNoise > 0.62f && rng.NextDouble() < 0.20;
    }

    private static BlockType GetBaseSurfaceForDecoration(Biome biome, bool isBeach)
    {
        if (isBeach) return BlockType.Sand;
        return biome switch
        {
            Biome.Winter or Biome.TundraForest or Biome.Mountains => BlockType.Snow,
            Biome.Swamp => BlockType.Mud,
            Biome.Desert or Biome.Savanna => BlockType.Sand,
            Biome.AutumnForest => BlockType.GrassAutumn,
            _ => BlockType.Grass
        };
    }

    private static bool IsForestBiome(Biome biome) => biome is
        Biome.AutumnForest or Biome.BirchForest or Biome.OakForest or
        Biome.WillowForest or Biome.CherryForest or Biome.AspenForest or
        Biome.MapleForest or Biome.DarkForest or Biome.Jungle;

    private bool IsFlowerMeadow(int x, int z)
    {
        float wave = MathF.Sin((x + _noiseOffsetX) * 0.008f) + MathF.Sin((z + _noiseOffsetZ) * 0.008f);
        return wave > 1.25f;
    }

                                                                                    
                                                                                      
                                                                                      
                                                                                      
                                                                                     
                                                         
    private static bool HasTreeSpacing(int x, int z, List<Vector2i> chunkTreeSpots)
    {
        foreach (var spot in chunkTreeSpots)
        {
            int dx = spot.X - x, dz = spot.Y - z;
            if (dx * dx + dz * dz < TreeMinSpacingSq) return false;
        }
        return true;
    }

                                                                               
                                                                             
    private static double TreeChanceForBiome(Biome biome)
    {
                                                                                   
                                                                                       
                                                                                           
                                                                                         
        return biome switch
        {
            Biome.AutumnForest => 0.012,
            Biome.TundraForest => 0.012,
            Biome.Swamp => 0.012,
            Biome.AutumnMeadow => 0.003,
            Biome.Winter => 0.0015,
            Biome.SummerMeadow => 0.002,
            Biome.BirchForest => 0.016,
            Biome.OakForest => 0.018,
            Biome.WillowForest => 0.028,
            Biome.CherryForest => 0.014,
            Biome.Jungle => 0.02,
            Biome.AspenForest => 0.032,
            Biome.MapleForest => 0.032,
            Biome.FlowerMeadow => 0.008,
            Biome.DarkForest => 0.022,
            Biome.Savanna => 0.012,
                                                                                  
                                                                                   
                                                                                  
                                  
            Biome.Mountains => 0.01,
            _ => 0.006
        };
    }

                                                                                    
                                                                               
                                                                             
                                                                                
                               
    private void FillSeaColumn(int x, int z, int topY)
    {
        for (int y = topY + 1; y <= SeaLevel; y++)
        {
            var pos = new Vector3i(x, y, z);
            if (GetBlock(pos) != BlockType.Air) continue;
            SetBlockSilent(pos, BlockType.Water);
        }
    }

                                                                          
                                                                                  
                                                           
       
                                                                                
                                                                             
                                                                                    
                                                                               
                                                                                  
                                                                                     
    private void CarveColumnOverhangs(int x, int z, int topY, int dirtBottom)
    {
        unchecked
        {
            uint hash = (uint)(x * 374761393 + z * 668265263 + _seed * 1442695041);
            hash ^= hash >> 16;
            if (hash % 3 != 0) return;
        }

                                                                                  
                                                                                
                                                                                    
                                                       
        int neighborhoodSurface = topY;
        for (int dx = -2; dx <= 2; dx++)
            for (int dz = -2; dz <= 2; dz++)
            {
                if (dx == 0 && dz == 0) continue;
                int sampleX = x + dx;
                int sampleZ = z + dz;
                neighborhoodSurface = Math.Min(neighborhoodSurface,
                    ComputeHeightSmoothed(sampleX, sampleZ, _noiseOffsetX, _noiseOffsetZ));
            }

                                                                               
                                                                             
                                                        
        int caveTop = Math.Min(neighborhoodSurface - 14, dirtBottom - 6);
        int caveBottom = BedrockLevel + 3;
        if (caveTop <= caveBottom) return;                                                                          

        float span = MathF.Max(1, caveTop - caveBottom);

        for (int y = caveTop; y > caveBottom; y--)
        {
                                                                                       
                                                                                     
                                                                                   
                                                                                  
            float depth01 = (caveTop - y) / span;

                                                                                 
                                                                                   
                                                                     
            float cheeseNoise = Fbm3D(
                (x + _noiseOffsetX) * 0.09f,
                y * 0.09f,
                (z + _noiseOffsetZ) * 0.09f);
            float cheeseThreshold = Lerp(0.76f, 0.62f, depth01);
            if (cheeseNoise > cheeseThreshold)
            {
                SetBlockSilent(new Vector3i(x, y, z), BlockType.Air);
                continue;
            }

                                                                             
                                                                              
                                                                                 
            float tunnelNoise = Fbm3D(
                (x + _noiseOffsetX + 1200f) * 0.05f,
                y * 0.08f,
                (z + _noiseOffsetZ + 1200f) * 0.05f);

            float tunnelWidth = Lerp(0.035f, 0.06f, depth01);
            if (MathF.Abs(tunnelNoise - 0.5f) < tunnelWidth)
                SetBlockSilent(new Vector3i(x, y, z), BlockType.Air);
        }
    }



                 
                                                                             
                                                                                 
                                                                      
                                                                                   
                                                                             
                                                                          
                  
    private void GenerateFlatCityPlatform()
    {
        int r = (int)MathF.Ceiling(_flatAreaRadius);
        int chunkRadius = r / ChunkSize + 1;

        for (int cx = -chunkRadius; cx <= chunkRadius; cx++)
        {
            for (int cz = -chunkRadius; cz <= chunkRadius; cz++)
            {
                int baseX = cx * ChunkSize;
                int baseZ = cz * ChunkSize;

                for (int lx = 0; lx < ChunkSize; lx++)
                {
                    for (int lz = 0; lz < ChunkSize; lz++)
                    {
                        int x = baseX + lx;
                        int z = baseZ + lz;

                        if (!IsWithinFlatPlatform(x, z)) continue;

                        FillCityColumn(x, z);
                    }
                }
            }
        }
    }
                 
                                                                               
                                                                              
                                                                                
                                                                          
                  
    private bool IsWithinFlatPlatform(int x, int z)
    {
        return MathF.Max(MathF.Abs(x), MathF.Abs(z)) <= _flatAreaRadius;
    }

                 
                                                                        
                                                                             
                                                                             
                                                                       
                                     
                  
    private bool IsImplicitStoneColumn(Vector3i pos)
    {
        const int dirtDepth = 3;                                     
        int implicitTop = CityGroundLevel - dirtDepth - 1;
        if (pos.Y < BedrockLevel || pos.Y > implicitTop) return false;
        return IsWithinFlatPlatform(pos.X, pos.Z);
    }
                                                                                  
                                                                               
                                                                              
                                                                       
                                                                             
                                                                         
                                                                                    
    private void FillCityColumn(int x, int z)
    {
        var surface = GetGroundSurfaceBlock();
        SetBlockSilent(new Vector3i(x, CityGroundLevel, z), surface);

        for (int y = CityGroundLevel - 1; y >= CityGroundLevel - 3; y--)
            SetBlockSilent(new Vector3i(x, y, z), BlockType.Dirt);
        for (int y = CityGroundLevel - 4; y >= BedrockLevel; y--)
            SetBlockSilent(new Vector3i(x, y, z), BlockType.Stone);
    }

                                                                                   
                                                                                
                                                                                      
                                                                           
                                                                                        
                                                                                        
                                                                                          
                                                                                        
                                                                                           
                                                                                         
                                                                                        
                                                                                        
                                                                                        
                                                                                      
                                                                                        
                                                                                              
    private readonly ThreadLocal<Dictionary<Vector2i, float>> _heightRawCache =
        new(() => new Dictionary<Vector2i, float>());

    private float ComputeHeightRaw(int x, int z, float dist, float noiseOffsetX, float noiseOffsetZ)
    {
        var cacheKey = new Vector2i(x, z);
        var heightCache = _heightRawCache.Value!;
        if (heightCache.TryGetValue(cacheKey, out var cachedHeight)) return cachedHeight;

        float nx = x + noiseOffsetX;
        float nz = z + noiseOffsetZ;

        if (dist > 250_000f)
        {
            float farlands = Math.Clamp((dist - 250_000f) / 750_000f, 0f, 1f);
            nx += MathF.Sin(nz * 0.0007f) * farlands * 900f;
            nz += MathF.Sin(nx * 0.0007f) * farlands * 900f;
        }

                                                                                 
                                                                                       
                                                                                    
                                                                                     
                                                                                   
                                                                            
                                     
        var (wx, wz) = DomainWarp(nx, nz);

                                                                                
                                                                                 
                                                                                
                                                                                    
                                                                               
                                                                            
        float continentalness = Fbm(nx * 0.006f + 1300f, nz * 0.006f + 1300f);
        float baseHeight = ContinentalnessToHeight(continentalness);

                                                                                    
                                                                                  
                                                                                
        float erosion = Fbm(nx * 0.011f + 1700f, nz * 0.011f + 1700f);

                                                                                
                                                                           
                                                                            
                                                                                  
        float regionNoise = Fbm(nx * 0.012f + 300f, nz * 0.012f + 300f);
        float hillRuggedness = SmoothStep(0.3f, 0.85f, regionNoise);
        hillRuggedness = MathF.Pow(hillRuggedness, 1.8f);

        float detailFreq = Lerp(0.025f, 0.11f, hillRuggedness);
        float detailAmp = Lerp(0.3f, 9f, hillRuggedness);
        float detail = FbmPerlin(wx * detailFreq, wz * detailFreq);

        float height = baseHeight + detail * detailAmp;

        if (hillRuggedness > 0.5f)
        {
            float ridgeNoise = FbmPerlin(wx * 0.18f + 700f, wz * 0.18f + 700f);
            float ridge = MathF.Pow(1f - MathF.Abs(ridgeNoise), 2f);
            height += ridge * (hillRuggedness - 0.5f) * 2f * 10f;
        }

                                                                                 
                                                                                   
                                                                                   
                                                                                   
                                                                               
                                                                                  
                                                                 
        float mountainMask = SmoothStep(0.62f, 0.86f, continentalness) * (1f - SmoothStep(0.2f, 0.55f, erosion));
        if (mountainMask > 0.01f)
        {
                                                                                   
                                                                             
                                                                                  
                                                                                 
                               
            float peakNoise = FbmPerlin(wx * 0.01f + 1900f, wz * 0.01f + 1900f);
            float peak = MathF.Pow(1f - MathF.Abs(peakNoise), 2.2f);
            height += peak * mountainMask * 95f;                                        
        }

                                                                               
                                                                                
                                                                               
                                                                          
        float basinNoise = Fbm(wx * 0.018f + 2600f, wz * 0.018f + 2600f);
        float basinMask = SmoothStep(0.76f, 0.9f, basinNoise) * (1f - SmoothStep(0.72f, 0.94f, dist / MathF.Max(1f, _effectiveRadius)));
        height -= basinMask * Lerp(5f, 20f, hillRuggedness);

                                                                                
                                                                               
                                                                                    
        float ravineA = Fbm(wx * 0.012f + 5100f, wz * 0.045f + 5100f);
        float ravineB = Fbm(wx * 0.026f - 8100f, wz * 0.018f - 8100f);
        float ravineMask = SmoothStep(0.82f, 0.94f, ravineA) * SmoothStep(0.58f, 0.78f, ravineB);
        ravineMask *= 1f - SmoothStep(0.8f, 0.98f, dist / MathF.Max(1f, _effectiveRadius));
        height -= ravineMask * Lerp(8f, 32f, hillRuggedness);

                                                                        
                                                                                     
                                                                                   
                                                                               
                                                                              
                                                                                  
                                                                                   
                                                                                   
                                                                                         
        const float terraceStep = 2f;                                           
                                                                               
                                                                                     
        float rawLevel = height / terraceStep;
        float baseLevel = MathF.Floor(rawLevel);
        float levelFrac = rawLevel - baseLevel;
        float terraced = (baseLevel + SmoothStep(0.4f, 0.6f, levelFrac)) * terraceStep;
        float flatness = (1f - hillRuggedness) * 0.85f * (1f - mountainMask);
        height = Lerp(height, terraced, flatness);

        float edgeStart = _effectiveRadius * 0.82f;
        if (dist > edgeStart)
        {
            float t = Clamp01((dist - edgeStart) / (_effectiveRadius - edgeStart));
            float falloff = t * t * (3f - 2f * t);
            height = Lerp(height, -6f, falloff);
        }

                                                                          
                                                                                   
                                                                                  
                                                                            
                                                                               
                                                                       
        height = Math.Clamp(height, BedrockLevel + 8f, MaxBuildHeight - 20f);

        heightCache[cacheKey] = height;
        return height;
    }

                                                                                     
                                                                               
                                                                                 
                                                                                   
                                                                                   
                                                                               
                                                                                   
    private static float ContinentalnessToHeight(float c)
    {
                                                                                
                                                                                
                                                                             
                                                                               
                                                                         
                                                                              
                                                                             
                                                                   
        if (c < 0.15f) return Lerp(-22f, -14f, c / 0.15f);                                       
        if (c < 0.30f) return Lerp(-14f, -6f, (c - 0.15f) / 0.15f);                     
        if (c < 0.44f) return Lerp(-6f, 0f, (c - 0.30f) / 0.14f);                                             
        if (c < 0.50f) return Lerp(0f, 3f, (c - 0.44f) / 0.06f);                                               
        if (c < 0.75f) return Lerp(3f, 8f, (c - 0.50f) / 0.25f);                           
        return Lerp(8f, 14f, Clamp01((c - 0.75f) / 0.25f));                                                      
    }

                                                                                   
                                                                                    
                                                                                    
                                                                                 
                                                                                 
                                                                                
                                                                                   
    private int ComputeHeightSmoothed(int x, int z, float noiseOffsetX, float noiseOffsetZ)
    {
        float sum = 0f;
        float weightSum = 0f;
        for (int dx = -2; dx <= 2; dx++)
        {
            for (int dz = -2; dz <= 2; dz++)
            {
                int sx = x + dx, sz = z + dz;
                float distance = MathF.Abs(dx) + MathF.Abs(dz);
                float weight = distance switch { 0f => 6f, 1f => 4f, 2f => 2f, _ => 1f };
                float dist = MathF.Sqrt(sx * sx + sz * sz);
                sum += ComputeHeightRaw(sx, sz, dist, noiseOffsetX, noiseOffsetZ) * weight;
                weightSum += weight;
            }
        }
        return (int)MathF.Floor(sum / weightSum);
    }

    private static float Clamp01(float v)
    {
        return v < 0f ? 0f : v > 1f ? 1f : v;
    }

    private static float Lerp(float a, float b, float t)
    {
        return a + (b - a) * t;
    }


    private static float SmoothStep(float edge0, float edge1, float x)
    {
        float t = Clamp01((x - edge0) / (edge1 - edge0));
        return t * t * (3f - 2f * t);
    }

                                                                                           
                                                                                                    
                                                                                     
                                                                                 
    private readonly struct LandmassSpec
    {
        public readonly float CenterX, CenterY, CenterZ, Radius, Phase;
        public LandmassSpec(float cx, float cy, float cz, float radius, float phase)
        {
            CenterX = cx; CenterY = cy; CenterZ = cz; Radius = radius; Phase = phase;
        }
    }

    private List<LandmassSpec> _landmassSpecs = new();

                                                                                 
                                                                                 
                                                                                    
                                                                                      
                                                                           
                                                                         
    private void PrecomputeLandmassSpecs()
    {
        var shapeRng = new Random(_seed * 31 + 17);
        int baseCount = 3 + shapeRng.Next(4);
        int count = Math.Max(baseCount, (int)MathF.Round(baseCount * (_effectiveRadius / WorldRadius)));

        float minDist = _effectiveRadius * 0.6f;
        for (int i = 0; i < count; i++)
        {
            float angle = (float)shapeRng.NextDouble() * MathF.Tau;
            float centerDist = minDist + (float)shapeRng.NextDouble() * (_effectiveRadius - 4f - minDist);
            float cx = MathF.Cos(angle) * centerDist;
            float cz = MathF.Sin(angle) * centerDist;
            float cy = 9f + (float)shapeRng.NextDouble() * 14f;
            float radius = 2.5f + (float)shapeRng.NextDouble() * 3.5f;
            float phase = (float)shapeRng.NextDouble() * 1000f;
            _landmassSpecs.Add(new LandmassSpec(cx, cy, cz, radius, phase));
        }
    }

                                                                                       
                                                                                     
                                                                                       
                                                                                           
    private void ApplyLandmassSpecsToChunk(Vector2i coord)
    {
        int baseX = coord.X * ChunkSize;
        int baseZ = coord.Y * ChunkSize;
        float chunkMinX = baseX, chunkMaxX = baseX + ChunkSize - 1;
        float chunkMinZ = baseZ, chunkMaxZ = baseZ + ChunkSize - 1;

        foreach (var spec in _landmassSpecs)
        {
            int ir = (int)MathF.Ceiling(spec.Radius) + 2;
            if (spec.CenterX + ir < chunkMinX || spec.CenterX - ir > chunkMaxX) continue;
            if (spec.CenterZ + ir < chunkMinZ || spec.CenterZ - ir > chunkMaxZ) continue;

            for (int dx = -ir; dx <= ir; dx++)
            {
                float x = spec.CenterX + dx;
                if (x < chunkMinX || x > chunkMaxX) continue;

                for (int dz = -ir; dz <= ir; dz++)
                {
                    float z = spec.CenterZ + dz;
                    if (z < chunkMinZ || z > chunkMaxZ) continue;
                    if (MathF.Sqrt(x * x + z * z) > _effectiveRadius) continue;

                    for (int dy = -ir; dy <= ir; dy++)
                    {
                        float y = spec.CenterY + dy;

                        float dist = MathF.Sqrt(dx * dx + dy * dy * 1.6f + dz * dz);
                        float n = Fbm3D(
                            (x + _noiseOffsetX + spec.Phase) * 0.18f,
                            y * 0.18f,
                            (z + _noiseOffsetZ + spec.Phase) * 0.18f);

                        float effectiveRadius = spec.Radius * (0.65f + n * 0.7f);
                        if (dist > effectiveRadius) continue;

                        var bp = new Vector3i((int)MathF.Floor(x), (int)MathF.Floor(y), (int)MathF.Floor(z));
                        if (GetBlockLocal(bp) != BlockType.Air) continue;

                        bool topSurface = dist > effectiveRadius - 1.1f && GetBlockLocal(bp + new Vector3i(0, 1, 0)) == BlockType.Air;
                        SetBlockSilent(bp, topSurface ? GetGroundSurfaceBlock() : BlockType.Stone);
                    }
                }
            }
        }
    }

                                                                                   
                                                                                       
                                                                                
                                                                                   
                                                                                
                                                                      
    private BlockType GetBlockLocal(Vector3i pos)
    {
        var coord = ChunkCoordOf(pos.X, pos.Z);
        Chunk? chunk;
        lock (_worldDataLock) { _chunks.TryGetValue(coord, out chunk); }
        if (chunk != null)
            return chunk.TryGetLocal(pos, out var t) ? t : BlockType.Air;
        return BlockType.Air;
    }
}
