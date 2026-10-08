using OpenTK.Mathematics;

namespace RainCore;

             
                                                                      
                                                                           
                                                                  
                                                                   
                                                                      
                                                                           
              
public class ThirdPersonOrbitCameraRig : ICameraRig
{
                                                                                            
    public float Distance = 4.5f;

                                                                                                          
    public float PivotHeight = 1.5f;

                                                                                                          
    public float ShoulderOffset = 0f;

                                                                                                        
    public float MinDistance = 0.4f;

                                                                                                  
    public float CollisionBuffer = 0.15f;

    public void Update(float dt, Camera camera, Vector3 targetPosition, float inputYawDelta, float inputPitchDelta, ILevel? level)
    {
        camera.Rotate(inputYawDelta, inputPitchDelta);

        var pivot = targetPosition + Vector3.UnitY * PivotHeight;
        var back = -camera.Front;
        var desiredDistance = Distance;

        if (level != null && level.Raycast(pivot, back, Distance, out var hitPoint, out _))
        {
            var hitDistance = (hitPoint - pivot).Length - CollisionBuffer;
            desiredDistance = MathF.Max(MinDistance, MathF.Min(Distance, hitDistance));
        }

        var shoulder = ShoulderOffset != 0f ? camera.Right * ShoulderOffset : Vector3.Zero;
        camera.Position = pivot + back * desiredDistance + shoulder;
    }
}
