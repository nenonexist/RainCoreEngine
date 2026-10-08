using ImGuiNET;

namespace RainCore.EditorApp;

             
                                                                                                    
                                                                                              
   
                                                                                                      
                                                                                                        
                                                                                                        
                                                                                                       
                              
              
public sealed partial class EditorWindow
{
    private sealed class SceneTab
    {
        public string Name;
        public MeshRoomLevel? Level;
        public int NextEventNumber = 1;
        public EditorHistory History = new();
        public SceneTab(string name) { Name = name; }
    }

    private readonly List<SceneTab> _sceneTabs = new();
    private SceneTab? _activeTab;
    private SceneTab? _tabSelectRequest;
    private SceneTab? _tabPendingClose;
    private bool _openCloseTabPopup;
    private bool _openNewSceneFromTabs;

                    
    private bool _showScenesPanel;
    private string _managerSelectedScene = string.Empty;
    private enum ScenePromptKind { None, Duplicate, Rename }
    private ScenePromptKind _scenePrompt = ScenePromptKind.None;
    private bool _openScenePrompt;
    private string _scenePromptBuffer = string.Empty;
    private string _scenePromptTarget = string.Empty;
    private string _deleteSceneTarget = string.Empty;
    private bool _openDeleteScenePopup;

                                     

                                                                                                                             
    private void InitializeSceneTabsForProject()
    {
        var scenes = ProjectDescriptor.ListScenes(_projectPath);
        string? preferred = null;
        if (_currentProjectDescriptor != null && scenes.Contains(_currentProjectDescriptor.StartScene, StringComparer.OrdinalIgnoreCase))
            preferred = scenes.First(s => s.Equals(_currentProjectDescriptor.StartScene, StringComparison.OrdinalIgnoreCase));
        else if (scenes.Count > 0 && !scenes.Contains(_sceneName, StringComparer.OrdinalIgnoreCase))
            preferred = scenes[0];

        if (preferred != null && preferred != _sceneName)
        {
            _sceneName = preferred;
            _level = EditorSceneIO.LoadOrCreate(_projectPath, _sceneName, _modelCache, out _nextEventNumber);
        }

        _sceneTabs.Clear();
        var tab = new SceneTab(_sceneName) { Level = _level, NextEventNumber = _nextEventNumber };
        _sceneTabs.Add(tab);
        _activeTab = tab;
        _tabSelectRequest = tab;
        _history = tab.History;
        _managerSelectedScene = _sceneName;
        ClearSceneSelection();
        _selectionHistory.Clear();
        _selectionHistoryPos = -1;
        _lastTrackedSelection = null;
        HistoryReset();
        ResetViewportForNewScene();
    }

    private void StashActiveTab()
    {
        if (_activeTab == null) return;
        _activeTab.Level = _level;
        _activeTab.NextEventNumber = _nextEventNumber;
    }

    private void ActivateTab(SceneTab tab)
    {
        if (tab.Level == null)
        {
            tab.Level = EditorSceneIO.LoadOrCreate(_projectPath, tab.Name, _modelCache, out var next);
            tab.NextEventNumber = next;
            tab.History = new EditorHistory();
        }

        _activeTab = tab;
        _level = tab.Level;
        _nextEventNumber = tab.NextEventNumber;
        _sceneName = tab.Name;
        _history = tab.History;
        _managerSelectedScene = tab.Name;

        ClearSceneSelection();
        _selectedAsset = string.Empty;
        _commandUndo.Clear();
        _editSceneSnapshotJson = null;
        _selectionHistory.Clear();
        _selectionHistoryPos = -1;
        _lastTrackedSelection = null;

        if (!_history.HasBaseline) HistoryReset();
        else
        {
            _historyCounts = CountObjects();
            _historyEnv = _level.Environment;
            _historyWasInteracting = false;
        }

        _tabSelectRequest = tab;
        ResetViewportForNewScene();
    }

                                                                                              
    private void SwitchToScene(string sceneName)
    {
        if (_playMode) { _statusMessage = "Stop Play Mode before switching scenes."; return; }
        if (_activeTab != null && _activeTab.Name.Equals(sceneName, StringComparison.OrdinalIgnoreCase)) return;

        HistoryCommitNow();
        StashActiveTab();

        var tab = _sceneTabs.FirstOrDefault(t => t.Name.Equals(sceneName, StringComparison.OrdinalIgnoreCase));
        if (tab == null)
        {
            tab = new SceneTab(sceneName);
            _sceneTabs.Add(tab);
        }

        ActivateTab(tab);
        _statusMessage = $"Scene: {tab.Name}";
    }

                                                                                                                    
    private void AdoptCurrentSceneAsTab()
    {
        _sceneTabs.RemoveAll(t => t.Name.Equals(_sceneName, StringComparison.OrdinalIgnoreCase));
        var tab = new SceneTab(_sceneName) { Level = _level, NextEventNumber = _nextEventNumber };
        _sceneTabs.Add(tab);
        _activeTab = tab;
        _history = tab.History;
        _managerSelectedScene = tab.Name;
        ClearSceneSelection();
        _commandUndo.Clear();
        _selectionHistory.Clear();
        _selectionHistoryPos = -1;
        _lastTrackedSelection = null;
        HistoryReset();
        _tabSelectRequest = tab;
        ResetViewportForNewScene();
    }

