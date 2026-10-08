using OpenTK.Mathematics;

namespace RainCore;

                                                                              
                                                                              
                                          
public enum MoodEventEffect
{
    Ghost,
    SoundStinger,
    Both,
}

                                                                               
                                                                                  
                                                                                            
public interface IMoodEventSink
{
    void SpawnGhost(Vector3 worldPosition);
    void PlayStingerSound(string soundName);
}

             
                                                                                
                                                                         
                                                                         
                                                                                 
                                                                               
                                                             
   
                                                                      
                                                                            
                                                                           
                                                                   
   
                                                                                
                                                                                  
                                     
              
public class MoodEvent
{
    public Vector3 Center { get; private set; }
    public float Radius { get; }
    public MoodEventEffect Effect { get; }

                                                                                            
                                                              
    public string SoundName { get; }

                                                                                      
                                                                                         
                                                                                     
                                                                                             
    public float CooldownSeconds { get; }

                                                                                            
                                                                                
    public float BaseChance { get; }

                                                                                       
                                                                                    
                                                                                       
                                                                           
    private const float CheckIntervalSeconds = 2f;

    private float _cooldownRemaining;
    private float _checkElapsed;

    public MoodEvent(Vector3 center, float radius, MoodEventEffect effect,
        float cooldownSeconds, float baseChance, string soundName = "")
    {
        Center = center;
        Radius = radius;
        Effect = effect;
        SoundName = soundName;
        CooldownSeconds = cooldownSeconds;
        BaseChance = Math.Clamp(baseChance, 0f, 1f);
    }

                                                                             
                                                                            
                                 
    public void SetCenter(Vector3 center) => Center = center;

                                                                                             
                                                                                             
    public void Update(float dt, Vector3 playerPos, MoodTracker mood, IMoodEventSink sink, Random rng)
    {
        if (_cooldownRemaining > 0f)
        {
            _cooldownRemaining -= dt;
            return;
        }

        if ((playerPos - Center).LengthFast > Radius) return;

        _checkElapsed += dt;
        if (_checkElapsed < CheckIntervalSeconds) return;
        _checkElapsed = 0f;

        float chance = BaseChance * MoodChanceMultiplier(mood.Level);
        if (rng.NextDouble() > chance) return;

        Fire(playerPos, sink, rng);
        _cooldownRemaining = CooldownSeconds;
    }

                                                                                                  
                                                                                          
                                                                                          
                                      
    private static float MoodChanceMultiplier(MoodLevel level) => level switch
    {
        MoodLevel.Calm => 1.6f,
        MoodLevel.Neutral => 1f,
        MoodLevel.Active => 0.35f,
        _ => 1f,
    };

    private void Fire(Vector3 playerPos, IMoodEventSink sink, Random rng)
    {
        if (Effect is MoodEventEffect.Ghost or MoodEventEffect.Both)
            sink.SpawnGhost(PickGhostSpawnPoint(playerPos, rng));

        if (Effect is MoodEventEffect.SoundStinger or MoodEventEffect.Both && SoundName.Length > 0)
            sink.PlayStingerSound(SoundName);
    }

                                                                                        
                                                                                       
                                                                                    
                                                                                    
                                                                                               
    private Vector3 PickGhostSpawnPoint(Vector3 playerPos, Random rng)
    {
        Vector3 best = Center;
        float bestDist = -1f;
        for (int i = 0; i < 4; i++)
        {
            float angle = (float)(rng.NextDouble() * Math.PI * 2);
            float dist = Radius * (0.55f + (float)rng.NextDouble() * 0.4f);
            var candidate = Center + new Vector3(MathF.Cos(angle) * dist, 0f, MathF.Sin(angle) * dist);
            float distFromPlayer = (candidate - playerPos).LengthFast;
            if (distFromPlayer > bestDist)
            {
                bestDist = distFromPlayer;
                best = candidate;
            }
        }
        return best;
    }
}
