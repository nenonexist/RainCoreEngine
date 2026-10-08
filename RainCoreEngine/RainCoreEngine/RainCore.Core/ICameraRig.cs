using OpenTK.Mathematics;

namespace RainCore;

             
                                                                                
                                                                 
                                                                              
                                                                          
                                                                            
                                                                    
                                                                            
                                                   
   
                                                                       
                                                                           
                                                                             
              
public interface ICameraRig
{
                 
                                     
                  
                                                        
                                                                                   
                                     
                                                                             
                                                                           
                                      
                
                                                                                                  
                                                                                                  
                            
                                                                           
                                                                          
                                            
                
    void Update(float dt, Camera camera, Vector3 targetPosition, float inputYawDelta, float inputPitchDelta, ILevel? level);
}
