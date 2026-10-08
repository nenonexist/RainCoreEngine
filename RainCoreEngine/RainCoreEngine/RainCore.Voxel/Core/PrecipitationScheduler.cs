namespace RainCore;

             
                                                                                 
                                                                         
                                                                    
                                                                               
                                                                                
                                                           
   
                                                                                
                                                                                      
                                                                                    
                                                                                   
                                                                               
               
   
                                                                                 
                                                               
              
public class PrecipitationScheduler
{
                                                                               
                                                    
    private const float MinActiveSeconds = 25f;
    private const float MaxActiveSeconds = 70f;
    private const float MinCalmSeconds = 20f;
    private const float MaxCalmSeconds = 55f;

                                                                                      
                                                                                 
                                                                               
    private const float FadeSpeed = 1f / 5f;

                                                                             
                                                                                
                                                                                  
                                                                                        
                                                                                  
    private const float BaseActiveChance = 0.5f;
    private const float CalmnessChanceBonus = 0.2f;

    private bool _active;
    private float _phaseTimeLeft;
    private float _fade;

                                                                             
                                                                               
    public float Multiplier => _fade;

                                                                                     
                                                                                      
                                                                          
    public void Build(Random rng)
    {
        _active = rng.NextDouble() < 0.5;
        _phaseTimeLeft = RandomPhaseDuration(rng, _active);
        _fade = _active ? 1f : 0f;
    }

                                                                                       
                                                                                   
                                                                                   
                                                                                  
                                                                                    
    public void Update(float dt, Random rng, float calmness)
    {
        _phaseTimeLeft -= dt;
        if (_phaseTimeLeft <= 0f)
        {
            if (_active)
            {
                _active = false;
            }
            else
            {
                float chance = BaseActiveChance + Math.Clamp(calmness, 0f, 1f) * CalmnessChanceBonus;
                _active = rng.NextDouble() < chance;
            }
            _phaseTimeLeft = RandomPhaseDuration(rng, _active);
        }

        float target = _active ? 1f : 0f;
        _fade = MathHelperMoveTowards(_fade, target, FadeSpeed * dt);
    }

    private static float RandomPhaseDuration(Random rng, bool active) => active
        ? MinActiveSeconds + (float)rng.NextDouble() * (MaxActiveSeconds - MinActiveSeconds)
        : MinCalmSeconds + (float)rng.NextDouble() * (MaxCalmSeconds - MinCalmSeconds);

    private static float MathHelperMoveTowards(float current, float target, float maxDelta)
    {
        if (MathF.Abs(target - current) <= maxDelta) return target;
        return current + MathF.Sign(target - current) * maxDelta;
    }
}
