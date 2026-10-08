namespace RainCore;

             
                                                                         
                                                                          
                                                                 
                                                                      
                                                                       
              
public interface IDreamContent
{
                                                                                                   
    void BuildStructures(World world);

                                                                                                            
    void BuildNpcsAndPortal(World world);

                                                                                                                 
    List<TriggerZone> BuildTriggers(World world);

                                                                                 
                                                                              
                                                                                    
                                                                             
                                                                                
                                                                          
                                                                                     
    void PopulateChunk(World world, int chunkX, int chunkZ, Random rng) { }
}