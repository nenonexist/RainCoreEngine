namespace RainCore;

                                                                           
                                                                        
                                                                       
                                                                         
public enum MoodOverrideSource
{
    Debug,
    Zone,
}

             
                                                                  
                                                                           
                                                                       
                                                                            
                                                                           
   
                                                                           
                                                                   
                                                                      
                                                                           
                                                                          
                                                     
   
                                                                            
                                                           
              
public class MoodTracker
{
                                                                         
    private const float ActiveThreshold = 75f;
    private const float NeutralThreshold = 45f;

                                                                                       
                                                                 
    private const float StandingSpeedThreshold = 0.3f;

                                                                                         
                                                                           
    private const float MaxRelevantSpeed = 4.5f;

                                                                                       
                                                                                                   
    private const float MovementGainPerSecond = 3.5f;

                                                                                     
                                                                                          
                                                           
    private const float DecayPerSecond = 1.4f;

                                                                                         
                                                                            
    private const float DefaultInteractionGain = 6f;

                                                                                      
                                                                                      
                                                       
    private float _value = 55f;

                                                                              
                                                                                         
    private float? _debugOverride;

                                                                                           
                                                                                        
                                                                                     
                                                                                     
                                                                                               
    private float? _zoneOverride;

                                                                                      
                                                                           
    public bool IsDebugOverridden => _debugOverride.HasValue;

                                                                                       
                                                                                    
    public bool IsZoneOverridden => _zoneOverride.HasValue;

                                                                                        
                                                                                
                                                                              
                                                                                     
                                                                                       
                                                                                          
    public float Value => _debugOverride ?? _zoneOverride ?? _value;

                                                                                     
                                                                                        
                                                                                      
                                                                                          
    public float OrganicValue => _value;

                                                                                         
                                                              
    public MoodLevel Level => Value >= ActiveThreshold ? MoodLevel.Active
        : Value >= NeutralThreshold ? MoodLevel.Neutral
        : MoodLevel.Calm;

                 
                                                                                       
                                                                                      
                                                                                   
                                                                                   
                                                                                  
                  
    public void SetDebugOverride(float value) => SetOverride(MoodOverrideSource.Debug, value);

                                                                                
                                                                                            
    public void ClearDebugOverride() => ClearOverride(MoodOverrideSource.Debug);

                 
                                                                                  
                                                                           
                                                                                    
                                                                              
                                                                      
                  
    public void SetOverride(MoodOverrideSource source, float value)
    {
        var clamped = Math.Clamp(value, 0f, 100f);
        if (source == MoodOverrideSource.Debug) _debugOverride = clamped;
        else _zoneOverride = clamped;
    }

                                                                                   
                                                                    
    public void ClearOverride(MoodOverrideSource source)
    {
        if (source == MoodOverrideSource.Debug) _debugOverride = null;
        else _zoneOverride = null;
    }

                 
                                                                                    
                                                                                         
                                                                                        
                                                     
                  
    public void Update(float dt, float horizontalSpeed)
    {
        if (horizontalSpeed > StandingSpeedThreshold)
        {
            float speedFactor = Math.Clamp(horizontalSpeed / MaxRelevantSpeed, 0f, 1f);
            _value += MovementGainPerSecond * speedFactor * dt;
        }
        else
        {
            _value -= DecayPerSecond * dt;
        }

        _value = Math.Clamp(_value, 0f, 100f);
    }

                 
                                                                                    
                                                                                       
                                                                                        
                                                                                       
                                                                                         
                  
    public void RegisterInteraction(float amount = DefaultInteractionGain)
    {
        _value = Math.Clamp(_value + amount, 0f, 100f);
    }
}
