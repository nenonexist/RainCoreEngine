using ImGuiNET;

namespace RainCore.EditorApp;

             
                                                                                                                 
                                                                                                                         
                                                                                                                   
                                                                                                               
              
public sealed partial class EditorWindow
{
    private const float RecoveryIntervalSeconds = 20f;
    private double _recoveryElapsed;
    private readonly List<string> _recoveryPending = new();
    private bool _openRecoveryPopup;

    private string RecoveryDir => Path.Combine(_projectPath, ".recovery");
    private string RecoveryPath(string scene) => Path.Combine(RecoveryDir, scene + SceneFileSuffix);

    private void DeleteRecoveryBackup(string scene)
    {
        try { var p = RecoveryPath(scene); if (File.Exists(p)) File.Delete(p); } catch {                   }
    }

    private void RecoveryTick()
    {
        if (!_projectLoaded || _playMode) return;

        _recoveryElapsed += _lastFrameDelta;
        if (_recoveryElapsed < RecoveryIntervalSeconds) return;
        _recoveryElapsed = 0;

        try
        {
            HistoryCommitNow();
            foreach (var tab in _sceneTabs)
            {
                bool active = ReferenceEquals(tab, _activeTab);
                var history = active ? _history : tab.History;
                if (!history.IsDirty) continue;

                Directory.CreateDirectory(RecoveryDir);
                var json = active ? history.Current
                    : tab.Level != null ? EditorSceneIO.SerializeToJson(tab.Level, tab.Name, tab.NextEventNumber) : null;
                if (json != null) File.WriteAllText(RecoveryPath(tab.Name), json);
            }
        }
        catch {                                           }
    }

                                                                                           
    private void ScanRecovery()
    {
        _recoveryPending.Clear();
        try
        {
            if (!Directory.Exists(RecoveryDir)) return;
            foreach (var file in Directory.EnumerateFiles(RecoveryDir, "*" + SceneFileSuffix))
            {
                var scene = Path.GetFileName(file)[..^SceneFileSuffix.Length];
                var real = ScenePathOnDisk(scene);
                if (!File.Exists(real) || File.GetLastWriteTimeUtc(file) > File.GetLastWriteTimeUtc(real))
                    _recoveryPending.Add(scene);
                else
                    File.Delete(file);                                        
            }
        }
        catch { }

        _openRecoveryPopup = _recoveryPending.Count > 0;
    }

    private void RestoreRecovery()
    {
        string? first = null;
        foreach (var scene in _recoveryPending.ToList())
        {
            try
            {
                var backupJson = File.ReadAllText(RecoveryPath(scene));
                var level = EditorSceneIO.DeserializeFromJson(backupJson, scene, _projectPath, _modelCache, out var next);

                var tab = _sceneTabs.FirstOrDefault(t => t.Name.Equals(scene, StringComparison.OrdinalIgnoreCase));
                if (tab == null) { tab = new SceneTab(scene); _sceneTabs.Add(tab); }
                else if (ReferenceEquals(tab, _activeTab)) StashActiveTab();

                                                                                                                 
                var diskJson = File.Exists(ScenePathOnDisk(scene))
                    ? EditorSceneIO.SerializeToJson(
                        EditorSceneIO.LoadOrCreate(_projectPath, scene, _modelCache, out var diskNext), scene, diskNext)
                    : EditorSceneIO.SerializeToJson(new MeshRoomLevel(scene), scene, 1);

                tab.Level = level;
                tab.NextEventNumber = next;
                tab.History = new EditorHistory();
                tab.History.Reset(diskJson);
                tab.History.Commit(backupJson, "Recovered");
                first ??= scene;
            }
            catch (Exception ex)
            {
                _statusMessage = $"Could not recover \"{scene}\": {ex.Message}";
            }
        }

        _recoveryPending.Clear();
        if (first != null)
        {
            var target = _sceneTabs.First(t => t.Name.Equals(first, StringComparison.OrdinalIgnoreCase));
            if (ReferenceEquals(target, _activeTab))
            {
                                                             
                _activeTab = null;
            }
            ActivateTab(target);
            _statusMessage = "Recovered unsaved work. Review it and press Ctrl+S to keep it.";
        }
    }

    private void DrawRecoveryPopup()
    {
        if (_openRecoveryPopup) { ImGui.OpenPopup("Recover unsaved work?"); _openRecoveryPopup = false; }
        if (!ImGui.BeginPopupModal("Recover unsaved work?", ImGuiWindowFlags.AlwaysAutoResize)) return;

        ImGui.TextWrapped("The editor was closed with unsaved changes (or crashed). A backup is available for:");
        foreach (var scene in _recoveryPending) ImGui.BulletText(scene);
        ImGui.Spacing();
        ImGui.TextDisabled("Recovered scenes open with a * - nothing is overwritten until you save.");
        ImGui.Separator();

        if (ImGui.Button("Recover"))
        {
            RestoreRecovery();
            ImGui.CloseCurrentPopup();
        }
        ImGui.SameLine();
        if (ImGui.Button("Discard backups"))
        {
            foreach (var scene in _recoveryPending) DeleteRecoveryBackup(scene);
            _recoveryPending.Clear();
            ImGui.CloseCurrentPopup();
        }
        ImGui.EndPopup();
    }
}
