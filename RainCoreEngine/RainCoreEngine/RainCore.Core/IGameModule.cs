namespace RainCore;

             
                                                                         
                                                                         
                                                                            
                                                                         
                                                                           
              
   
                                                                          
                                                                              
                                                                              
                                                                          
                                                                            
                                                        
   
                                                                         
                                                                            
                                                                       
                                                                                
                                                                         
                                                                          
                                                                              
                                        
              
public interface IGameModule
{
                                                                                    
                                                                                  
                                                                                
                                         
    void Initialize(ILevel level, string projectPath);

                                                                              
                                                                             
                                                    
    void Update(float deltaTime);

                                                                                    
                                                                        
    void Render();
}
