using System.Text.Json;
using OpenTK.Mathematics;

namespace RainCore;

             
                                                              
                                                                         
                                                                          
                                                                 
                                                                        
                                                                  
   
                                                                          
                                                                         
                                                                            
                                                                     
                                                                       
                                              
   
                                                                         
                                                                               
                                                                        
                                                                           
                                                                            
                                                                           
                                                                              
                                                        
                                                                        
                                                                           
                     
              
public class MeshRoomLevel : ILevel
{
    private readonly List<ModelInstance> _models = new();
    private readonly List<InteractableObject> _interactables = new();
    private readonly List<Npc> _npcs = new();
    private readonly List<TriggerZone> _triggers = new();
    private readonly List<PosterInstance> _posters = new();
    private readonly List<MoodEvent> _moodEvents = new();
    private readonly List<MoodZone> _moodZones = new();

    public string Name { get; }

    public MeshRoomLevel(string name)
    {
        Name = name;
    }

                                                                            
                                                                             
                                                                                          
    public WorldBounds Bounds
    {
        get
        {
            if (_models.Count == 0) return new WorldBounds(0, 0, 0, 0);
            float minX = float.MaxValue, maxX = float.MinValue, minZ = float.MaxValue, maxZ = float.MinValue;
            foreach (var m in _models)
            {
                m.GetWorldBounds(out var mn, out var mx);
                minX = MathF.Min(minX, mn.X); maxX = MathF.Max(maxX, mx.X);
                minZ = MathF.Min(minZ, mn.Z); maxZ = MathF.Max(maxZ, mx.Z);
            }
            return new WorldBounds(minX, maxX, minZ, maxZ);
        }
    }

    public IReadOnlyList<InteractableObject> Interactables => _interactables;
    public IReadOnlyList<Npc> Npcs => _npcs;
    public IReadOnlyList<TriggerZone> Triggers => _triggers;
    public IReadOnlyList<ModelInstance> Models => _models;
    public IReadOnlyList<PosterInstance> Posters => _posters;
    public IReadOnlyList<MoodEvent> MoodEvents => _moodEvents;
    public IReadOnlyList<MoodZone> MoodZones => _moodZones;

                                                                            
                                                                            
                                                                             
                                                                             
                                                                            
                                                                            
                                                                          
                                                                    
                                                                             
                                 
    public Vector3? PlayerStartPosition { get; set; }

                                                                                     
                                                                                         
                                                                                                    
    public SceneEnvironment Environment { get; set; } = SceneEnvironment.Default;
    public float PlayerStartYawDegrees { get; set; }

                                                                                  
                                                                                   
                                                                         
    public float WeatherCeiling => 0f;
    public float WeatherFloor => 0f;

    public bool IsSolidAt(Vector3i pos)
    {
        var point = new Vector3(pos.X, pos.Y, pos.Z);
        foreach (var m in _models)
        {
            if (!m.IsSolid) continue;
            m.GetWorldBounds(out var mn, out var mx);
            if (point.X >= mn.X && point.X <= mx.X &&
                point.Y >= mn.Y && point.Y <= mx.Y &&
                point.Z >= mn.Z && point.Z <= mx.Z)
                return true;
        }
        return false;
    }

                                                                                    
                                                                                  
                                                                                             
    public int GetSurfaceHeight(int x, int z)
    {
        float best = 0f;
        bool any = false;
        foreach (var m in _models)
        {
            if (!m.IsSolid) continue;
            m.GetWorldBounds(out var mn, out var mx);
            if (x < mn.X || x > mx.X || z < mn.Z || z > mx.Z) continue;
            if (!any || mx.Y > best) { best = mx.Y; any = true; }
        }
        return (int)MathF.Round(best);
    }

    public bool Raycast(Vector3 origin, Vector3 direction, float maxDistance, out Vector3 hitPoint, out Vector3 hitNormal)
    {
        direction = direction.LengthSquared > 0f ? Vector3.Normalize(direction) : Vector3.UnitZ;

        var found = false;
        var bestT = maxDistance;
        var bestNormal = Vector3.UnitY;

        foreach (var m in _models)
        {
            m.GetWorldBounds(out var mn, out var mx);
            if (GeometryUtils.RayIntersectsAabb(origin, direction, mn, mx, bestT, out var t, out var normal))
            {
                bestT = t;
                bestNormal = normal;
                found = true;
            }
        }

        if (!found)
        {
            hitPoint = default;
            hitNormal = default;
            return false;
        }

        hitPoint = origin + direction * bestT;
        hitNormal = bestNormal;
        return true;
    }

