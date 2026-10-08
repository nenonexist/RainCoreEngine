using OpenTK.Mathematics;

namespace RainCore;

             
                                                                     
                                                                            
                                                                         
                                                                   
                                                                            
             
   
                                                                             
                                                                          
                                                            
   
                                                                        
                                                                              
                                                       
              
public static class ObjectStatePicker
{
                                                                                     
                                                                                               
    public static bool TryPick(ILevel level, Vector3 origin, Vector3 direction, float maxDistance, out ModelInstance? picked)
    {
        direction = direction.LengthSquared > 0f ? Vector3.Normalize(direction) : Vector3.UnitZ;

        ModelInstance? best = null;
        var bestT = maxDistance;

        foreach (var m in level.Models)
        {
            m.GetWorldBounds(out var mn, out var mx);
            if (GeometryUtils.RayIntersectsAabb(origin, direction, mn, mx, bestT, out var t, out _))
            {
                bestT = t;
                best = m;
            }
        }

        picked = best;
        return best != null;
    }

                                               
    public static void ToggleSolid(ModelInstance model) => model.IsSolid = !model.IsSolid;

                                               
    public static void ToggleVisible(ModelInstance model) => model.Visible = !model.Visible;

                                                                                         
                                                                                                  
    public static void RotateStep(ModelInstance model, float stepDegrees = 90f)
    {
        model.RotationDeg.Y = (model.RotationDeg.Y + stepDegrees) % 360f;
    }
}
