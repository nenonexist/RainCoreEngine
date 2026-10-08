using OpenTK.Mathematics;

namespace RainCore;

                                                                   
                                                                       
                                    

public partial class World
{

    private static float Fbm(float x, float z)
    {
        float total = 0f, amplitude = 1f, frequency = 1f, maxAmp = 0f;
        for (int octave = 0; octave < 3; octave++)
        {
            total += ValueNoise(x * frequency, z * frequency) * amplitude;
            maxAmp += amplitude;
            amplitude *= 0.5f;
            frequency *= 2f;
        }
        return total / maxAmp;
    }


    private static float ValueNoise(float x, float z)
    {
        int x0 = (int)MathF.Floor(x), z0 = (int)MathF.Floor(z);
        int x1 = x0 + 1, z1 = z0 + 1;
        float tx = x - x0, tz = z - z0;

        float v00 = HashToFloat(x0, z0);
        float v10 = HashToFloat(x1, z0);
        float v01 = HashToFloat(x0, z1);
        float v11 = HashToFloat(x1, z1);

        float sx = tx * tx * (3f - 2f * tx);
        float sz = tz * tz * (3f - 2f * tz);

        float ix0 = v00 + (v10 - v00) * sx;
        float ix1 = v01 + (v11 - v01) * sx;
        return ix0 + (ix1 - ix0) * sz;
    }


    private static float HashToFloat(int x, int z)
    {
        unchecked
        {
            int h = x * 374761393 + z * 668265263;
            h = (h ^ (h >> 13)) * 1274126177;
            h ^= h >> 16;
            return (h & 0xFFFFFF) / (float)0xFFFFFF;
        }
    }


                                                                                      
                                                                                       
                                                                                         
                                                                                        
                                                                                        
                                                                      
    private static float Fbm3D(float x, float y, float z)
    {
        float total = 0f, amplitude = 1f, frequency = 1f, maxAmp = 0f;
        for (int octave = 0; octave < 2; octave++)
        {
            total += ValueNoise3D(x * frequency, y * frequency, z * frequency) * amplitude;
            maxAmp += amplitude;
            amplitude *= 0.5f;
            frequency *= 2f;
        }
        return total / maxAmp;
    }


    private static float ValueNoise3D(float x, float y, float z)
    {
        int x0 = (int)MathF.Floor(x), y0 = (int)MathF.Floor(y), z0 = (int)MathF.Floor(z);
        int x1 = x0 + 1, y1 = y0 + 1, z1 = z0 + 1;
        float tx = x - x0, ty = y - y0, tz = z - z0;
        float sx = tx * tx * (3f - 2f * tx);
        float sy = ty * ty * (3f - 2f * ty);
        float sz = tz * tz * (3f - 2f * tz);

        float v000 = HashToFloat3D(x0, y0, z0), v100 = HashToFloat3D(x1, y0, z0);
        float v010 = HashToFloat3D(x0, y1, z0), v110 = HashToFloat3D(x1, y1, z0);
        float v001 = HashToFloat3D(x0, y0, z1), v101 = HashToFloat3D(x1, y0, z1);
        float v011 = HashToFloat3D(x0, y1, z1), v111 = HashToFloat3D(x1, y1, z1);

        float x00 = v000 + (v100 - v000) * sx;
        float x10 = v010 + (v110 - v010) * sx;
        float x01 = v001 + (v101 - v001) * sx;
        float x11 = v011 + (v111 - v011) * sx;

        float y0i = x00 + (x10 - x00) * sy;
        float y1i = x01 + (x11 - x01) * sy;
        return y0i + (y1i - y0i) * sz;
    }


    private static float HashToFloat3D(int x, int y, int z)
    {
        unchecked
        {
            int h = x * 374761393 + y * 668265263 + z * 2147483647;
            h = (h ^ (h >> 13)) * 1274126177;
            h ^= h >> 16;
            return (h & 0xFFFFFF) / (float)0xFFFFFF;
        }
    }


                                                                   
                                                                     
                                                                   
                                                                             
                                                                            
                                                                        
                                                                         
                                                                        
                                                                          
                                                                            
                                                                            
                                                                          
                                                   
      
                                                                                 
                                                                             
                                                                           
                                                                       
                                                                             
                                                                
                                                            

    private static Vector2 GradientAt(int x, int z)
    {
                                                                              
                                                                          
                                                              
        unchecked
        {
            int h = x * 668265263 + z * 374761393;
            h = (h ^ (h >> 15)) * unchecked((int)2246822519);
            h ^= h >> 13;
            h *= unchecked((int)3266489917);
            h ^= h >> 16;
            float angle = (h & 0xFFFFFF) / (float)0xFFFFFF * MathF.Tau;
            return new Vector2(MathF.Cos(angle), MathF.Sin(angle));
        }
    }


                                                                                       
                                                                                                 
    private static float PerlinFade(float t) => t * t * t * (t * (t * 6f - 15f) + 10f);


                                                                                    
                                                                                                     
    private static float PerlinNoise(float x, float z)
    {
        int x0 = (int)MathF.Floor(x), z0 = (int)MathF.Floor(z);
        int x1 = x0 + 1, z1 = z0 + 1;
        float tx = x - x0, tz = z - z0;

        float n00 = Vector2.Dot(GradientAt(x0, z0), new Vector2(tx, tz));
        float n10 = Vector2.Dot(GradientAt(x1, z0), new Vector2(tx - 1f, tz));
        float n01 = Vector2.Dot(GradientAt(x0, z1), new Vector2(tx, tz - 1f));
        float n11 = Vector2.Dot(GradientAt(x1, z1), new Vector2(tx - 1f, tz - 1f));

        float sx = PerlinFade(tx), sz = PerlinFade(tz);
        float ix0 = n00 + (n10 - n00) * sx;
        float ix1 = n01 + (n11 - n01) * sx;
        return (ix0 + (ix1 - ix0) * sz) * 1.4142f;                                                                          
    }


                                                                                          
                                                                                                     
    private static float FbmPerlin(float x, float z)
    {
        float total = 0f, amplitude = 1f, frequency = 1f, maxAmp = 0f;
        for (int octave = 0; octave < 4; octave++)
        {
            total += PerlinNoise(x * frequency, z * frequency) * amplitude;
            maxAmp += amplitude;
            amplitude *= 0.5f;
            frequency *= 2f;
        }
        return total / maxAmp;          
    }


                 
                                                                                       
                                                                                    
                                                                                 
                                                                               
                                                                                 
                                                                               
                                                                                     
                                                                       
                  
    private static (float X, float Z) DomainWarp(float x, float z)
    {
        const float warpFrequency = 0.010f;
        const float warpStrength = 22f;

        float wx = FbmPerlin(x * warpFrequency + 91.7f, z * warpFrequency + 13.1f);
        float wz = FbmPerlin(x * warpFrequency - 47.3f, z * warpFrequency + 68.9f);

        return (x + wx * warpStrength, z + wz * warpStrength);
    }
}