    private void CycleScene(int direction)
    {
        if (_sceneTabs.Count < 2 || _activeTab == null) return;
        int index = _sceneTabs.IndexOf(_activeTab);
        int next = ((index + direction) % _sceneTabs.Count + _sceneTabs.Count) % _sceneTabs.Count;
        SwitchToScene(_sceneTabs[next].Name);
    }

    private bool IsTabDirty(SceneTab tab)
    {
        if (ReferenceEquals(tab, _activeTab)) return _history.IsDirty;
        return tab.History.IsDirty;
    }

    private void RequestCloseTab(SceneTab tab)
    {
        if (_playMode) return;
        if (ReferenceEquals(tab, _activeTab)) HistoryCommitNow();

        if (IsTabDirty(tab))
        {
            _tabPendingClose = tab;
            _openCloseTabPopup = true;
        }
        else CloseTabNow(tab);
    }

    private void CloseTabNow(SceneTab tab)
    {
        int index = _sceneTabs.IndexOf(tab);
        if (index < 0) return;

        bool wasActive = ReferenceEquals(tab, _activeTab);
        DeleteRecoveryBackup(tab.Name);
        _sceneTabs.RemoveAt(index);

        if (!wasActive) return;

        _activeTab = null;
        if (_sceneTabs.Count == 0)
        {
                                                                                                                 
            var scenes = ProjectDescriptor.ListScenes(_projectPath);
            var fallback = _currentProjectDescriptor?.StartScene is { Length: > 0 } start
                && !start.Equals(tab.Name, StringComparison.OrdinalIgnoreCase) ? start
                : scenes.FirstOrDefault(s => !s.Equals(tab.Name, StringComparison.OrdinalIgnoreCase)) ?? "Main";
            var created = new SceneTab(fallback);
            _sceneTabs.Add(created);
            ActivateTab(created);
        }
        else
        {
            ActivateTab(_sceneTabs[Math.Min(index, _sceneTabs.Count - 1)]);
        }
    }

    private void SaveTab(SceneTab tab)
    {
        if (ReferenceEquals(tab, _activeTab)) { SaveCurrentScene(); return; }
        if (tab.Level == null) return;
        EditorSceneIO.Save(_projectPath, tab.Name, tab.Level, tab.NextEventNumber);
        tab.History.MarkSaved();
        DeleteRecoveryBackup(tab.Name);
        _statusMessage = $"Saved: Scenes/{tab.Name}{SceneFileSuffix}";
    }

                                                                                       
    private void RenameActiveTab(string newName)
    {
        if (_activeTab != null) _activeTab.Name = newName;
        _managerSelectedScene = newName;
    }

                                                    

    private void PlayScene()
    {
        if (_playMode) return;
        StartPlayMode();
    }

    private void PlayProject()
    {
        if (_playMode) return;
        var start = _currentProjectDescriptor?.StartScene;
        if (!string.IsNullOrWhiteSpace(start)
            && (ProjectDescriptor.ListScenes(_projectPath).Contains(start, StringComparer.OrdinalIgnoreCase)
                || _sceneTabs.Any(t => t.Name.Equals(start, StringComparison.OrdinalIgnoreCase))))
            SwitchToScene(start);
        else
            _statusMessage = "Startup scene is not set or missing - playing the current scene. (Scenes panel -> Set as Startup)";
        StartPlayMode();
    }

