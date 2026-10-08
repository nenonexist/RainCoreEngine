using ImGuiNET;
using System.Windows.Forms;

namespace RainCore.EditorApp;

             
                                                                              
                                                                           
                               
              
public sealed partial class EditorWindow
{
    private const string EditorVersion = "0.11.0-dev";

    private EditorSettings _settings = EditorSettings.Load();
    private bool _showSettingsWindow;
    private bool _showAboutWindow;
    private bool _showBuildWindow;

    private bool _showSaveSceneAsPopup;
    private string _saveSceneAsNameBuffer = string.Empty;

    private bool _showSaveProjectAsPopup;
    private string _saveProjectAsNameBuffer = string.Empty;
    private string _saveProjectAsParentDir = string.Empty;

    private double _autosaveElapsed;

    private void DrawTopBar()
    {
        if (ImGui.BeginMainMenuBar())
        {
            DrawFileMenu();
            DrawSceneMenu();
            DrawProjectMenu();
            DrawEditMenu();
            DrawWindowMenu();

            if (ImGui.MenuItem("Settings")) _showSettingsWindow = true;
            if (ImGui.MenuItem("About")) _showAboutWindow = true;
            if (ImGui.MenuItem("Build...")) _showBuildWindow = true;

            ImGui.Separator();

            if (_playMode)
            {
                ImGui.PushStyleColor(ImGuiCol.Button, new System.Numerics.Vector4(0.7f, 0.2f, 0.18f, 1f));
                if (ImGui.Button("Stop")) StopPlayMode();
                ImGui.PopStyleColor();
                Tip("Stop Play Mode and restore the scene to how it was before Play.\nShortcut: Esc");
                ImGui.SameLine();
                ImGui.Checkbox("Keep changes", ref _keepPlayChangesOnStop);
                Tip("Tick it to KEEP what happened in the scene during Play when you press Stop (for example objects you moved with scripts).\nOtherwise the scene is restored. Undo (Ctrl+Z) can take it back.");
                ImGui.SameLine();
                ImGui.TextColored(new System.Numerics.Vector4(1f, 0.45f, 0.4f, 1f), "PLAY MODE");
                ImGui.SameLine();
                ImGui.TextDisabled("(WASD move, RMB look, F interact, Esc stop - changes are discarded on Stop)");
            }
            else
            {
                if (ImGui.Button("Play Scene")) PlayScene();
                Tip("Play the current scene from its Player Start.");
                ImGui.SameLine();
                if (ImGui.Button("Play From Here")) PlayFromHere();
                Tip("Start playing exactly where the Scene View camera is looking - great for testing one corner of a big scene.");
                ImGui.SameLine();
                if (ImGui.Button("Play Project")) PlayProject();
                Tip("Play the project from its startup scene (set in the Scenes panel).");
                ImGui.SameLine();
                ImGui.TextDisabled("Edit Mode");

                int problems = DiagnosticsWarningCount;
                if (problems > 0)
                {
                    ImGui.SameLine();
                    ImGui.PushStyleColor(ImGuiCol.Text, new System.Numerics.Vector4(1f, 0.8f, 0.3f, 1f));
                    if (ImGui.SmallButton($"! {problems}")) _showDiagnosticsWindow = true;
                    ImGui.PopStyleColor();
                    Tip($"{problems} problem(s) in this scene. Click to see them and fix them.");
                }
            }

            if (!string.IsNullOrEmpty(_statusMessage))
            {
                ImGui.SameLine();
                ImGui.TextDisabled("|  " + _statusMessage);
            }

            ImGui.EndMainMenuBar();
        }

        DrawSaveSceneAsPopup();
        DrawSaveProjectAsPopup();
        DrawSettingsWindow();
        DrawAboutWindow();
        DrawBuildWindow();
        DrawDatabaseWindow();
        DrawViewportCreateMenu();

        MaybeAutosave();
    }

