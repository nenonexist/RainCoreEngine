using OpenTK.Mathematics;

namespace RainCore;

             
                                                                 
                                                                               
                                                                             
                                                                            
                                                                        
   
                                                                           
                                                                          
                                                                       
                                                                              
                                                  
              
public class MoodZone
{
    public Vector3 Center { get; private set; }
    public float Radius { get; }

                                                                                      
                                                                               
                                                                                    
                                                                              
    public float TargetValue { get; }

                                                                                         
                                
    public float PullRatePerSecond { get; }

                                                                                     
                                                                         
                                                                                                
    public float ReleaseSeconds { get; }

    public MoodZone(Vector3 center, float radius, float targetValue, float pullRatePerSecond, float releaseSeconds)
    {
        Center = center;
        Radius = radius;
        TargetValue = Math.Clamp(targetValue, 0f, 100f);
        PullRatePerSecond = MathF.Max(0.01f, pullRatePerSecond);
        ReleaseSeconds = MathF.Max(0.01f, releaseSeconds);
    }

    public bool Contains(Vector3 playerPos) => (playerPos - Center).LengthFast <= Radius;

                                                                          
                                                                                   
    public void SetCenter(Vector3 center) => Center = center;

                                                                                      
                                                                                         
                                                                            
    public float DistanceTo(Vector3 playerPos) => (playerPos - Center).LengthFast;
}
