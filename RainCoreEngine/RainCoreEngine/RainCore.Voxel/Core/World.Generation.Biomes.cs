namespace RainCore;

             
                                                                                   
                                                                                    
                                                                                     
                                                                                
                                                                                    
                                                           
   
                                                                                   
                                                                                       
                                                                       
              
public partial class World
{
                                                                              
                                                                                     
                                                                               
    private const float ClimateOffsetX = 4000f;
    private const float ClimateOffsetZ = 4000f;
    private const float VarietyOffsetX = 9000f;
    private const float VarietyOffsetZ = 9000f;

                                                                             
                                                                             
                                                                             
                                                 
      
                                                                           
                                                                        
                                                            
      
                                                                           
                                                                         
                                                                       
                                                                       
                                                                        
                                                                        
                                                                       
                                                                         
                                                                             
                                                                        
                                                                          
      
                                                                               
                                                                             
                                                                      
                                                                            
                                                                              
                           
    private const float ClimateFreq = 0.0035f;
    private const float ClimateWinterThreshold = 0.18f;
    private const float ClimateSummerThreshold = 0.42f;

                                                                          
                                                                            
                                                                        
                                                                          
                                                                             
                                
    private const float ClimateBufferHalfWidth = 0.02f;

                                                                                     
                                                                                      
                                                                                   
                                                                    
    internal static (float OffsetX, float OffsetZ) NoiseOffsetsForSeed(int seed)
    {
        return ((seed % 1000) * 17.13f, (seed % 733) * 9.71f);
    }

                                                                       
                                                                           
                                                                                 
                                                                              
                                                                                    
                                                                                  
    private static float ClimateNoiseAt(int x, int z, float noiseOffsetX, float noiseOffsetZ)
    {
        return Fbm(
            (x + noiseOffsetX) * ClimateFreq + ClimateOffsetX,
            (z + noiseOffsetZ) * ClimateFreq + ClimateOffsetZ);
    }

                                                                                   
                                                                            
                                                                                   
                                                                             
                                                                             
                                                                          
                                                                            
                                                                                 
                                                                               
                                                                                   
    internal static WorldTheme DominantThemeForSeed(int seed, float sampleRadius)
    {
        var (offsetX, offsetZ) = NoiseOffsetsForSeed(seed);

        int winterCount = 0, autumnCount = 0, summerCount = 0;
        const int samplesPerAxis = 9;
        for (int i = 0; i < samplesPerAxis; i++)
        {
            for (int j = 0; j < samplesPerAxis; j++)
            {
                float u = i / (samplesPerAxis - 1f) * 2f - 1f;         
                float v = j / (samplesPerAxis - 1f) * 2f - 1f;
                int x = (int)(u * sampleRadius);
                int z = (int)(v * sampleRadius);

                float climate = ClimateNoiseAt(x, z, offsetX, offsetZ);
                if (climate < ClimateWinterThreshold) winterCount++;
                else if (climate < ClimateSummerThreshold) autumnCount++;
                else summerCount++;
            }
        }

        if (winterCount >= autumnCount && winterCount >= summerCount) return WorldTheme.Winter;
        return autumnCount >= summerCount ? WorldTheme.Autumn : WorldTheme.Summer;
    }

                                                                                  
                                                                          
                                                                             
                                                                                
                                                                               
                                                                          
                                                                       
                                                                           
                                                                        
    private float VarietyNoiseAt(int x, int z)
    {
        const float freq = 0.01f;
        return Fbm(
            (x + _noiseOffsetX) * freq + VarietyOffsetX,
            (z + _noiseOffsetZ) * freq + VarietyOffsetZ);
    }

                                                                             
                                                                           
                                                                             
                                                                          
       
                                                                        
                                                                             
                                                                          
                                                                           
                           
    private Biome GetBiomeCandidate(int x, int z)
    {
        float climate = ClimateNoiseAt(x, z, _noiseOffsetX, _noiseOffsetZ);

        if (climate < ClimateWinterThreshold - ClimateBufferHalfWidth)
        {
                                                                            
                                                                             
                                                                      
                                                                            
                                    
            float variety = VarietyNoiseAt(x, z);
            return variety < 0.58f ? Biome.Winter : Biome.TundraForest;
        }

        if (climate < ClimateWinterThreshold + ClimateBufferHalfWidth)
            return Biome.AutumnMeadow;                    

        if (climate < ClimateSummerThreshold - ClimateBufferHalfWidth)
        {
                                                                              
                                                                         
                                                                             
                                                                            
                                                                 
              
                                                                           
                                                                             
                                                                       
                                         
            float variety = VarietyNoiseAt(x, z);
            if (variety < 0.15f) return Biome.Swamp;
                if (variety < 0.23f) return Biome.AutumnForest;
            if (variety < 0.28f) return Biome.AspenForest;
            if (variety < 0.33f) return Biome.MapleForest;
            return Biome.SummerMeadow;
        }

        if (climate < ClimateSummerThreshold + ClimateBufferHalfWidth)
            return Biome.SummerMeadow;                    

        float summerVariety = VarietyNoiseAt(x, z);
        if (summerVariety < 0.12f) return Biome.Desert;
        if (summerVariety < 0.20f) return Biome.Savanna;
        if (summerVariety < 0.28f) return Biome.FlowerMeadow;
        if (summerVariety < 0.36f) return Biome.BirchForest;
        if (summerVariety < 0.43f) return Biome.OakForest;
        if (summerVariety < 0.49f) return Biome.WillowForest;
        if (summerVariety < 0.55f) return Biome.CherryForest;
        if (summerVariety < 0.60f) return Biome.DarkForest;
        if (summerVariety < 0.66f) return Biome.Jungle;
        return Biome.SummerMeadow;
    }

                                                                                   
                                                                                    
                                                                               
                                                                                  
                                                                                    
                                                                                
                                                                                           
    private const int SwampMaxHeightAboveSea = 3;

                                                                                   
                                                                                    
                                                                                
                                                                                
                                                                  
    private const int MountainRockLevel = 34;

                                                                                
                                                                   
                                                                                  
                                                                                    
                                               
    private const int MountainSnowLevel = 68;

                                                                                     
                                                                                
                                                                              
                                                                                     
                                                                                     
                                                                                
                                                                                     
                                                                                 
                                                                             
                                      