    private void DrawSceneMenu()
    {
        if (!ImGui.BeginMenu("Scene")) return;

        if (ImGui.MenuItem("New Scene...", null, false, !_playMode)) { _newSceneNameBuffer = "NewScene"; _requestNewScenePopup = true; }
        if (ImGui.MenuItem("Save Scene", "Ctrl+S", false, !_playMode)) SaveCurrentScene();
        if (ImGui.MenuItem("Scenes Panel...")) _showScenesPanel = true;
        if (ImGui.MenuItem("Next Scene", "Ctrl+Tab", false, _sceneTabs.Count > 1)) CycleScene(+1);
        if (ImGui.MenuItem("Previous Scene", "Ctrl+Shift+Tab", false, _sceneTabs.Count > 1)) CycleScene(-1);

        ImGui.Separator();
        if (ImGui.MenuItem("Environment...")) _showEnvironmentWindow = true;
        if (ImGui.MenuItem("Diagnostics...")) _showDiagnosticsWindow = true;

        ImGui.Separator();
        if (ImGui.BeginMenu("Create", !_playMode))
        {
            var (origin, direction) = CameraRay();
            DrawCreateMenuItems(origin, direction);
            ImGui.EndMenu();
        }

        ImGui.Separator();
        if (ImGui.MenuItem("Play Scene", null, false, !_playMode)) PlayScene();
        if (ImGui.MenuItem("Play From Here", null, false, !_playMode)) PlayFromHere();
        if (ImGui.MenuItem("Play Project", null, false, !_playMode)) PlayProject();

        ImGui.EndMenu();
    }

    private void DrawWindowMenu()
    {
        if (!ImGui.BeginMenu("Window")) return;

        ImGui.MenuItem("Scenes", null, ref _showScenesPanel);
        ImGui.MenuItem("Environment", null, ref _showEnvironmentWindow);
        ImGui.MenuItem("Diagnostics", null, ref _showDiagnosticsWindow);
        ImGui.MenuItem("Console", null, ref _showConsoleWindow);
        ImGui.MenuItem("History", null, ref _showHistoryWindow);
        ImGui.MenuItem("Script Editor", null, ref _showScriptEditor);
        ImGui.MenuItem("Shortcuts", "F1", ref _showShortcutsWindow);

        ImGui.Separator();
        if (ImGui.BeginMenu("Layout"))
        {
            if (ImGui.MenuItem("Save Layout")) { _imgui?.SaveLayout(); _statusMessage = "Layout saved to editor_layout.ini in the project."; }
            if (ImGui.MenuItem("Reset to Default"))
            {
                ResetLayoutToDefault();
            }
            ImGui.EndMenu();
        }

        ImGui.EndMenu();
    }

                                                                                                          
                                                                                                    
                                                                                 
    private void ResetLayoutToDefault()
    {
        try
        {
            var ini = Path.Combine(_projectPath, "editor_layout.ini");
            if (File.Exists(ini)) File.Delete(ini);
            ImGui.LoadIniSettingsFromMemory(string.Empty);
            _statusMessage = "Layout reset. Restart the editor to see the default panel arrangement.";
        }
        catch (Exception ex) { _statusMessage = $"Could not reset layout: {ex.Message}"; }
    }