    public ModelInstance AddModel(GlbModel model, string modelKey, Vector3 position, Vector3 rotationDeg = default, float scale = 1f)
    {
        var instance = new ModelInstance(model, modelKey, position, rotationDeg, scale);
        _models.Add(instance);
        return instance;
    }

                                                                       
                                                                 
                                                                     
    public bool RemoveModel(ModelInstance model) => _models.Remove(model);

                                                                                         
                                                                          
                                                                                     
                                                                                   
                                                                               
                                                                                   
                                                         
    public void AddMoodEvent(MoodEvent moodEvent) => _moodEvents.Add(moodEvent);

    public void RemoveMoodEvent(MoodEvent moodEvent) => _moodEvents.Remove(moodEvent);

    public void AddMoodZone(MoodZone zone) => _moodZones.Add(zone);

    public void RemoveMoodZone(MoodZone zone) => _moodZones.Remove(zone);

    public void AddNpc(Npc npc) => _npcs.Add(npc);

    public void RemoveNpc(Npc npc) => _npcs.Remove(npc);

                                                                              
                                                                                      
                                                                  
                                                                                  
                                                                                          
    public void AddInteractable(InteractableObject interactable)
    {
        _interactables.RemoveAll(existing => existing.Id == interactable.Id);
        _interactables.Add(interactable);
    }

                                                                                  
                                                                      
    public bool RemoveInteractable(InteractableObject interactable) => _interactables.Remove(interactable);

    public void AddTrigger(TriggerZone trigger) => _triggers.Add(trigger);

    public bool RemoveTrigger(TriggerZone trigger) => _triggers.Remove(trigger);

    public void AddPoster(PosterInstance poster) => _posters.Add(poster);

                                                                              
                                                                           
                                                                                    
                                                                                 
                                                                          
                                                                                            
    public bool RemovePoster(PosterInstance poster)
    {
        if (!_posters.Remove(poster)) return false;
        poster.Dispose();
        return true;
    }

    private string SaveDirectory => Path.Combine(AppContext.BaseDirectory, "Saves", "rooms", PathUtils.SanitizeForPath(Name));

    private record struct SavedRoomModel(string ModelKey, float X, float Y, float Z,
        float RX, float RY, float RZ, float Scale, bool IsSolid, bool Visible);

                                                                                          
                                                                                              
    private record struct SavedVec3(float X, float Y, float Z);

                                                                            
                                                                                         
                                                                                     
                                                                               
                                                                             
                                          
    private record struct SavedInteractable(
        string Id, string DisplayName, float X, float Y, float Z, bool IsActive,
        string ActiveText, string InactiveText, float InteractRadius,
        bool ExamineOnly, bool OnceOnly, string? RequiredItem, int RequiredItemAmount,
        bool ConsumesItem, string LockedText, string MarkerPath, float MarkerScale);

                 
                                                                                 
                                                                                  
                                                                                      
                                                                                         
                                                                                      
                                                                                   
                                                                                      
                  
    private record struct SavedNpc(
        string Name, float X, float Y, float Z, string ModelName, float ModelScale,
        MovementPattern Movement, List<SavedVec3> PatrolPoints,
        bool InteractRunOnce, List<SavedCommand>? InteractCommands);

                                                                                   
                                                                                     
                                                                           
                                                                            
                                                                                         
    private record struct SavedPage(string? RequiredFlag, bool RequiredFlagValue, List<SavedCommand> Commands);

                 
                                                                                     
                                                                                    
                                                                                      
                                                                                      
                                                                                       
                                                                                  
                  
    private record struct SavedTrigger(
        float CenterX, float CenterY, float CenterZ, float Radius, TriggerType Type,
        string? RequiredFlag, bool RequiredFlagValue, bool RunOnce,
        float ChancePerCheck, float CheckIntervalSeconds,
        string MarkerPath, float MarkerScale,
        string DoorInteriorId, int DoorWidth, int DoorHeight, int DoorDepth,
        List<SavedCommand>? Commands, List<SavedPage>? Pages);

                                                                                       
                                                                                               
    private record struct SavedPoster(string? ImagePath, string? VideoPath,
        float X, float Y, float Z, float Scale, float NormalX, float NormalY, float NormalZ);

                 
                                                                                    
                                                                                
                                                                               
                                                                                    
                                                                                   
                                                      
                  
