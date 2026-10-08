using OpenTK.Mathematics;

namespace RainCore;

             
                                                                             
                                                                       
                                                                              
                                                                             
                                                                             
                                                              
   
                                                                           
                                                                           
                                                                             
                                                                        
                                                                             
                                                                           
             
   
                                                                          
                                                                           
                                                                         
                                                                        
                                              
              
public class HeadBob
{
    private const float BobFrequency = 9.5f;                                                
    private const float BobHeightAmplitude = 0.045f;
    private const float BobSwayAmplitude = 0.03f;
    private const float ReturnToRestSpeed = 8f;                                                  

    private float _phase;
    private Vector2 _current;                                       

                                                                                       
                                                                                    
                                                                               
                                                                                       
                                                                                                   
    public Vector2 Update(float dt, float horizontalSpeed, bool walking)
    {
        if (walking && horizontalSpeed > 0.05f)
        {
                                                                                    
                                                                                
            _phase += dt * BobFrequency * (horizontalSpeed / 5f);

            float bobY = MathF.Abs(MathF.Sin(_phase)) * BobHeightAmplitude;
            float bobX = MathF.Sin(_phase * 0.5f) * BobSwayAmplitude;
            var target = new Vector2(bobX, bobY);
            _current = Vector2.Lerp(_current, target, MathHelper.Clamp(dt * ReturnToRestSpeed, 0f, 1f));
        }
        else
        {
            _current = Vector2.Lerp(_current, Vector2.Zero, MathHelper.Clamp(dt * ReturnToRestSpeed, 0f, 1f));
        }

        return _current;
    }
}
