using System.Text.Json;

namespace RainCore;

public enum QuestState
{
    NotStarted,
    Active,
    Completed,
    Failed
}

             
                                                                           
                                                                          
                                                                             
                                                                               
                                                          
              
public static class QuestTracker
{
    private readonly record struct Progress(QuestState State, int Step);

    private static readonly Dictionary<string, Progress> _progress = new(StringComparer.OrdinalIgnoreCase);

    public static QuestState GetState(string questId) =>
        _progress.TryGetValue(questId, out var p) ? p.State : QuestState.NotStarted;

    public static int GetStep(string questId) =>
        _progress.TryGetValue(questId, out var p) ? p.Step : 0;

                                                                                       
                                                                               
                                                                               
                                                                             
                                                                                                  
    public static void Start(string questId)
    {
        if (GetState(questId) is QuestState.Active or QuestState.Completed) return;
        _progress[questId] = new Progress(QuestState.Active, 0);
    }

                                                                                    
                                                                                       
                                                                                              
    public static void Advance(string questId)
    {
        if (GetState(questId) != QuestState.Active) return;
        var next = GetStep(questId) + 1;
        var definition = QuestBook.Find(questId);
        var completed = definition != null && next >= definition.Steps.Count;
        _progress[questId] = new Progress(completed ? QuestState.Completed : QuestState.Active, next);
    }

                                                                                           
    public static void Complete(string questId)
    {
        _progress[questId] = new Progress(QuestState.Completed, GetStep(questId));
    }

                                                                                   
                                              
    public static void Fail(string questId)
    {
        _progress[questId] = new Progress(QuestState.Failed, GetStep(questId));
    }

    private static string SaveFilePath => Path.Combine(AppContext.BaseDirectory, "Saves", "quests.json");

    private record struct SaveEntry(string QuestId, QuestState State, int Step);

                                                                                     
                                                                                            
    public static void Save()
    {
        try
        {
            Directory.CreateDirectory(Path.GetDirectoryName(SaveFilePath)!);
            var list = _progress.Select(kv => new SaveEntry(kv.Key, kv.Value.State, kv.Value.Step)).ToList();
            AtomicFile.WriteAllText(SaveFilePath, JsonSerializer.Serialize(list));
            Console.WriteLine($"[Квесты] Сохранено: {list.Count} -> {SaveFilePath}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[Квесты] Не удалось сохранить: {ex.Message}");
        }
    }

                                                                                        
                                                                                                  
    public static void Load()
    {
        if (!File.Exists(SaveFilePath)) return;
        try
        {
            var list = JsonSerializer.Deserialize<List<SaveEntry>>(File.ReadAllText(SaveFilePath)) ?? new();
            _progress.Clear();
            foreach (var entry in list) _progress[entry.QuestId] = new Progress(entry.State, entry.Step);
            Console.WriteLine($"[Квесты] Загружено: {list.Count}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[Квесты] Не удалось загрузить: {ex.Message}");
        }
    }
}