    void ILevel.Save()
    {
        SaveModels();
        SaveInteractables();
        SaveNpcs();
        SaveTriggers();
        SavePosters();
    }

    private void SaveModels()
    {
        try
        {
            Directory.CreateDirectory(SaveDirectory);
            var list = _models.Where(m => m.Persist).Select(m => new SavedRoomModel(
                m.ModelKey, m.Position.X, m.Position.Y, m.Position.Z,
                m.RotationDeg.X, m.RotationDeg.Y, m.RotationDeg.Z, m.Scale, m.IsSolid, m.Visible)).ToList();
            AtomicFile.WriteAllText(Path.Combine(SaveDirectory, "models.json"), JsonSerializer.Serialize(list));
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[MeshRoomLevel] Не удалось сохранить модели \"{Name}\": {ex.Message}");
        }
    }

    private void SaveInteractables()
    {
        try
        {
            Directory.CreateDirectory(SaveDirectory);
            var list = _interactables.Select(i => new SavedInteractable(
                i.Id, i.DisplayName, i.Position.X, i.Position.Y, i.Position.Z, i.IsActive,
                i.ActiveText, i.InactiveText, i.InteractRadius, i.ExamineOnly, i.OnceOnly,
                i.RequiredItem, i.RequiredItemAmount, i.ConsumesItem, i.LockedText,
                i.MarkerPath, i.MarkerScale)).ToList();
            AtomicFile.WriteAllText(Path.Combine(SaveDirectory, "interactables.json"), JsonSerializer.Serialize(list));
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[MeshRoomLevel] Не удалось сохранить интерактивные объекты \"{Name}\": {ex.Message}");
        }
    }

    private void SaveNpcs()
    {
        try
        {
            Directory.CreateDirectory(SaveDirectory);
            var list = new List<SavedNpc>();
            foreach (var n in _npcs)
            {
                List<SavedCommand>? commands = null;
                if (n.HasInteractEvent)
                {
                    try
                    {
                        commands = n.InteractCommands.Select(EventCommandCodec.Encode).ToList();
                    }
                    catch (Exception ex)
                    {
                                                                                         
                                                                                        
                                                                                       
                        Console.WriteLine($"[MeshRoomLevel] NPC \"{n.Name}\": событие взаимодействия не сохранено (не удалось закодировать команду): {ex.Message}");
                    }
                }

                list.Add(new SavedNpc(
                    n.Name, n.Position.X, n.Position.Y, n.Position.Z, n.ModelName, n.ModelScale,
                    n.Movement, n.PatrolPoints.Select(p => new SavedVec3(p.X, p.Y, p.Z)).ToList(),
                    n.InteractRunOnce, commands));
            }
            AtomicFile.WriteAllText(Path.Combine(SaveDirectory, "npcs.json"), JsonSerializer.Serialize(list));
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[MeshRoomLevel] Не удалось сохранить NPC \"{Name}\": {ex.Message}");
        }
    }

    private void SaveTriggers()
    {
        try
        {
            Directory.CreateDirectory(SaveDirectory);
            var list = new List<SavedTrigger>();
            foreach (var t in _triggers)
            {
                List<SavedCommand> commands;
                List<SavedPage> pages;
                try
                {
                    commands = t.Commands.Select(EventCommandCodec.Encode).ToList();
                                                                                  
                                                                               
                                                                                
                                                                                
                                                                                   
                                        
                    pages = t.Pages.Select(p => new SavedPage(
                        p.RequiredFlag, p.RequiredFlagValue,
                        p.Commands.Select(EventCommandCodec.Encode).ToList())).ToList();
                }
                catch (Exception ex)
                {
                                                                                      
                                                                                      
                                        
                    Console.WriteLine($"[MeshRoomLevel] Триггер в {t.Center} не сохранён (не удалось закодировать команду): {ex.Message}");
                    continue;
                }

                list.Add(new SavedTrigger(
                    t.Center.X, t.Center.Y, t.Center.Z, t.Radius, t.Type,
                    t.RequiredFlag, t.RequiredFlagValue, t.RunOnce,
                    t.ChancePerCheck, t.CheckIntervalSeconds,
                    t.MarkerPath, t.MarkerScale,
                    t.DoorInteriorId, t.DoorWidth, t.DoorHeight, t.DoorDepth,
                    commands, pages));
            }
            AtomicFile.WriteAllText(Path.Combine(SaveDirectory, "triggers.json"), JsonSerializer.Serialize(list));
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[MeshRoomLevel] Не удалось сохранить триггеры \"{Name}\": {ex.Message}");
        }
    }

    private void SavePosters()
    {
        try
        {
            Directory.CreateDirectory(SaveDirectory);
            var list = _posters.Select(p =>
            {
                                                                                      
                                                                               
                var normal = Vector3.Cross(p.Up, p.Right);
                return new SavedPoster(p.ImagePath, p.VideoPath, p.Position.X, p.Position.Y, p.Position.Z,
                    p.Scale, normal.X, normal.Y, normal.Z);
            }).ToList();
            AtomicFile.WriteAllText(Path.Combine(SaveDirectory, "posters.json"), JsonSerializer.Serialize(list));
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[MeshRoomLevel] Не удалось сохранить постеры \"{Name}\": {ex.Message}");
        }
    }

    private readonly List<(string ModelKey, Vector3 Position, Vector3 RotationDeg, float Scale, bool IsSolid, bool Visible)> _pendingSavedModels = new();

                                                                                      
                                                                                    
                                                                                             
    public static MeshRoomLevel Load(string name)
    {
        var level = new MeshRoomLevel(name);
        level.LoadModelPlacements();
        level.LoadInteractablePlacements();
        level.LoadNpcPlacements();
        level.LoadTriggerPlacements();
        level.LoadPosterPlacements();
        return level;
    }

    private void LoadModelPlacements()
    {
        var path = Path.Combine(SaveDirectory, "models.json");
        if (!File.Exists(path)) return;
        try
        {
            var list = JsonSerializer.Deserialize<List<SavedRoomModel>>(File.ReadAllText(path));
            if (list == null) return;
            foreach (var m in list)
                _pendingSavedModels.Add((m.ModelKey, new Vector3(m.X, m.Y, m.Z),
                    new Vector3(m.RX, m.RY, m.RZ), m.Scale, m.IsSolid, m.Visible));
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[MeshRoomLevel] Не удалось загрузить \"{Name}\": {ex.Message}");
        }
    }

                                                                                         
                                                                            
                                                                                    
                                                                                      
                                                                                
    public List<(string ModelKey, Vector3 Position, Vector3 RotationDeg, float Scale, bool IsSolid, bool Visible)> ConsumeSavedModelPlacements()
    {
        var result = new List<(string ModelKey, Vector3 Position, Vector3 RotationDeg, float Scale, bool IsSolid, bool Visible)>(_pendingSavedModels);
        _pendingSavedModels.Clear();
        return result;
    }

    private readonly List<InteractableObject> _pendingSavedInteractables = new();

    private void LoadInteractablePlacements()
    {
        var path = Path.Combine(SaveDirectory, "interactables.json");
        if (!File.Exists(path)) return;
        try
        {
            var list = JsonSerializer.Deserialize<List<SavedInteractable>>(File.ReadAllText(path));
            if (list == null) return;
            foreach (var i in list)
                _pendingSavedInteractables.Add(new InteractableObject(
                    i.Id, i.DisplayName, new Vector3(i.X, i.Y, i.Z), i.IsActive,
                    i.ActiveText, i.InactiveText, i.InteractRadius, i.ExamineOnly, i.OnceOnly,
                    i.RequiredItem, i.RequiredItemAmount, i.ConsumesItem, i.LockedText,
                    i.MarkerPath, i.MarkerScale));
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[MeshRoomLevel] Не удалось загрузить интерактивные объекты \"{Name}\": {ex.Message}");
        }
    }

                                                                         
                                                                                 
                                                                                     
                                                                                      
                                                                                     
                                                                              
    public List<InteractableObject> ConsumeSavedInteractables()
    {
        var result = new List<InteractableObject>(_pendingSavedInteractables);
        _pendingSavedInteractables.Clear();
        return result;
    }

    private readonly List<Npc> _pendingSavedNpcs = new();

    private void LoadNpcPlacements()
    {
        var path = Path.Combine(SaveDirectory, "npcs.json");
        if (!File.Exists(path)) return;
        try
        {
            var list = JsonSerializer.Deserialize<List<SavedNpc>>(File.ReadAllText(path));
            if (list == null) return;
            foreach (var n in list)
            {
                List<EventCommand>? commands = null;
                if (n.InteractCommands is { Count: > 0 })
                {
                    try
                    {
                        commands = n.InteractCommands.Select(EventCommandCodec.Decode).ToList();
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"[MeshRoomLevel] NPC \"{n.Name}\": событие взаимодействия не загружено (не удалось декодировать команду): {ex.Message}");
                    }
                }

                var npc = new Npc(n.Name, new Vector3(n.X, n.Y, n.Z),
                    modelName: n.ModelName,
                    movement: n.Movement,
                    patrolPoints: n.PatrolPoints?.Select(p => new Vector3(p.X, p.Y, p.Z)).ToArray(),
                    modelScale: n.ModelScale,
                    interactCommands: commands,
                    interactRunOnce: n.InteractRunOnce);
                _pendingSavedNpcs.Add(npc);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[MeshRoomLevel] Не удалось загрузить NPC \"{Name}\": {ex.Message}");
        }
    }

                                                                                 
                                                                            
                                                                                        
                                                                      
                                                                                      
                                                                                        
    public List<Npc> ConsumeSavedNpcs()
    {
        var result = new List<Npc>(_pendingSavedNpcs);
        _pendingSavedNpcs.Clear();
        return result;
    }

    private readonly List<TriggerZone> _pendingSavedTriggers = new();

    private void LoadTriggerPlacements()
    {
        var path = Path.Combine(SaveDirectory, "triggers.json");
        if (!File.Exists(path)) return;
        try
        {
            var list = JsonSerializer.Deserialize<List<SavedTrigger>>(File.ReadAllText(path));
            if (list == null) return;
            foreach (var t in list)
            {
                List<EventCommand> commands;
                List<EventPage> pages;
                try
                {
                    commands = (t.Commands ?? new List<SavedCommand>()).Select(EventCommandCodec.Decode).ToList();
                    pages = (t.Pages ?? new List<SavedPage>()).Select(p => new EventPage(
                        p.RequiredFlag, p.RequiredFlagValue,
                        p.Commands.Select(EventCommandCodec.Decode).ToList())).ToList();
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[MeshRoomLevel] Триггер в ({t.CenterX}, {t.CenterY}, {t.CenterZ}) не загружен (не удалось декодировать команду): {ex.Message}");
                    continue;
                }

                _pendingSavedTriggers.Add(new TriggerZone(
                    new Vector3(t.CenterX, t.CenterY, t.CenterZ), t.Radius, t.Type, commands,
                    requiredFlag: t.RequiredFlag, requiredFlagValue: t.RequiredFlagValue, runOnce: t.RunOnce,
                    chancePerCheck: t.ChancePerCheck, checkIntervalSeconds: t.CheckIntervalSeconds,
                    pages: pages.Count > 0 ? pages : null,
                    markerPath: t.MarkerPath, markerScale: t.MarkerScale,
                    doorInteriorId: t.DoorInteriorId, doorWidth: t.DoorWidth, doorHeight: t.DoorHeight, doorDepth: t.DoorDepth));
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[MeshRoomLevel] Не удалось загрузить триггеры \"{Name}\": {ex.Message}");
        }
    }

                                                                                    
                                                                                        
                                                                               
                                                                                    
    public List<TriggerZone> ConsumeSavedTriggers()
    {
        var result = new List<TriggerZone>(_pendingSavedTriggers);
        _pendingSavedTriggers.Clear();
        return result;
    }

    private readonly List<PosterInstance> _pendingSavedPosters = new();

    private void LoadPosterPlacements()
    {
        var path = Path.Combine(SaveDirectory, "posters.json");
        if (!File.Exists(path)) return;
        try
        {
            var list = JsonSerializer.Deserialize<List<SavedPoster>>(File.ReadAllText(path));
            if (list == null) return;
            foreach (var p in list)
            {
                var normal = new Vector3(p.NormalX, p.NormalY, p.NormalZ);
                                                                                              
                                                                                                
                if (normal.LengthSquared < 0.01f) normal = -Vector3.UnitZ;
                _pendingSavedPosters.Add(new PosterInstance(p.ImagePath, p.VideoPath, new Vector3(p.X, p.Y, p.Z), normal, p.Scale));
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[MeshRoomLevel] Не удалось загрузить постеры \"{Name}\": {ex.Message}");
        }
    }

                                                                                  
                                                                                  
                                                                                           
    public List<PosterInstance> ConsumeSavedPosters()
    {
        var result = new List<PosterInstance>(_pendingSavedPosters);
        _pendingSavedPosters.Clear();
        return result;
    }
}