    private void DrawFileMenu()
    {
        if (!ImGui.BeginMenu("File")) return;

                                                                         
                                                                             
                                                                      
        if (ImGui.MenuItem("New Project...")) _projectLoaded = false;
        if (ImGui.MenuItem("Open Project...")) _projectLoaded = false;

        if (ImGui.BeginMenu("Recent Projects"))
        {
            var recent = RecentProjectsStore.Load();
            if (recent.Count == 0) ImGui.MenuItem("(empty)", enabled: false);
            foreach (var path in recent)
                if (ImGui.MenuItem(path)) OpenProject(path, isNew: false);
            ImGui.EndMenu();
        }

        ImGui.Separator();

                                                                     
                                                                             
                                                                             
                                                                            
                                                                   
        if (ImGui.MenuItem("New Scene..."))
        {
            _newSceneNameBuffer = "NewScene";
            ImGui.OpenPopup("NewSceneNamePopup");
        }
        if (ImGui.MenuItem("Save Scene")) SaveCurrentScene();
        if (ImGui.MenuItem("Save Scene As..."))
        {
            _saveSceneAsNameBuffer = _sceneName;
            _showSaveSceneAsPopup = true;
        }
        if (ImGui.MenuItem("Save Project As..."))
        {
            _saveProjectAsNameBuffer = Path.GetFileName(_projectPath.TrimEnd(Path.DirectorySeparatorChar));
            _saveProjectAsParentDir = Path.GetDirectoryName(_projectPath) ?? string.Empty;
            _showSaveProjectAsPopup = true;
        }

        ImGui.Separator();
        if (ImGui.MenuItem("Exit")) Close();

        ImGui.EndMenu();
    }

    private void DrawProjectMenu()
    {
        if (!ImGui.BeginMenu("Project")) return;

        ImGui.MenuItem($"Path: {_projectPath}", enabled: false);
        ImGui.MenuItem($"Scene: {_sceneName}", enabled: false);

                                                                                
                                                                   
        var name = _currentProjectDescriptor?.Name ?? Path.GetFileName(_projectPath.TrimEnd(Path.DirectorySeparatorChar));
        var nameBuffer = name;
        ImGui.SetNextItemWidth(200);
        if (ImGui.InputText("Project name", ref nameBuffer, 128) && !string.IsNullOrWhiteSpace(nameBuffer))
        {
            var updated = _currentProjectDescriptor! with { Name = nameBuffer };
            ProjectDescriptor.Save(_projectPath, updated);
            _currentProjectDescriptor = updated;
        }

        ImGui.Separator();
                                                                               
                                                                               
                                                                           
                                                                             
                                         
        if (ImGui.MenuItem("Database...")) _showDatabaseWindow = true;

        ImGui.EndMenu();
    }

                                                                                
                                                                  
                                                                                
                                                             
    private void SaveCurrentScene()
    {
        HistoryCommitNow();
        EditorSceneIO.Save(_projectPath, _sceneName, _level, _nextEventNumber);
        _history.MarkSaved();
        DeleteRecoveryBackup(_sceneName);
        _diagnosticsAge = 999;
        _statusMessage = $"Saved: Scenes/{_sceneName}.roomscene.json " +
            $"({_level.Models.Count} props, {_level.Interactables.Count} events, {_level.Triggers.Count} triggers)";
    }

    private void MaybeAutosave()
    {
        if (!_projectLoaded || _playMode || _settings.AutosaveIntervalSeconds <= 0) return;

        _autosaveElapsed += _lastFrameDelta;
        if (_autosaveElapsed < _settings.AutosaveIntervalSeconds) return;

        _autosaveElapsed = 0;
        SaveCurrentScene();
        _statusMessage = "Autosaved " + _statusMessage;
    }

