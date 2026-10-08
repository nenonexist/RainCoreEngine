namespace RainCore;

             
                                                                      
                                                                         
              
public enum QuestStepType
{
    CollectItem,
    DeliverItem,
    TalkToNpc,
    ReachZone,
    Custom
}

             
                                                                    
                                                                     
                                                                             
                                                                       
                                                                         
                                                                           
                                                                       
                                                                   
              
public sealed class QuestStep
{
    public QuestStepType Type { get; }
    public string JournalText { get; }
    public string? ItemName { get; init; }
    public int ItemAmount { get; init; } = 1;
    public string? NpcName { get; init; }

    public QuestStep(QuestStepType type, string journalText)
    {
        Type = type;
        JournalText = journalText;
    }
}

                                                                               
                                                                                  
public sealed class QuestDefinition
{
    public string Id { get; }
    public string Title { get; }
    public string Description { get; }
    public IReadOnlyList<QuestStep> Steps { get; }

    public QuestDefinition(string id, string title, string description, IEnumerable<QuestStep> steps)
    {
        Id = id;
        Title = title;
        Description = description;
        Steps = steps.ToList();
    }
}

                                                                        
                                                                            
public static class QuestBook
{
    private static readonly Dictionary<string, QuestDefinition> Definitions = new(StringComparer.OrdinalIgnoreCase);

    public static IReadOnlyCollection<QuestDefinition> All => Definitions.Values;

    public static void Register(QuestDefinition quest)
    {
        if (string.IsNullOrWhiteSpace(quest.Id))
            throw new ArgumentException("Id квеста не может быть пустым.", nameof(quest));
        Definitions[quest.Id] = quest;
    }

    public static QuestDefinition? Find(string id) =>
        Definitions.TryGetValue(id, out var quest) ? quest : null;

    public static void Remove(string id) => Definitions.Remove(id);
}
