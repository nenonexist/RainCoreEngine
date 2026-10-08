namespace RainCore;

             
                                                                   
                                                                           
                                                                            
                                                                        
                                                                      
                                                                   
   
                                                                        
                                                                          
                                                                         
                                                                            
                                                                              
              
public static class MoodState
{
    public static MoodLevel Level { get; private set; } = MoodLevel.Neutral;
    public static float Value { get; private set; } = 50f;

    public static void Set(MoodLevel level, float value)
    {
        Level = level;
        Value = value;
    }
}
