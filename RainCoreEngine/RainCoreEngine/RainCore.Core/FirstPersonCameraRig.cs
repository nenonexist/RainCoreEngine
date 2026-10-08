using OpenTK.Mathematics;

namespace RainCore;

             
                                                                       
                                                                      
                                                                             
                                    
              
public class FirstPersonCameraRig : ICameraRig
{
    public void Update(float dt, Camera camera, Vector3 targetPosition, float inputYawDelta, float inputPitchDelta, ILevel? level)
    {
        camera.Rotate(inputYawDelta, inputPitchDelta);
        camera.Position = targetPosition;
    }
}
