using OpenTK.Mathematics;

namespace RainCore;

             
                                                                              
                                                                            
                                                                               
                                                                                  
                                      
   
                                                                       
                                                                             
                                                                           
                                                                             
                                                                            
                                                                         
                                                                         
                                                            
                                                                               
                                                                        
                                                                  
              
public interface IInteractable
{
                                                                                               
    Vector3 Position { get; }

                                                                                               
    float InteractRadius { get; }

                                                                                             
    string DisplayName { get; }

                 
                                                  
                                                                       
                                                                            
                                                            
                  
    string? Interact();
}