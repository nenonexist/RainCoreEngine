namespace RainCore;

             
                                                                         
                                                                       
                                                                           
                                                                       
                                                                            
                                                                         
                                                                        
                                                       
              
public readonly record struct SavedCommand(
    string Kind,
    List<string> Args,
    List<SavedCommand>? ThenCommands = null,
    List<SavedCommand>? ElseCommands = null,
    List<SavedCommand>? Body = null);

                                                                                                
public static class EventCommandCodec
{
    public static SavedCommand Encode(EventCommand command)
    {
        return EventCommandRegistry.Encode(command);
    }

    public static EventCommand Decode(SavedCommand data)
    {
        return EventCommandRegistry.Decode(data);
    }
}