    private Biome ResolveBiome(int x, int z, int topY)
    {
        var candidate = GetBiomeCandidate(x, z);
        if (candidate == Biome.Swamp && topY > SeaLevel + SwampMaxHeightAboveSea)
            candidate = Biome.AutumnForest;

        if (topY >= SeaLevel + MountainRockLevel)
            return Biome.Mountains;

        return candidate;
    }

    public Biome GetBiomeAt(int x, int z)
    {
        int topY = ComputeHeightSmoothed(x, z, _noiseOffsetX, _noiseOffsetZ);
        return ResolveBiome(x, z, topY);
    }

                                                                                   
                                                                                  
                                                                                    
                                                                                                          
                                
       
                                                                                    
                                                                                    
                                                                                 
                                                                           
                                                                                    
                                                                              
                                                                                    
                                                                              
    public float GetWinterFactorAt(int x, int z)
    {
        if (_kind != WorldKind.ProceduralIsland)
            return _theme == WorldTheme.Winter ? 1f : 0f;

        float climate = ClimateNoiseAt(x, z, _noiseOffsetX, _noiseOffsetZ);
                                                                                     
                                                                                     
                                                                                      
                                                                              
        float t = Math.Clamp(
            (climate - ClimateWinterThreshold) / (ClimateSummerThreshold - ClimateWinterThreshold),
            0f, 1f);
        t = t * t * (3f - 2f * t);                                               
        return 1f - t;
    }

    public float GetBiomeNightDarknessAt(int x, int z)
    {
        return GetBiomeAt(x, z) switch
        {
            Biome.Winter or Biome.TundraForest => 1f,
            Biome.Mountains => 0.95f,
            Biome.Swamp => 0.72f,
            Biome.DarkForest => 0.88f,
            Biome.AspenForest or Biome.MapleForest => 0.64f,
            Biome.Desert or Biome.Savanna => 0.48f,
            Biome.FlowerMeadow => 0.56f,
            _ => 0.60f
        };
    }

    public OpenTK.Mathematics.Vector3 GetBiomeFogTint(int x, int z)
    {
        return GetBiomeAt(x, z) switch
        {
            Biome.Swamp => new(0.12f, 0.22f, 0.17f),
            Biome.Desert or Biome.Savanna => new(0.54f, 0.43f, 0.30f),
            Biome.Winter or Biome.TundraForest => new(0.40f, 0.48f, 0.60f),
            Biome.AutumnForest or Biome.AspenForest or Biome.MapleForest => new(0.48f, 0.30f, 0.22f),
            _ => new(0.30f, 0.45f, 0.38f)
        };
    }

    public bool IsSwampAt(int x, int z)
    {
        int topY = GetSurfaceHeight(x, z);
        return ResolveBiome(x, z, topY) == Biome.Swamp;
    }

    public OpenTK.Mathematics.Vector3 GetWaterColorAt(int x, int z)
    {
        var key = new OpenTK.Mathematics.Vector2i(x, z);
        lock (_worldDataLock)
        {
            if (_waterColorCache.TryGetValue(key, out var cached)) return cached;
        }

        OpenTK.Mathematics.Vector3 result;
        if (GetBiomeAt(x, z) == Biome.Swamp)
            result = new OpenTK.Mathematics.Vector3(0.16f, 0.32f, 0.18f);
        else
            result = BlockData.GetWaterColor(GetWinterFactorAt(x, z));

        lock (_worldDataLock) _waterColorCache[key] = result;
        return result;
    }

                                                                                  
                                                                                      
                                                                                  
                                                                                     
                                                                                   
                                                                                
                                                                                     
                                                                                                
    public bool IsNearWaterAt(int x, int z)
    {
        const int Radius = 8;
        for (int dx = -Radius; dx <= Radius; dx += Radius)
        {
            for (int dz = -Radius; dz <= Radius; dz += Radius)
            {
                if (GetSurfaceHeight(x + dx, z + dz) <= SeaLevel) return true;
            }
        }
        return false;
    }
}
