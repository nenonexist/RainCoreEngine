namespace RainCore;

public enum EditorConditionKind
{
    GlobalSwitch,
    SelfSwitch,
    Variable,
                                                                                   
                                                                 
    LookAtNpc,
                                                                                           
                                                                                    
                                                                                                        
    HasItem,
                                                                                        
                                                                         
                                                                                  
                                                                                
                                                                                    
                                                              
    Mood,
}

public enum EditorConditionGroup
{
    All,
    Any,
}

public sealed class EditorConditionSpec
{
    public EditorConditionKind Kind { get; set; }
    public string Name { get; set; } = "";
    public bool Expected { get; set; } = true;
    public VariableComparison Comparison { get; set; } = VariableComparison.Equal;
    public int Value { get; set; }

    public EventCondition Build()
    {
        return Kind switch
        {
            EditorConditionKind.GlobalSwitch => EventCondition.GlobalSwitch(Name, Expected),
            EditorConditionKind.SelfSwitch => EventCondition.SelfSwitch(Name, Expected),
            EditorConditionKind.Variable => EventCondition.Variable(Name, Comparison, Value),
            EditorConditionKind.LookAtNpc => EventCondition.LookAtNpc(Name, Value > 0 ? Value : 20),
            EditorConditionKind.HasItem => EventCondition.HasItem(Name, Value > 0 ? Value : 1),
            EditorConditionKind.Mood => Comparison == VariableComparison.Equal
                ? EventCondition.Mood(MoodBucketFromIndex(Value))
                : EventCondition.Mood(Comparison, Value),
            _ => EventCondition.Always(),
        };
    }

                                                                                         
                                                                               
                                                                                        
    private static MoodLevel MoodBucketFromIndex(int index) => index switch
    {
        <= 0 => MoodLevel.Calm,
        1 => MoodLevel.Neutral,
        _ => MoodLevel.Active,
    };
}

public sealed class EditorEventPageDefinition
{
    public string Name { get; set; } = "Страница";
    public EditorConditionGroup Group { get; set; } = EditorConditionGroup.All;
    public bool Negated { get; set; }
    public List<EditorConditionSpec> Conditions { get; } = new();
    public List<EventCommand> Commands { get; } = new();

    public EventPage BuildPage()
    {
        var conditions = Conditions.Count == 0
            ? EventCondition.Always()
            : Group == EditorConditionGroup.All
                ? EventCondition.All(Conditions.Select(item => item.Build()).ToArray())
                : EventCondition.Any(Conditions.Select(item => item.Build()).ToArray());
        return new EventPage(Negated ? EventCondition.Not(conditions) : conditions, Commands);
    }
}
