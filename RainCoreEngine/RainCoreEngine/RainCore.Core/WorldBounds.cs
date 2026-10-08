using OpenTK.Mathematics;

namespace RainCore;

             
                                                                               
                                                                                
                                                                               
                                                                          
              
                                                                                     
                                                                    
                                                                                
                                                                       
                                                                              
                                         
public readonly record struct WorldBounds(float MinX, float MaxX, float MinZ, float MaxZ, float MinY = float.NegativeInfinity)
{
                                                                                    
                                                                                 
    public Vector3 Clamp(Vector3 pos, float margin)
    {
        pos.X = Math.Clamp(pos.X, MinX + margin, MaxX - margin);
        pos.Z = Math.Clamp(pos.Z, MinZ + margin, MaxZ - margin);
        return pos;
    }
}