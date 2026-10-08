using OpenTK.Mathematics;

namespace RainCore;

             
                                                                            
                                                                        
                                                                              
                                                                     
                                                                              
                                                                            
                                                                         
                                                                           
   
                                                                          
                                                                           
                                                                            
                                                                      
                                                                        
              
public interface ILevel
{
                                                                           
    string Name { get; }

                                                           
    WorldBounds Bounds { get; }

                                                                            
    bool IsSolidAt(Vector3i pos);

                                                                   
    int GetSurfaceHeight(int x, int z);

                 
                                                                    
                                                                  
                                                                             
                                                   
                  
    bool Raycast(Vector3 origin, Vector3 direction, float maxDistance, out Vector3 hitPoint, out Vector3 hitNormal);

    IReadOnlyList<InteractableObject> Interactables { get; }
    IReadOnlyList<Npc> Npcs { get; }
    IReadOnlyList<TriggerZone> Triggers { get; }
    IReadOnlyList<ModelInstance> Models { get; }
    IReadOnlyList<PosterInstance> Posters { get; }
    IReadOnlyList<MoodEvent> MoodEvents { get; }
    IReadOnlyList<MoodZone> MoodZones { get; }

    float WeatherCeiling { get; }
    float WeatherFloor { get; }

                                                                                            
    ModelInstance AddModel(GlbModel model, string modelKey, Vector3 position, Vector3 rotationDeg = default, float scale = 1f);

                                                                                    
                                                                             
                                                                              
                                                                            
                                                                          
                                                       
    bool RemoveModel(ModelInstance model);

    void AddNpc(Npc npc);
    void RemoveNpc(Npc npc);

                                                                                    
                                                                       
                                                                              
                                                                                
                                                                         
                                                                                      
    void AddInteractable(InteractableObject interactable);

                                                                               
                                                                             
                                                                                     
    bool RemoveInteractable(InteractableObject interactable);

                                                                                     
                                                                             
                                                                           
    void AddTrigger(TriggerZone trigger);

                                                                                  
                                                                            
                                                                                 
                                                                             
                                                                                         
    bool RemoveTrigger(TriggerZone trigger);

                                                                                         
                                                                                 
                                                                           
    void AddPoster(PosterInstance poster);

                                                                                        
                                                                             
                                                                                    
                                                                                    
                                                                                
                                                                               
                                                                       
    bool RemovePoster(PosterInstance poster);

                                                                               
                                                                               
                                                                      
    void AddMoodEvent(MoodEvent moodEvent);

                                                                                             
    void RemoveMoodEvent(MoodEvent moodEvent);

                                                                               
                                                                                           
    void AddMoodZone(MoodZone zone);

                                                                                       
                                                                   
    void RemoveMoodZone(MoodZone zone);

                 
                                                                        
                                                                            
                                                                            
                         
                  
    void Save();
}
