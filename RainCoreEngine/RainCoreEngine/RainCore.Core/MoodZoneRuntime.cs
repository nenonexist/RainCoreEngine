using OpenTK.Mathematics;

namespace RainCore;

             
                                                                        
                                                                     
                                                                          
                                                                        
                                                                    
                                                                              
                                                                   
                                                                       
   
                                                          
                                                                           
                                                                           
                                                                                
                                                                        
                                                                                
                                                         
              
public sealed class MoodZoneRuntime
{
    private MoodZone? _activeZone;

                                                                                   
                                                                                    
                                                                                   
    private float? _pulledValue;

    private bool _releasing;
    private float _releaseElapsed;
    private float _releaseDuration;
    private float _releaseStartValue;

                                                                                        
                                                                                       
    public void Update(float dt, Vector3 playerPos, IReadOnlyList<MoodZone> zones, MoodTracker mood)
    {
        var nearest = FindNearest(zones, playerPos);

        if (nearest != null)
        {
            _releasing = false;
            _activeZone = nearest;
            float current = _pulledValue ?? mood.OrganicValue;
            current = MoveToward(current, nearest.TargetValue, nearest.PullRatePerSecond * dt);
            _pulledValue = current;
            mood.SetOverride(MoodOverrideSource.Zone, current);
            return;
        }

        if (_activeZone == null) return;                                                         

        if (!_releasing)
        {
                                                                               
                                                                                  
            _releasing = true;
            _releaseDuration = _activeZone.ReleaseSeconds;
            _releaseElapsed = 0f;
            _releaseStartValue = _pulledValue ?? mood.OrganicValue;
        }

        _releaseElapsed += dt;
        if (_releaseElapsed >= _releaseDuration)
        {
            mood.ClearOverride(MoodOverrideSource.Zone);
            _activeZone = null;
            _releasing = false;
            _pulledValue = null;
            return;
        }

                                                                              
                                                                                  
                                                                                   
                                                                           
        float t = Math.Clamp(_releaseElapsed / _releaseDuration, 0f, 1f);
        float blended = _releaseStartValue + (mood.OrganicValue - _releaseStartValue) * t;
        _pulledValue = blended;
        mood.SetOverride(MoodOverrideSource.Zone, blended);
    }

                                                                                      
                                                                                 
                                             
    private static MoodZone? FindNearest(IReadOnlyList<MoodZone> zones, Vector3 playerPos)
    {
        MoodZone? nearest = null;
        float bestDist = float.MaxValue;
        foreach (var zone in zones)
        {
            if (!zone.Contains(playerPos)) continue;
            float dist = zone.DistanceTo(playerPos);
            if (dist < bestDist) { bestDist = dist; nearest = zone; }
        }
        return nearest;
    }

    private static float MoveToward(float current, float target, float maxDelta)
    {
        if (MathF.Abs(target - current) <= maxDelta) return target;
        return current + MathF.Sign(target - current) * maxDelta;
    }
}
