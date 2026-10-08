namespace RainCore;

             
                                                                             
                                                                               
                                                                                     
   
                                                                                
                                                                                 
                                                                                
                                                                                 
                                                                                 
                                                                                
                                                                                 
                                                                            
                                                     
   
                                                                                      
                                                                                    
                                                                               
                                                              
              
public record SceneDefinition(
    string Id,
    string DisplayName,
    int Seed,
    float TintR,
    float TintG,
    float TintB,
    WorldTheme Theme,
    WorldKind Kind,
    string ContentId,
    float WeatherCeiling = 1f,
    int Order = 0,
    string? StructuresAsset = null);