    private void SetStartupScene(string sceneName)
    {
        if (_currentProjectDescriptor == null || string.IsNullOrWhiteSpace(sceneName)) return;
        var updated = _currentProjectDescriptor with { StartScene = sceneName };
        ProjectDescriptor.Save(_projectPath, updated);
        _currentProjectDescriptor = updated;
        _statusMessage = $"Startup scene: {sceneName} (Play Project and the built game start here)";
    }

    private bool IsStartupScene(string name) =>
        _currentProjectDescriptor != null && _currentProjectDescriptor.StartScene.Equals(name, StringComparison.OrdinalIgnoreCase);

    private string ScenePathOnDisk(string name) => Path.Combine(_projectPath, SceneFolderName, name + SceneFileSuffix);

    private List<string> AllSceneNames()
    {
        var names = new List<string>(ProjectDescriptor.ListScenes(_projectPath));
        foreach (var tab in _sceneTabs)
            if (!names.Contains(tab.Name, StringComparer.OrdinalIgnoreCase)) names.Add(tab.Name);
        names.Sort(StringComparer.OrdinalIgnoreCase);
        return names;
    }

                      

    private void DrawSceneTabs()
    {
        if (_sceneTabs.Count == 0) return;

        const ImGuiTabBarFlags barFlags = ImGuiTabBarFlags.FittingPolicyScroll | ImGuiTabBarFlags.Reorderable;
        if (!ImGui.BeginTabBar("SceneTabs", barFlags)) return;

        SceneTab? toClose = null;
        SceneTab? toSwitch = null;

                                                                            
        foreach (var tab in _sceneTabs.ToList())
        {
            bool dirty = IsTabDirty(tab);
            var label = $"{tab.Name}{(dirty ? " *" : "")}###scenetab_{tab.Name}";
            var flags = ReferenceEquals(tab, _tabSelectRequest) ? ImGuiTabItemFlags.SetSelected : ImGuiTabItemFlags.None;
            bool open = true;

            if (ImGui.BeginTabItem(label, ref open, flags))
            {
                if (!ReferenceEquals(tab, _activeTab) && _tabSelectRequest == null) toSwitch = tab;
                ImGui.EndTabItem();
            }

            Tip(dirty ? $"{tab.Name} - unsaved changes\nMiddle-click or X closes the tab." : $"{tab.Name}\nMiddle-click or X closes the tab.\nCtrl+Tab switches scenes.");
            if (!open) toClose = tab;
        }

        _tabSelectRequest = null;

        if (ImGui.TabItemButton("+", ImGuiTabItemFlags.Trailing | ImGuiTabItemFlags.NoTooltip))
            _openNewSceneFromTabs = true;
        Tip("New Scene");

        ImGui.EndTabBar();

        if (toClose != null) RequestCloseTab(toClose);
        else if (toSwitch != null && !_playMode) SwitchToScene(toSwitch.Name);

        if (_openNewSceneFromTabs)
        {
            _openNewSceneFromTabs = false;
            _newSceneNameBuffer = "NewScene";
            _requestNewScenePopup = true;
        }
    }

                                                                                                        
                                                                                                                
    private bool _requestNewScenePopup;

                                                                                                       
    private void DrawSceneModals()
    {
        if (_openCloseTabPopup) { ImGui.OpenPopup("Unsaved scene"); _openCloseTabPopup = false; }
        if (ImGui.BeginPopupModal("Unsaved scene", ImGuiWindowFlags.AlwaysAutoResize))
        {
            var tab = _tabPendingClose;
            if (tab == null) { ImGui.CloseCurrentPopup(); ImGui.EndPopup(); return; }

            ImGui.Text($"Scene \"{tab.Name}\" has unsaved changes.");
            ImGui.Text("Save before closing?");
            ImGui.Separator();
            if (ImGui.Button("Save"))
            {
                SaveTab(tab);
                CloseTabNow(tab);
                _tabPendingClose = null;
                ImGui.CloseCurrentPopup();
            }
            ImGui.SameLine();
            if (ImGui.Button("Don't Save"))
            {
                CloseTabNow(tab);
                _tabPendingClose = null;
                ImGui.CloseCurrentPopup();
            }
            ImGui.SameLine();
            if (ImGui.Button("Cancel"))
            {
                _tabPendingClose = null;
                ImGui.CloseCurrentPopup();
            }
            ImGui.EndPopup();
        }
    }

                            

