namespace RainCore;

             
                                                                            
                                                                           
                             
              
internal sealed class WorldEntityStore
{
    public List<Npc> Npcs { get; } = new();
    public List<InteractableObject> Interactables { get; } = new();
    public List<ModelInstance> Models { get; } = new();
    public List<TriggerZone> Triggers { get; } = new();
    public List<MoodEvent> MoodEvents { get; } = new();
    public List<MoodZone> MoodZones { get; } = new();
    public List<ItemDefinition> Items { get; } = new();
}
