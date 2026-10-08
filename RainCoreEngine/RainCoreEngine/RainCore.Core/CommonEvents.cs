namespace RainCore;

                                                                                                   
public static class CommonEvents
{
    private static readonly Dictionary<string, IReadOnlyList<EventCommand>> Definitions =
        new(StringComparer.OrdinalIgnoreCase);

    public static IReadOnlyCollection<string> Names => Definitions.Keys;

    public static void Register(string name, IEnumerable<EventCommand> commands)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Имя общего события не может быть пустым.", nameof(name));
        Definitions[name.Trim()] = commands.ToList();
    }

    public static bool TryGet(string name, out IReadOnlyList<EventCommand> commands) =>
        Definitions.TryGetValue(name, out commands!);

    public static void Remove(string name) => Definitions.Remove(name);
}

                                                                                                  
public sealed class EventPage
{
    public EventCondition Condition { get; }
    public IReadOnlyList<EventCommand> Commands { get; }

                                                                                      
                                                                                       
                                                                                       
                                                                              
                                                                                                      
    public string? RequiredFlag { get; }
    public bool RequiredFlagValue { get; }

    public EventPage(EventCondition condition, IEnumerable<EventCommand> commands)
    {
        Condition = condition;
        Commands = commands.ToList();
    }

                                                                                      
                                                                            
                                                                                 
                                  
    public EventPage(string? requiredFlag, bool requiredFlagValue, IEnumerable<EventCommand> commands)
        : this(requiredFlag == null ? EventCondition.Always() : EventCondition.GlobalSwitch(requiredFlag, requiredFlagValue), commands)
    {
        RequiredFlag = requiredFlag;
        RequiredFlagValue = requiredFlagValue;
    }
}