    private void DrawSaveSceneAsPopup()
    {
        if (_showSaveSceneAsPopup) { ImGui.OpenPopup("Save Scene As"); _showSaveSceneAsPopup = false; }

        if (ImGui.BeginPopupModal("Save Scene As", ImGuiWindowFlags.AlwaysAutoResize))
        {
            ImGui.SetNextItemWidth(250);
            ImGui.InputText("Scene name", ref _saveSceneAsNameBuffer, 128);

            if (ImGui.Button("Save") && !string.IsNullOrWhiteSpace(_saveSceneAsNameBuffer))
            {
                _sceneName = SanitizeSceneName(_saveSceneAsNameBuffer);
                RenameActiveTab(_sceneName);
                SaveCurrentScene();
                ImGui.CloseCurrentPopup();
            }
            ImGui.SameLine();
            if (ImGui.Button("Cancel")) ImGui.CloseCurrentPopup();

            ImGui.EndPopup();
        }
    }

                                                                            
                                                                               
                                                                         
                                                                
                                    
    private void DrawSaveProjectAsPopup()
    {
        if (_showSaveProjectAsPopup) { ImGui.OpenPopup("Save Project As"); _showSaveProjectAsPopup = false; }

        if (ImGui.BeginPopupModal("Save Project As", ImGuiWindowFlags.AlwaysAutoResize))
        {
            ImGui.SetNextItemWidth(300);
            ImGui.InputText("New project name", ref _saveProjectAsNameBuffer, 128);

            ImGui.SetNextItemWidth(400);
            ImGui.InputText("Parent folder", ref _saveProjectAsParentDir, 512);
            ImGui.SameLine();
            if (ImGui.Button("Browse..."))
            {
                using var dialog = new FolderBrowserDialog { Description = "Choose a folder for the project copy" };
                if (dialog.ShowDialog() == DialogResult.OK)
                    _saveProjectAsParentDir = dialog.SelectedPath;
            }

            if (ImGui.Button("Save"))
            {
                var destination = Path.Combine(_saveProjectAsParentDir, _saveProjectAsNameBuffer);
                if (!string.IsNullOrWhiteSpace(_saveProjectAsNameBuffer) && Directory.Exists(_saveProjectAsParentDir)
                    && !Directory.Exists(destination))
                {
                    SaveCurrentScene();                                                                  
                    CopyDirectoryRecursive(_projectPath, destination);
                    var descriptor = ProjectDescriptor.TryLoad(destination);
                    if (descriptor != null) ProjectDescriptor.Save(destination, descriptor with { Name = _saveProjectAsNameBuffer });
                    OpenProject(destination, isNew: false);
                    ImGui.CloseCurrentPopup();
                }
            }
            ImGui.SameLine();
            if (ImGui.Button("Cancel")) ImGui.CloseCurrentPopup();

            ImGui.EndPopup();
        }
    }

    private void DrawSettingsWindow()
    {
        if (!_showSettingsWindow) return;

        ImGui.Begin("Settings", ref _showSettingsWindow, ImGuiWindowFlags.AlwaysAutoResize);

        ImGui.TextDisabled("Editor settings - not tied to any one project.");
        ImGui.Dummy(new System.Numerics.Vector2(0, 8));

        ImGui.SetNextItemWidth(400);
        var defaultFolder = _settings.DefaultNewProjectParentFolder;
        if (ImGui.InputText("Default new-project folder", ref defaultFolder, 512))
            _settings.DefaultNewProjectParentFolder = defaultFolder;
        ImGui.SameLine();
        if (ImGui.Button("Browse..."))
        {
            using var dialog = new FolderBrowserDialog { Description = "Default parent folder for New Project" };
            if (dialog.ShowDialog() == DialogResult.OK)
                _settings.DefaultNewProjectParentFolder = dialog.SelectedPath;
        }

        var sensitivity = _settings.CameraMouseSensitivity;
        if (ImGui.SliderFloat("Camera mouse sensitivity", ref sensitivity, 0.02f, 0.5f))
        {
            _settings.CameraMouseSensitivity = sensitivity;
            if (_flyCamera != null) _flyCamera.MouseSensitivity = sensitivity;
        }

        var autosave = _settings.AutosaveIntervalSeconds;
        if (ImGui.SliderInt("Autosave interval (s, 0 = off)", ref autosave, 0, 600))
            _settings.AutosaveIntervalSeconds = autosave;

        ImGui.Dummy(new System.Numerics.Vector2(0, 8));
        if (ImGui.Button("Save Settings")) _settings.Save();

        ImGui.End();
    }

    private void DrawAboutWindow()
    {
        if (!_showAboutWindow) return;

        ImGui.Begin("About", ref _showAboutWindow, ImGuiWindowFlags.AlwaysAutoResize);
        ImGui.TextUnformatted("RainCore Editor");
        ImGui.TextDisabled($"Version {EditorVersion}");
        ImGui.End();
    }
}