    private void DrawScenesPanel()
    {
        if (!_showScenesPanel) return;

        ImGui.SetNextWindowSize(new System.Numerics.Vector2(300, 380), ImGuiCond.FirstUseEver);
        if (!ImGui.Begin("Scenes", ref _showScenesPanel)) { ImGui.End(); return; }

        if (ImGui.Button("+ New Scene")) { _newSceneNameBuffer = "NewScene"; _requestNewScenePopup = true; }
        Tip("Create an empty scene and open it in a new tab.");

        var names = AllSceneNames();
        var selected = _managerSelectedScene;
        bool hasSelection = selected.Length > 0 && names.Contains(selected, StringComparer.OrdinalIgnoreCase);
        bool selectedIsCurrent = hasSelection && selected.Equals(_sceneName, StringComparison.OrdinalIgnoreCase);
        bool onDisk = hasSelection && File.Exists(ScenePathOnDisk(selected));

        ImGui.SameLine();
        ImGui.BeginDisabled(!hasSelection || selectedIsCurrent || _playMode);
        if (ImGui.Button("Open")) SwitchToScene(selected);
        ImGui.EndDisabled();
        Tip("Open the selected scene in a tab (double-click also works).");

        ImGui.SameLine();
        ImGui.BeginDisabled(!selectedIsCurrent);
        if (ImGui.Button("Save")) SaveCurrentScene();
        ImGui.EndDisabled();

        ImGui.BeginDisabled(!hasSelection || _playMode);
        if (ImGui.Button("Duplicate")) { _scenePrompt = ScenePromptKind.Duplicate; _scenePromptTarget = selected; _scenePromptBuffer = selected + "_Copy"; _openScenePrompt = true; }
        ImGui.SameLine();
        if (ImGui.Button("Rename")) { _scenePrompt = ScenePromptKind.Rename; _scenePromptTarget = selected; _scenePromptBuffer = selected; _openScenePrompt = true; }
        ImGui.SameLine();
        if (ImGui.Button("Delete")) { _deleteSceneTarget = selected; _openDeleteScenePopup = true; }
        ImGui.EndDisabled();

        ImGui.BeginDisabled(!hasSelection || !onDisk && !selectedIsCurrent);
        if (ImGui.Button("Set as Startup")) SetStartupScene(selected);
        ImGui.EndDisabled();
        Tip("The scene that 'Play Project' and the built game start from.");

        ImGui.Separator();

        foreach (var name in names)
        {
            bool isCurrent = name.Equals(_sceneName, StringComparison.OrdinalIgnoreCase);
            var tab = _sceneTabs.FirstOrDefault(t => t.Name.Equals(name, StringComparison.OrdinalIgnoreCase));
            bool dirty = tab != null && IsTabDirty(tab);
            bool unsavedFile = !File.Exists(ScenePathOnDisk(name));
            var label = $"{(IsStartupScene(name) ? "* " : "  ")}{name}{(dirty ? " (modified)" : "")}{(unsavedFile ? " (not saved yet)" : "")}{(isCurrent ? "  <- current" : "")}";

            if (ImGui.Selectable(label + "##scn_" + name, name.Equals(selected, StringComparison.OrdinalIgnoreCase), ImGuiSelectableFlags.AllowDoubleClick))
            {
                _managerSelectedScene = name;
                if (ImGui.IsMouseDoubleClicked(ImGuiMouseButton.Left) && !isCurrent) SwitchToScene(name);
            }
        }

        if (names.Count == 0) ImGui.TextDisabled("No scenes yet - press '+ New Scene'.");
        ImGui.Spacing();
        ImGui.TextDisabled("* = startup scene");

                                                                        
        if (_openScenePrompt) { ImGui.OpenPopup("Scene name"); _openScenePrompt = false; }
        if (ImGui.BeginPopupModal("Scene name", ImGuiWindowFlags.AlwaysAutoResize))
        {
            ImGui.Text(_scenePrompt == ScenePromptKind.Rename ? $"Rename \"{_scenePromptTarget}\" to:" : $"Duplicate \"{_scenePromptTarget}\" as:");
            ImGui.InputText("##scenePromptName", ref _scenePromptBuffer, 64);
            var clean = SanitizeSceneName(_scenePromptBuffer);
            bool exists = clean.Length > 0 && AllSceneNames().Contains(clean, StringComparer.OrdinalIgnoreCase);
            if (clean.Length == 0) ImGui.TextColored(new System.Numerics.Vector4(1f, 0.5f, 0.4f, 1f), "Enter a valid name.");
            else if (exists) ImGui.TextColored(new System.Numerics.Vector4(1f, 0.5f, 0.4f, 1f), "A scene with this name already exists.");

            ImGui.BeginDisabled(clean.Length == 0 || exists);
            if (ImGui.Button("OK"))
            {
                if (_scenePrompt == ScenePromptKind.Rename) RenameScene(_scenePromptTarget, clean);
                else DuplicateScene(_scenePromptTarget, clean);
                _scenePrompt = ScenePromptKind.None;
                ImGui.CloseCurrentPopup();
            }
            ImGui.EndDisabled();
            ImGui.SameLine();
            if (ImGui.Button("Cancel")) { _scenePrompt = ScenePromptKind.None; ImGui.CloseCurrentPopup(); }
            ImGui.EndPopup();
        }

        if (_openDeleteScenePopup) { ImGui.OpenPopup("Delete scene?"); _openDeleteScenePopup = false; }
        if (ImGui.BeginPopupModal("Delete scene?", ImGuiWindowFlags.AlwaysAutoResize))
        {
            ImGui.Text($"Delete scene \"{_deleteSceneTarget}\"?");
            ImGui.TextDisabled("The file is removed from disk and cannot be undone.");
            if (ImGui.Button("Delete")) { DeleteScene(_deleteSceneTarget); ImGui.CloseCurrentPopup(); }
            ImGui.SameLine();
            if (ImGui.Button("Cancel")) ImGui.CloseCurrentPopup();
            ImGui.EndPopup();
        }

        ImGui.End();
    }

