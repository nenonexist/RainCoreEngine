using System.Text.Json;
using OpenTK.Mathematics;

namespace RainCore;

public partial class World
{
    public void AddInteractable(InteractableObject interactable)
    {
        _entities.Interactables.RemoveAll(item => item.Id == interactable.Id);
        _entities.Interactables.Add(interactable);
    }

                                                                                       
                                                                                       
                                                                              
    public bool RemoveInteractable(InteractableObject interactable)
    {
        return _entities.Interactables.Remove(interactable);
    }

    public void SaveInteractableStates()
    {
        try
        {
            Directory.CreateDirectory(SaveDirectory);
            var states = _entities.Interactables.Select(item => new SavedInteractableState(item.Id, item.IsActive)).ToList();
            AtomicFile.WriteAllText(Path.Combine(SaveDirectory, "interactables.json"), JsonSerializer.Serialize(states));
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[Мир] Не удалось сохранить состояния объектов: {ex.Message}");
        }
    }

    private void LoadInteractableStates()
    {
        var path = Path.Combine(SaveDirectory, "interactables.json");
        if (!File.Exists(path)) return;
        try
        {
            var states = JsonSerializer.Deserialize<List<SavedInteractableState>>(File.ReadAllText(path));
            if (states == null) return;
            foreach (var state in states)
            {
                var item = _entities.Interactables.FirstOrDefault(candidate => candidate.Id == state.Id);
                item?.SetState(state.IsActive);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[Мир] Не удалось загрузить состояния объектов: {ex.Message}");
        }
    }

                                                                                            
                                                                                        
                                                                                        
                                                                                          
                                                                                       
                           
    private record struct SavedWorldSettings(
        List<string> DisabledMusicTracks, List<string> MusicTrackOrder,
        List<string> DisabledAmbientTracks, List<string> AmbientTrackOrder,
        float WeatherCeiling = -1f, float WeatherFloor = -1f);

                                                                                    
                                                                                     
                                                                                                 
                                                                                         
                                                                        
                                                        
    public void SaveWorldSettings()
    {
        try
        {
            Directory.CreateDirectory(SaveDirectory);
            AtomicFile.WriteAllText(Path.Combine(SaveDirectory, "world_settings.json"),
                JsonSerializer.Serialize(new SavedWorldSettings(
                    DisabledMusicTracks.ToList(), MusicTrackOrder.ToList(),
                    DisabledAmbientTracks.ToList(), AmbientTrackOrder.ToList(),
                    _weatherCeiling, _weatherFloor)));
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[Мир] Не удалось сохранить настройки мира: {ex.Message}");
        }
    }

                 
                                                                            
                                                                                  
                                                                              
                                                                                     
                                                                     
                  
    private void LoadWorldSettings()
    {
        var path = Path.Combine(SaveDirectory, "world_settings.json");
        if (!File.Exists(path)) return;
        try
        {
            using var doc = JsonDocument.Parse(File.ReadAllText(path));
            var root = doc.RootElement;

            if (root.TryGetProperty("DisabledMusicTracks", out var disabledMusicEl) && disabledMusicEl.ValueKind == JsonValueKind.Array)
            {
                DisabledMusicTracks.Clear();
                foreach (var item in disabledMusicEl.EnumerateArray())
                    if (item.GetString() is { Length: > 0 } s) DisabledMusicTracks.Add(s);

                if (root.TryGetProperty("MusicTrackOrder", out var musicOrderEl) && musicOrderEl.ValueKind == JsonValueKind.Array)
                {
                    MusicTrackOrder.Clear();
                    foreach (var item in musicOrderEl.EnumerateArray())
                        if (item.GetString() is { Length: > 0 } s) MusicTrackOrder.Add(s);
                }
            }
            else
            {
                MigrateLegacyMusicSettings(root);
            }

            if (root.TryGetProperty("DisabledAmbientTracks", out var disabledAmbientEl) && disabledAmbientEl.ValueKind == JsonValueKind.Array)
            {
                DisabledAmbientTracks.Clear();
                foreach (var item in disabledAmbientEl.EnumerateArray())
                    if (item.GetString() is { Length: > 0 } s) DisabledAmbientTracks.Add(s);

                if (root.TryGetProperty("AmbientTrackOrder", out var ambientOrderEl) && ambientOrderEl.ValueKind == JsonValueKind.Array)
                {
                    AmbientTrackOrder.Clear();
                    foreach (var item in ambientOrderEl.EnumerateArray())
                        if (item.GetString() is { Length: > 0 } s) AmbientTrackOrder.Add(s);
                }
            }
            else
            {
                MigrateLegacyAmbientSettings(root);
            }

                                                                                         
                                                                                     
            if (root.TryGetProperty("WeatherCeiling", out var weatherCeilingEl) &&
                weatherCeilingEl.ValueKind == JsonValueKind.Number && weatherCeilingEl.GetSingle() >= 0f)
                SetWeatherCeiling(weatherCeilingEl.GetSingle());

            if (root.TryGetProperty("WeatherFloor", out var weatherFloorEl) &&
                weatherFloorEl.ValueKind == JsonValueKind.Number && weatherFloorEl.GetSingle() >= 0f)
                SetWeatherFloor(weatherFloorEl.GetSingle());
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[Мир] Не удалось загрузить настройки мира: {ex.Message}");
        }
    }

                                                                      
                                                                                       
                                                                                          
                                                                                     
                                                                                       
                                                                                         
                                                                               
    private void MigrateLegacyMusicSettings(JsonElement root)
    {
        var legacyPlaylist = new List<string>();
        bool legacyIsCustom;

        if (root.TryGetProperty("MusicPlaylist", out var playlistEl) && playlistEl.ValueKind == JsonValueKind.Array)
        {
            foreach (var item in playlistEl.EnumerateArray())
                if (item.GetString() is { Length: > 0 } s) legacyPlaylist.Add(s);

            legacyIsCustom = root.TryGetProperty("MusicPlaylistIsCustom", out var customEl)
                ? customEl.GetBoolean()
                : legacyPlaylist.Count > 0;
        }
        else if (root.TryGetProperty("MusicPath", out var legacyEl) && legacyEl.ValueKind == JsonValueKind.String
                 && legacyEl.GetString() is { Length: > 0 } legacy)
        {
            legacyPlaylist.Add(legacy);
            legacyIsCustom = true;
            Console.WriteLine($"[Мир] Мигрирован старый одиночный MusicPath \"{legacy}\" сна \"{DreamName}\" в новый формат музыкального менеджера.");
        }
        else
        {
            return;                                                                     
        }

        MusicTrackOrder.Clear();
        MusicTrackOrder.AddRange(legacyPlaylist);
        if (!legacyIsCustom) return;                               

        var onDisk = AudioLibrary.ScanFileNames(AudioLibrary.MusicDirFor(SanitizeForPath(DreamName)));
        var kept = new HashSet<string>(legacyPlaylist, StringComparer.OrdinalIgnoreCase);
        DisabledMusicTracks.Clear();
        foreach (var name in onDisk)
            if (!kept.Contains(name))
                DisabledMusicTracks.Add(name);

        Console.WriteLine($"[Мир] Старый ручной плейлист музыки сна \"{DreamName}\" переведён в новый формат: {legacyPlaylist.Count} трек(ов) включено, {DisabledMusicTracks.Count} выключено. Нажми O в игре, чтобы зафиксировать.");
    }

                                                                             
                                                                                        
    private void MigrateLegacyAmbientSettings(JsonElement root)
    {
        if (!root.TryGetProperty("AmbientPlaylist", out var ambientEl) || ambientEl.ValueKind != JsonValueKind.Array)
            return;                                                                     

        var legacyPlaylist = new List<string>();
        foreach (var item in ambientEl.EnumerateArray())
            if (item.GetString() is { Length: > 0 } s) legacyPlaylist.Add(s);

        bool legacyIsCustom = root.TryGetProperty("AmbientPlaylistIsCustom", out var customEl)
            ? customEl.GetBoolean()
            : legacyPlaylist.Count > 0;

        AmbientTrackOrder.Clear();
        AmbientTrackOrder.AddRange(legacyPlaylist);
        if (!legacyIsCustom) return;

        var onDisk = AudioLibrary.ScanFileNames(AudioLibrary.AmbientDir());
        var kept = new HashSet<string>(legacyPlaylist, StringComparer.OrdinalIgnoreCase);
        DisabledAmbientTracks.Clear();
        foreach (var name in onDisk)
            if (!kept.Contains(name))
                DisabledAmbientTracks.Add(name);

        Console.WriteLine($"[Мир] Старый ручной плейлист эмбиента сна \"{DreamName}\" переведён в новый формат: {legacyPlaylist.Count} трек(ов) включено, {DisabledAmbientTracks.Count} выключено. Нажми O в игре, чтобы зафиксировать.");
    }

    private record struct SavedModelPlacement(string ModelKey, float X, float Y, float Z,
        float RotX, float RotY, float RotZ, float Scale, bool IsSolid = false);

                                                                                          
                                                                                     
                                                                                     
                                                                                            
    private readonly List<(string ModelKey, Vector3 Position, Vector3 RotationDeg, float Scale, bool IsSolid)> _pendingSavedModels = new();

                                                                                          
                                                                                    
                                                                                
                                                                                                       
    public void SaveModelPlacements()
    {
        try
        {
            Directory.CreateDirectory(SaveDirectory);
            var list = _entities.Models.Where(m => m.Persist).Select(m => new SavedModelPlacement(
                m.ModelKey, m.Position.X, m.Position.Y, m.Position.Z,
                m.RotationDeg.X, m.RotationDeg.Y, m.RotationDeg.Z, m.Scale, m.IsSolid)).ToList();
            AtomicFile.WriteAllText(Path.Combine(SaveDirectory, "models.json"), JsonSerializer.Serialize(list));
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[Мир] Не удалось сохранить расставленные модели: {ex.Message}");
        }
    }

    private void LoadModelPlacements()
    {
        var path = Path.Combine(SaveDirectory, "models.json");
        if (!File.Exists(path)) return;
        try
        {
            var list = JsonSerializer.Deserialize<List<SavedModelPlacement>>(File.ReadAllText(path));
            if (list == null) return;
            foreach (var m in list)
            {
                                                                            
                                                                           
                                                                           
                _pendingSavedModels.Add((m.ModelKey, new Vector3(m.X, m.Y, m.Z),
                    new Vector3(m.RotX, m.RotY, m.RotZ), m.Scale, m.IsSolid));
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[Мир] Не удалось загрузить расставленные модели: {ex.Message}");
        }
    }

                                                                                         
                                                                                     
                                                                                      
    public List<(string ModelKey, Vector3 Position, Vector3 RotationDeg, float Scale, bool IsSolid)> ConsumeSavedModelPlacements()
    {
        var result = new List<(string ModelKey, Vector3 Position, Vector3 RotationDeg, float Scale, bool IsSolid)>(_pendingSavedModels);
        _pendingSavedModels.Clear();
        return result;
    }

    private record struct SavedItemDefinition(string Name, string IconPath);

                                                                                          
                                                                                     
                                                                                           
                                                                                     
                                                                                      
    public void SaveItemCatalog()
    {
        try
        {
            Directory.CreateDirectory(SaveDirectory);
            var list = _entities.Items.Select(item => new SavedItemDefinition(item.Name, item.IconPath)).ToList();
            AtomicFile.WriteAllText(Path.Combine(SaveDirectory, "items.json"), JsonSerializer.Serialize(list));
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[Мир] Не удалось сохранить каталог предметов: {ex.Message}");
        }
    }

    private void LoadItemCatalog()
    {
        var path = Path.Combine(SaveDirectory, "items.json");
        if (!File.Exists(path)) return;
        try
        {
            var list = JsonSerializer.Deserialize<List<SavedItemDefinition>>(File.ReadAllText(path));
            if (list == null) return;
            _entities.Items.Clear();
            foreach (var i in list)
                if (!string.IsNullOrWhiteSpace(i.Name)) _entities.Items.Add(new ItemDefinition(i.Name, i.IconPath));
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[Мир] Не удалось загрузить каталог предметов: {ex.Message}");
        }
    }

    public void SaveEdits()
    {
        try
        {
            Directory.CreateDirectory(SaveDirectory);
            foreach (var staleFile in Directory.GetFiles(SaveDirectory, "chunk_*.json"))
                File.Delete(staleFile);

            var byChunk = _savedEdits.GroupBy(e => ChunkCoordOf(e.Key.X, e.Key.Z));
            int chunkFileCount = 0;
            foreach (var group in byChunk)
            {
                var list = group.Select(e => new SavedBlockEdit(e.Key.X, e.Key.Y, e.Key.Z, e.Value, GetBlockShape(e.Key), GetLiquidLevel(e.Key))).ToList();
                AtomicFile.WriteAllText(ChunkSaveFilePath(group.Key), JsonSerializer.Serialize(list));
                chunkFileCount++;
            }

            Console.WriteLine($"[Мир] Сохранено правок: {_savedEdits.Count} (файлов чанков: {chunkFileCount}) -> {SaveDirectory}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[Мир] Не удалось сохранить мир: {ex.Message}");
        }
    }

    private void LoadEdits()
    {
        bool hasChunkSaves = Directory.Exists(SaveDirectory) && Directory.GetFiles(SaveDirectory, "chunk_*.json").Length > 0;
        if (!hasChunkSaves)
        {
            LoadLegacyFlatSaveIfPresent();
            return;
        }

        int totalLoaded = 0;
        foreach (var file in Directory.GetFiles(SaveDirectory, "chunk_*.json"))
        {
            try
            {
                var list = JsonSerializer.Deserialize<List<SavedBlockEdit>>(File.ReadAllText(file));
                if (list == null) continue;
                foreach (var e in list)
                {
                    var pos = new Vector3i(e.X, e.Y, e.Z);
                    SetBlockSilent(pos, e.Type);
                    if (e.Shape != BlockShape.Cube) SetBlockShape(pos, e.Shape);
                    if (e.Type == BlockType.Water && e.LiquidLevel > 0) _liquidLevel[pos] = e.LiquidLevel;
                    _savedEdits[pos] = e.Type;
                }
                totalLoaded += list.Count;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[Мир] Пропущен повреждённый файл чанка {Path.GetFileName(file)}: {ex.Message}");
            }
        }
        Console.WriteLine($"[Мир] Загружено сохранённых правок: {totalLoaded} (по чанкам, {SaveDirectory})");
    }

    private void LoadLegacyFlatSaveIfPresent()
    {
        if (!File.Exists(LegacySaveFilePath)) return;
        try
        {
            var list = JsonSerializer.Deserialize<List<SavedBlockEdit>>(File.ReadAllText(LegacySaveFilePath));
            if (list == null) return;
            foreach (var e in list)
            {
                var pos = new Vector3i(e.X, e.Y, e.Z);
                SetBlockSilent(pos, e.Type);
                if (e.Shape != BlockShape.Cube) SetBlockShape(pos, e.Shape);
                if (e.Type == BlockType.Water && e.LiquidLevel > 0) _liquidLevel[pos] = e.LiquidLevel;
                _savedEdits[pos] = e.Type;
            }
            Console.WriteLine($"[Мир] Мигрировано правок из старого world_edits.json: {list.Count}. Нажми O в игре, чтобы зафиксировать их в новом формате (по чанкам).");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[Мир] Не удалось прочитать старый world_edits.json: {ex.Message}");
        }
    }
}
