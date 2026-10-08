namespace RainCore;

             
                                                                         
                                                                              
                                                                          
                                                                              
                                                                           
                                                                               
                                                                              
                                                           
   
                                                                          
                                                                                
                                                                           
                                                                         
                                                                          
                                                                       
                                                                         
                                                                        
                                                                            
                                                       
              
public class DayNightCycle
{
                                                                                    
                                                                               
                                                                                   
    private const float CycleDurationSeconds = 1200f;                                                  

                                                                                     
                                                                                
    private const float StartPhase = 0.3f;

                                                                              
    public float Phase { get; private set; } = StartPhase;

                                                                       
                                                                             
                                                                              
                                                                      
    public void Update(float dt)
    {
        Phase += dt / CycleDurationSeconds;
        Phase -= MathF.Floor(Phase);                                                                                                            
    }

                                                                                
                                                                                
                                                                                
                                                                                         
    public void SetPhase(float phase)
    {
        Phase = phase - MathF.Floor(phase);
    }

    private float Angle => Phase * MathF.PI * 2f;

                                                                             
                                                                              
                                                                    
    public float SunHeight => -MathF.Cos(Angle);

                                                                                          
    public float MoonHeight => -SunHeight;

                                                                            
                                                                          
    public float SunHorizontal => MathF.Sin(Angle);

                                                                                             
    public float MoonHorizontal => -SunHorizontal;

                                                                                    
                                                                        
                                                                             
                                                                         
    public bool IsDaytime => SunHeight > 0f;

                                                                                 
                                                                              
                                                                            
                                                                            
                                                                              
                                                                                    
                                                                
    public float TwilightFactor
    {
        get
        {
            const float Width = 0.25f;                                                                                                      
            float t = 1f - Math.Clamp(MathF.Abs(SunHeight) / Width, 0f, 1f);
            return t * t * (3f - 2f * t);                                                 
        }
    }

                                                                                     
                                                                                 
                                                                              
                                                                               
                                                                                       
    public float DayFactor
    {
        get
        {
            const float TwilightBand = 0.15f;                                                     
            float t = (SunHeight + TwilightBand) / (2f * TwilightBand);
            t = Math.Clamp(t, 0f, 1f);
            return t * t * (3f - 2f * t);                                                   
        }
    }
}
