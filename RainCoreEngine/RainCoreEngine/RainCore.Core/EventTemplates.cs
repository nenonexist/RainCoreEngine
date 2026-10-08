using OpenTK.Mathematics;

namespace RainCore;

             
                                                                       
                                                                        
                                                                             
                                                                            
                                                                
              
public static class EventTemplates
{
    public enum EventCategory
    {
        Script,
        Trigger,
        Interaction,
        Ambient,                                                                                                                   
    }

                 
                                                                        
                                                                             
                                                                             
                                                                               
                              
                  
    public static readonly (string Name, TriggerType Type, bool RunOnce, Func<Vector3i, List<EventCommand>>? Build, EventCategory Category, bool IsTextChain, float Radius, float ChancePerCheck, float CheckIntervalSeconds)[] EditorTriggerPalette =
    {
        ("Добавить текст", TriggerType.OnInteract, false, null, EventCategory.Script, true, 1.5f, 1f, 0f),
        ("Звук: портал", TriggerType.OnInteract, false, origin => new List<EventCommand> { new PlaySoundCommand("portal") }, EventCategory.Script, false, 1.5f, 1f, 0f),
        ("Звук: npc_talk", TriggerType.OnInteract, false, origin => new List<EventCommand> { new PlaySoundCommand("npc_talk") }, EventCategory.Script, false, 1.5f, 1f, 0f),
        ("Предмет: ключ", TriggerType.OnInteract, true, origin => new List<EventCommand> { new GiveItemCommand("ключ") }, EventCategory.Script, false, 1.5f, 1f, 0f),
        ("Явление: дождь", TriggerType.OnInteract, false, origin => new List<EventCommand>
            {
                new SetWeatherCommand(true, 8f),
                new SetWeatherCommand(false),
            }, EventCategory.Trigger, false, 1.5f, 1f, 0f),
        ("Явление: выключить", TriggerType.OnInteract, false, origin => new List<EventCommand> { new SetWeatherCommand(false) }, EventCategory.Trigger, false, 1.5f, 1f, 0f),
        ("НПС: наблюдает и исчезает", TriggerType.OnInteract, false, origin => new List<EventCommand>
            {
                new ShowMessage("", "Кто-то наблюдает за тобой...", 3f),
                new SpawnWatchNpcCommand(new Npc("Наблюдатель", new Vector3(origin.X + 0.5f, origin.Y, origin.Z + 0.5f), Array.Empty<string>())),
            }, EventCategory.Trigger, false, 1.5f, 1f, 0f),
        ("Телепорт: точка A", TriggerType.OnInteract, false, origin => new List<EventCommand> { new TeleportCommand(0f, 20f, 0f) }, EventCategory.Interaction, false, 1.5f, 1f, 0f),
        ("Текст+предмет: находка", TriggerType.OnInteract, true, origin => new List<EventCommand>
            {
                new ShowMessage("", "Ты нашёл что-то знакомое среди травы...", 3f),
                new GiveItemCommand("находка"),
            }, EventCategory.Script, false, 1.5f, 1f, 0f),

                                                                                     
        ("Фон: далёкий шорох", TriggerType.Parallel, false, origin => new List<EventCommand>
            {
                new PlaySoundCommand("npc_talk"),                                                                
            }, EventCategory.Ambient, false, 14f, 0.12f, 6f),                                                              
        ("Фон: силуэт вдали", TriggerType.Parallel, true, origin => new List<EventCommand>
            {
                new ShowMessage("", "Что-то мелькнуло на краю зрения...", 2.5f),
                new SpawnWatchNpcCommand(new Npc("???", new Vector3(origin.X + 0.5f, origin.Y, origin.Z + 0.5f), Array.Empty<string>()), duration: 3f),
            }, EventCategory.Ambient, false, 20f, 0.05f, 10f),                                                                  
        ("Авто: при входе в сон", TriggerType.Auto, true, origin => new List<EventCommand>
            {
                new ShowMessage("", "...", 3f),                                                                
            }, EventCategory.Ambient, false, 0f, 1f, 0f),
    };
}