    private void DuplicateScene(string source, string newName)
    {
        try
        {
            var tab = _sceneTabs.FirstOrDefault(t => t.Name.Equals(source, StringComparison.OrdinalIgnoreCase));
            if (tab != null)
            {
                if (ReferenceEquals(tab, _activeTab)) { StashActiveTab(); }
                if (tab.Level != null)
                {
                    EditorSceneIO.Save(_projectPath, newName, tab.Level, tab.NextEventNumber);
                    _statusMessage = $"Duplicated \"{source}\" -> \"{newName}\"";
                    _managerSelectedScene = newName;
                    return;
                }
            }

            File.Copy(ScenePathOnDisk(source), ScenePathOnDisk(newName), overwrite: false);
            _statusMessage = $"Duplicated \"{source}\" -> \"{newName}\"";
            _managerSelectedScene = newName;
        }
        catch (Exception ex)
        {
            _statusMessage = $"Could not duplicate scene: {ex.Message}";
        }
    }

    private void RenameScene(string oldName, string newName)
    {
        try
        {
            var oldPath = ScenePathOnDisk(oldName);
            if (File.Exists(oldPath)) File.Move(oldPath, ScenePathOnDisk(newName));

            var tab = _sceneTabs.FirstOrDefault(t => t.Name.Equals(oldName, StringComparison.OrdinalIgnoreCase));
            if (tab != null) tab.Name = newName;
            if (_sceneName.Equals(oldName, StringComparison.OrdinalIgnoreCase)) _sceneName = newName;
            if (IsStartupScene(oldName)) SetStartupScene(newName);
            _managerSelectedScene = newName;
            _statusMessage = $"Renamed \"{oldName}\" -> \"{newName}\"";
        }
        catch (Exception ex)
        {
            _statusMessage = $"Could not rename scene: {ex.Message}";
        }
    }

    private void DeleteScene(string name)
    {
        try
        {
            var tab = _sceneTabs.FirstOrDefault(t => t.Name.Equals(name, StringComparison.OrdinalIgnoreCase));
            var path = ScenePathOnDisk(name);
            if (File.Exists(path)) File.Delete(path);
            if (tab != null) CloseTabNow(tab);
            if (_managerSelectedScene.Equals(name, StringComparison.OrdinalIgnoreCase)) _managerSelectedScene = _sceneName;
            _statusMessage = $"Deleted scene \"{name}\"";
        }
        catch (Exception ex)
        {
            _statusMessage = $"Could not delete scene: {ex.Message}";
        }
    }
}
