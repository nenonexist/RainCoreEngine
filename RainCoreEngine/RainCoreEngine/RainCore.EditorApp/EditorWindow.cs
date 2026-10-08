using System.Globalization;
using ImGuiNET;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;
using OpenTK.Windowing.GraphicsLibraryFramework;

namespace RainCore.EditorApp;

             
                                                                                   
                                                                                 
                                                                               
                                                      
   
                                                                             
                                                                      
                                                                               
                                                                      
                                                                           
                                                                         
   
                                                            
                                                                            
                                                                          
                                                                            
                                                                       
                                                                      
                                     
                                                                        
                                                                           
                                                                             
                                                                 
                                                                        
                                                                               
                            
                                                                               
                                                                       
                                                                    
                                                                 
   
                                                                                 
                                                                             
                                                              
                                                             
                                                                    
                                                                               
                                                                     
                                                                          
                                                                           
                                                                           
                                                
   
                                                                           
                                                                              
                                                                     
                                                                     
                                                                    
                                                                        
                    
   
                                                                      
                                                                          
                                                                     
                                                                          
                                                                      
   
                                                                      
                                                                          
                                                                     
                                                                           
                                                                    
                                                                           
                                                               
   
                                                                  
                                                                        
                                                                            
                                                                           
   
                                                                             
                               
              
public sealed partial class EditorWindow : GameWindow
{
    private static readonly string[] ProjectFolders =
    {
        "Scenes", "Scripts", "Models", "Images", "Videos", "Sound", "Objects", "Prefabs"
    };

                                                                               
                                                                             
                                                                            
                                                                            
                                                                              
                                                                   
                                                                          
                                                                            
                                   
    private const string SceneFolderName = "Scenes";
    private const string SceneFileSuffix = ".roomscene.json";

                                                                               
                                                                              
                                                        
    private static readonly (string Key, string Label)[] Primitives =
    {
        (EditorSceneIO.PrimitivePrefix + "floor", "Floor"),
        (EditorSceneIO.PrimitivePrefix + "wall", "Wall"),
        (EditorSceneIO.PrimitivePrefix + "ceiling", "Ceiling"),
    };

    private string _projectPath = string.Empty;
    private ProjectDescriptor? _currentProjectDescriptor;
    private ScriptHost _scripts = null!;

                                                                              
                                                                              
                                                                             
                                                                              
                                                                         
                                                                             
                                                     
    private string _selectedProjectFolder = "Primitives";
    private string _projectFilter = string.Empty;
    private ImGuiController? _imgui;

                                                                                 
                                                                             
                                                                             
                                                                             
                                                                            
                                                                              
                                   
    private bool _projectLoaded;

    private bool _playMode;
    private string _selectedAsset = string.Empty;

    private SceneFramebuffer? _sceneFramebuffer;
    private EditorViewportRenderer? _viewportRenderer;
    private EditorFlyCamera? _flyCamera;
    private double _lastFrameDelta = 1.0 / 60.0;

                                                                               
                                                                            
                                     
    private MeshRoomLevel _level = null!;
    private readonly Dictionary<string, GlbModel> _modelCache = new();
    private Shader? _modelShader;
    private string _sceneName = "Untitled";
    private ModelInstance? _selectedModel;

                                                                      
                                                                    
                                                                         
                                                              
                                   
    private bool _isDraggingSelection;
    private bool _isDraggingPlayerStart;
    private Vector2 _dragMouseStart;

                                                                                   
                                                                             
                                                                               
                                                                            
                                                                             
                                                                           
                                                                          
                                                                   
    private GizmoAxis? _gizmoHoverAxis;
    private GizmoAxis? _gizmoDragAxis;
    private Vector3 _gizmoDragAxisWorldStart;

                                                                                 
                                                                               
                                                                                
                                                                                 
                                                                           
                                                                            
                                                                 
    private Vector3 _gizmoDragEntityStart;

                                                                                
                                                                           
                                                                             
                                         
    private GizmoMode _gizmoDragMode;

                                                                             
                                                                             
                                                                   
                                                                       
                                                                           
    private float _gizmoDragStartScreenAngle;
    private Vector3 _gizmoDragStartRotationDeg;

                                                                               
                                                                       
                                                                              
                                                                                 
    private float _gizmoDragStartScale;
    private float _gizmoDragStartAxisLength;

    private string _newSceneNameBuffer = "NewScene";
    private string _statusMessage = string.Empty;

                                                                  
                                                                         
                                                                             
                                                                
                                                                        
                                                                      
                                                                            
                                                                           
                                                                                  
    private TriggerZone? _selectedTrigger;
    private List<EventCommand> _triggerCommandsBuffer = new();

                                                                                
                                                                                
                                                                                
                                                                                 
                                                                                        
    private sealed class TriggerPageBuffer
    {
        public string RequiredFlag = string.Empty;
        public bool RequiredFlagValue = true;
        public List<EventCommand> Commands = new();
    }

    private readonly List<TriggerPageBuffer> _triggerPagesBuffer = new();

                                                                       
                                                                                 
                                                                                     
                                                     
    private int _activeTriggerPage = -1;

                                                                         
                                                                           
                                                                            
                                                                          
                                                                      
    private readonly Stack<List<EventCommand>> _commandUndo = new();
    private TriggerType _triggerType = TriggerType.OnInteract;
    private float _triggerRadius = 2f;
    private bool _triggerRunOnce = true;
    private string _triggerRequiredFlag = string.Empty;

                                                                             
                                                                    
                                                                            
                                                                      
                                                                 
    private string _triggerMarkerPath = string.Empty;
    private float _triggerMarkerScale = 1f;
    private MovementPattern _triggerMovement = MovementPattern.Static;
    private List<Vector3> _triggerPatrolPointsBuffer = new();
    private bool _triggerRequiredFlagValue = true;
    private float _triggerChance = 1f;
    private float _triggerInterval = 5f;

                                                                               
                                                                              
                                                                       
                                                                               
                                                                           
                                                                            
                                                                              
                                                                   
                                                               
    private string _pendingCommandCategory = string.Empty;
    private string _pendingCommandKind = string.Empty;
    private string[] _pendingCommandArgs = Array.Empty<string>();
    private List<string> _pendingVariadicArgs = new();

                                                                                 
                                                                                 
                                                                                   
                                                                                
                                                                           
    private PosterInstance? _selectedPoster;
    private float _posterScaleBuffer = 1f;

                                                                                      
                                                                                           
    private MoodZone? _selectedMoodZone;
    private float _moodZoneRadiusBuffer = 5f;
    private float _moodZoneTargetValueBuffer = 50f;
    private float _moodZonePullRateBuffer = 5f;
    private float _moodZoneReleaseSecondsBuffer = 3f;

                                                                           
                                                                     
    private MoodEvent? _selectedMoodEvent;
    private float _moodEventRadiusBuffer = 8f;
    private MoodEventEffect _moodEventEffectBuffer = MoodEventEffect.Ghost;
    private float _moodEventCooldownBuffer = 120f;
    private float _moodEventChanceBuffer = 0.3f;
    private string _moodEventSoundBuffer = string.Empty;

                                                                           
                                                                            
                                                                     
                              
    private Vector2 _viewportOrigin;
    private Vector2 _viewportSize;

                                                                    
                                                                         
                                                                            
                                                                             
                                                                            
                                                                      
                                                                       
                                                                      
                                                                          
                                                                       
                                                                        
                                                                  
                                                                         
                                                                         
                                                                       
                         
    private Camera? _playCamera;
    private readonly ICameraRig _playCameraRig = new ThirdPersonOrbitCameraRig();
    private Vector3 _playerPosition;
    private readonly EventRunner _playEventRunner = new();
    private EditorPlayEventContext? _playEventContext;
    private Vector2 _playLastMouse;
    private bool _playFirstMouse = true;
    private string? _editSceneSnapshotJson;
    private readonly List<string> _playLog = new();
    private DialogueState? _dialogueState;

                                                                              
                                                                           
                                                                               
                                                                          
                                                                     
                                                                    
    private GameDatabase _database = new();
    private readonly PartyState _party = new();
    private BattleState? _battle;
    private bool _showDatabaseWindow;

    private readonly string? _initialProjectPath;

    public EditorWindow(string? initialProjectPath)
        : base(GameWindowSettings.Default, new NativeWindowSettings
        {
            ClientSize = new Vector2i(1600, 900),
            Title = "RainCore Editor",
            APIVersion = new Version(3, 3),
            Flags = ContextFlags.ForwardCompatible,
            WindowBorder = WindowBorder.Resizable,
        })
    {
        _initialProjectPath = initialProjectPath;
    }

    protected override void OnLoad()
    {
        base.OnLoad();

        GL.ClearColor(0.10f, 0.10f, 0.12f, 1f);

                                                                              
                                                                
                                                                              
                                                                           
                                                                            
                                                              
                                                                              
                                                                           
        var launcherIniPath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "RainCore.Editor", "launcher_layout.ini");
        Directory.CreateDirectory(Path.GetDirectoryName(launcherIniPath)!);
        _imgui = new ImGuiController(this, ClientSize.X, ClientSize.Y, launcherIniPath);

        var shadersDir = Path.Combine(AppContext.BaseDirectory, "Shaders");
        _sceneFramebuffer = new SceneFramebuffer(1280, 720);
        _viewportRenderer = new EditorViewportRenderer(shadersDir);
        _modelShader = new Shader(Path.Combine(shadersDir, "shader.vert"), Path.Combine(shadersDir, "shader.frag"));

                                                                  
                                                                           
                                                                      
                                                                           
                                                                             
                                                                       
                                                                       
                                                                                
                                                                              
                                                                              
                                                                                            
                                                                       
                                                                          
                                                                     
                                      
        var startPos = new Vector3(6f, 4f, 6f);
        _flyCamera = new EditorFlyCamera(startPos);
        var lookDir = Vector3.Normalize(-startPos);                          
        float targetPitch = MathHelper.RadiansToDegrees(MathF.Asin(lookDir.Y));
        float targetYaw = MathHelper.RadiansToDegrees(MathF.Atan2(lookDir.Z, lookDir.X));
        _flyCamera.Camera.Rotate(targetYaw - _flyCamera.Camera.Yaw, targetPitch - _flyCamera.Camera.Pitch);

                                                                               
                                                                             
                                                     
        if (!string.IsNullOrEmpty(_initialProjectPath))
            OpenProject(_initialProjectPath, isNew: false);
    }

                                                                            
                                                                               
                                                                            
                                                                        
                                                                            
                                                                            
                                            
    private void OpenProject(string projectPath, bool isNew)
    {
        _projectPath = projectPath;
        EnsureProjectFolders();

        var projectLayoutIni = Path.Combine(_projectPath, "editor_layout.ini");
        _imgui!.SwitchIniFile(projectLayoutIni);

        _level = EditorSceneIO.LoadOrCreate(_projectPath, _sceneName, _modelCache, out _nextEventNumber);
        _currentProjectDescriptor = ProjectDescriptor.TryLoad(_projectPath);
        if (_flyCamera != null) _flyCamera.MouseSensitivity = _settings.CameraMouseSensitivity;

                                                                             
                                                                             
                                                                          
                                                                            
                                                                    
                                                                
        _database = GameDatabase.Load(_projectPath);
        _scripts = new ScriptHost(_projectPath, msg => _statusMessage = msg);
        _party.InitializeFromDatabase(_database);

        _userPresets = null;
        _envPreview = null;
        _console.Clear();
        _diagnosticsAge = 999;
        _scriptScanAge = 999;
        _scriptPath = string.Empty;
        InitializeSceneTabsForProject();
        ScanRecovery();

        RecentProjectsStore.AddOrPromote(_projectPath);
        _projectLoaded = true;
        _statusMessage = isNew ? $"Project created: {_projectPath}" : $"Project opened: {_projectPath}";
    }

    private void EnsureProjectFolders()
    {
        Directory.CreateDirectory(_projectPath);
        foreach (var folder in ProjectFolders)
            Directory.CreateDirectory(Path.Combine(_projectPath, folder));
    }

    protected override void OnUnload()
    {
                                                                                      
                                                
        _imgui?.SaveLayout();

        _viewportRenderer?.Dispose();
        _sceneFramebuffer?.Dispose();
        _modelShader?.Dispose();
        _imgui?.Dispose();
        base.OnUnload();
    }

    protected override void OnResize(ResizeEventArgs e)
    {
        base.OnResize(e);
        GL.Viewport(0, 0, e.Width, e.Height);
        _imgui?.WindowResized(e.Width, e.Height);
    }

    protected override void OnTextInput(TextInputEventArgs e)
    {
        base.OnTextInput(e);
    }

    protected override void OnRenderFrame(FrameEventArgs args)
    {
        base.OnRenderFrame(args);

        _lastFrameDelta = args.Time;
        GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

        _imgui!.Update(this, (float)args.Time);

        if (!_projectLoaded)
        {
            DrawLauncher();
        }
        else
        {
            DrawDockSpace();
            DrawProjectPanel();
            DrawHierarchyPanel();
            DrawInspectorPanel();
            DrawSceneViewPanel();
            DrawTopBar();
            DrawScenesPanel();
            DrawEnvironmentWindow();
            DrawDiagnosticsWindow();
            DrawConsoleWindow();
            DrawHistoryWindow();
            DrawScriptEditor();
            DrawShortcutsWindow();
            DrawSceneModals();
            DrawPalette();
            HandleGlobalShortcuts();
            RefreshDiagnostics();
            TrackSelectionHistory();
            HistoryTick();
            ConsoleTick();
            RecoveryTick();
            DrawRecoveryPopup();
            if (_playMode) DrawPlayModeDialogueOverlay();
            if (_playMode && _battle != null && (_battle.IsActive || _battle.Outcome != null)) DrawBattleOverlay();
        }

        _imgui.Render();

        SwapBuffers();
    }

    private void DrawDockSpace()
    {
        var viewport = ImGui.GetMainViewport();
        ImGui.SetNextWindowPos(viewport.WorkPos);
        ImGui.SetNextWindowSize(viewport.WorkSize);
        ImGui.SetNextWindowViewport(viewport.ID);

        var flags = ImGuiWindowFlags.NoDocking | ImGuiWindowFlags.NoTitleBar | ImGuiWindowFlags.NoCollapse
            | ImGuiWindowFlags.NoResize | ImGuiWindowFlags.NoMove | ImGuiWindowFlags.NoBringToFrontOnFocus
            | ImGuiWindowFlags.NoNavFocus | ImGuiWindowFlags.MenuBar;

        ImGui.PushStyleVar(ImGuiStyleVar.WindowRounding, 0f);
        ImGui.PushStyleVar(ImGuiStyleVar.WindowBorderSize, 0f);
        ImGui.PushStyleVar(ImGuiStyleVar.WindowPadding, System.Numerics.Vector2.Zero);
        ImGui.Begin("EditorDockSpaceHost", flags);
        ImGui.PopStyleVar(3);

        uint dockspaceId = ImGui.GetID("EditorDockSpace");
        ImGui.DockSpace(dockspaceId, System.Numerics.Vector2.Zero, ImGuiDockNodeFlags.None);

                                                                       
                                                                           
                                                                        
                                                                             
                                                                             
                                                                   
                                                                             
                                                                           
                                                                          
                           

        ImGui.End();
    }

                                                                                       
                                                                                     
                                                                                   
                                                                                  
                                                                                     
                                                                               
                                                                                            
    private void DrawProjectPanel()
    {
        ImGui.Begin("Project");
        ImGui.SetNextItemWidth(-1);
        ImGui.InputTextWithHint("##projectFilter", "Search assets...", ref _projectFilter, 64);
        Tip("Filter the files in the current folder.\nDrag a model/image/video from here into the Scene View to place it.\nShift+P/E/T/M/G place things at the cursor.");
        ImGui.Separator();

        ImGui.BeginChild("##ProjectFolderList", new System.Numerics.Vector2(160, 0), ImGuiChildFlags.Border);
        if (ImGui.Selectable("Primitives", _selectedProjectFolder == "Primitives"))
            _selectedProjectFolder = "Primitives";
        ImGui.Separator();
        foreach (var folder in ProjectFolders)
        {
                                                                                 
                                                                            
            var label = folder == SceneFolderName ? $"{folder} (Rooms)" : folder;
            if (ImGui.Selectable(label, _selectedProjectFolder == folder))
                _selectedProjectFolder = folder;

            if (ImGui.BeginPopupContextItem($"FolderCtx_{folder}"))
            {
                DrawFolderCreateMenu(folder);
                ImGui.EndPopup();
            }
        }
        ImGui.EndChild();

        ImGui.SameLine();
        ImGui.BeginChild("##ProjectFolderContent", System.Numerics.Vector2.Zero, ImGuiChildFlags.Border);
        DrawProjectFolderContent();
        ImGui.EndChild();

                                                                                    
                                                                                
                                                                            
        if (_requestNewScenePopup) { ImGui.OpenPopup("NewSceneNamePopup"); _requestNewScenePopup = false; }
        DrawNewSceneNamePopup();
        DrawNewScriptPopup();
        DrawSavePrefabPopup();

        ImGui.End();
    }

                                                                              
                                                                            
                                                                               
                                                          
    private void DrawProjectFolderContent()
    {
        if (_selectedProjectFolder == "Primitives")
        {
            ImGui.TextDisabled("Built-in shapes - select one, then P (or Objects tool) to place it.");
            ImGui.Separator();
            foreach (var (key, label) in Primitives)
            {
                bool isSelected = _selectedAsset == key;
                if (ImGui.Selectable(label, isSelected))
                {
                    _selectedAsset = key;
                    ClearSceneSelection();
                }
                BeginAssetDragSource(key, label);
            }
            return;
        }

        var folder = _selectedProjectFolder;
        bool isScenesFolder = folder == SceneFolderName;
        var fullPath = Path.Combine(_projectPath, folder);

        if (isScenesFolder)
        {
            if (ImGui.Button("+ New Scene...", new System.Numerics.Vector2(-1, 0)))
            {
                _newSceneNameBuffer = "NewScene";
                ImGui.OpenPopup("NewSceneNamePopup");
            }
            ImGui.Separator();
        }
        else
        {
            ImGui.TextDisabled($"Drop .{(folder == "Models" ? "glb" : folder == "Sound" ? "wav/mp3" : folder == "Images" ? "png" : folder == "Videos" ? "mp4" : "*")} files into this folder - they show up here.");
            ImGui.Separator();
        }

        int listed = 0;
        if (Directory.Exists(fullPath))
        {
            foreach (var file in Directory.EnumerateFiles(fullPath).OrderBy(f => f, StringComparer.OrdinalIgnoreCase))
            {
                var name = Path.GetFileName(file);
                if (_projectFilter.Length > 0 && !name.Contains(_projectFilter, StringComparison.OrdinalIgnoreCase)) continue;

                if (isScenesFolder && name.EndsWith(SceneFileSuffix, StringComparison.OrdinalIgnoreCase))
                {
                    var sceneKey = name[..^SceneFileSuffix.Length];
                    bool isCurrentScene = sceneKey == _sceneName;
                    var sceneLabel = (IsStartupScene(sceneKey) ? "* " : "") + (isCurrentScene ? $"{sceneKey}  (current)" : sceneKey);
                    if (ImGui.Selectable(sceneLabel, isCurrentScene) && !isCurrentScene)
                        OpenScene(sceneKey);
                    Tip(IsStartupScene(sceneKey) ? "Startup scene" : "Click to open in a tab.");
                    listed++;
                    continue;
                }

                bool isSelected = _selectedAsset == file;
                bool isLua = name.EndsWith(".lua", StringComparison.OrdinalIgnoreCase);
                if (ImGui.Selectable(name, isSelected, ImGuiSelectableFlags.AllowDoubleClick))
                {
                    _selectedAsset = file;
                    ClearSceneSelection();                                                    
                    if (isLua && ImGui.IsMouseDoubleClicked(ImGuiMouseButton.Left)) OpenScriptInEditor(file);
                }
                if (isLua) Tip("Double-click to edit.");
                else BeginAssetDragSource(file, name);
                listed++;
            }
        }

        if (listed == 0)
        {
            ImGui.Spacing();
            if (_projectFilter.Length > 0) ImGui.TextDisabled("No files match the search.");
            else if (isScenesFolder) ImGui.TextDisabled("No scenes yet.\nPress '+ New Scene...' above.");
            else if (folder == "Scripts") ImGui.TextDisabled("No scripts yet.\nRight-click -> New Script...");
            else if (folder == "Prefabs") ImGui.TextDisabled("No prefabs yet.\nSelect objects in the scene, then Edit -> Save Selection as Prefab.");
            else ImGui.TextDisabled("No assets yet.\nRight-click -> Open in Explorer and drop files in.");
        }

                                                                                  
                                                                                   
                                                                                    
                                                                   
        if (ImGui.BeginPopupContextWindow("FolderContentCtx"))
        {
            DrawFolderCreateMenu(folder);
            ImGui.EndPopup();
        }
    }

                                                                                  
                                                                                 
                                                                           
                                                                            
                                                                               
                                                                                  
    private void DrawFolderCreateMenu(string folder)
    {
        if (folder == SceneFolderName && ImGui.MenuItem("New Scene..."))
        {
            _newSceneNameBuffer = "NewScene";
            ImGui.OpenPopup("NewSceneNamePopup");
        }

        if (folder == "Scripts" && ImGui.MenuItem("New Script..."))
        {
            _newScriptName = "NewScript";
            _requestNewScriptPopup = true;
        }

        if (ImGui.MenuItem("Open in Explorer"))
        {
            var fullPath = Path.Combine(_projectPath, folder);
            Directory.CreateDirectory(fullPath);                                                 
            try { System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo(fullPath) { UseShellExecute = true }); }
            catch (Exception ex) { _statusMessage = $"Could not open folder: {ex.Message}"; }
        }
    }

                                                                                    
                                                                                    
                                                                                            
    private void DrawNewSceneNamePopup()
    {
        bool modalOpen = true;
        if (!ImGui.BeginPopupModal("NewSceneNamePopup", ref modalOpen, ImGuiWindowFlags.AlwaysAutoResize)) return;

        ImGui.InputText("Name", ref _newSceneNameBuffer, 64);
        var sanitized = SanitizeSceneName(_newSceneNameBuffer);
        bool sceneAlreadyExists = sanitized.Length > 0 &&
            File.Exists(Path.Combine(_projectPath, SceneFolderName, sanitized + SceneFileSuffix));
        if (sanitized.Length == 0)
            ImGui.TextColored(new System.Numerics.Vector4(1f, 0.5f, 0.4f, 1f), "Enter a valid name.");
        else if (sceneAlreadyExists)
            ImGui.TextColored(new System.Numerics.Vector4(1f, 0.5f, 0.4f, 1f), "A scene with this name already exists.");

        ImGui.BeginDisabled(sanitized.Length == 0 || sceneAlreadyExists);
        if (ImGui.Button("Create"))
        {
            NewScene(sanitized);
            ImGui.CloseCurrentPopup();
        }
        ImGui.EndDisabled();

        ImGui.SameLine();
        if (ImGui.Button("Cancel")) ImGui.CloseCurrentPopup();
        ImGui.EndPopup();
    }

                                                                        
                                                                            
                                                                            
                                                                             
                                                                       
                                                                        
    private static string SanitizeSceneName(string raw)
    {
        var invalid = Path.GetInvalidFileNameChars();
        return new string(raw.Trim().Where(c => !invalid.Contains(c)).ToArray());
    }

                                                                                
                                                                     
                                                                      
                                                                             
                                                                          
                                                                            
                                      
    private void NewScene(string sanitizedName)
    {
        if (_playMode) { _statusMessage = "Stop Play Mode before creating a scene."; return; }
        HistoryCommitNow();
        StashActiveTab();
        _level = new MeshRoomLevel(sanitizedName);
        _nextEventNumber = 1;
        _sceneName = sanitizedName;
        _editSceneSnapshotJson = null;

        _selectedModel = null;
        _selectedTrigger = null;
        _selectedAsset = string.Empty;

        EditorSceneIO.Save(_projectPath, _sceneName, _level, _nextEventNumber);
        AdoptCurrentSceneAsTab();
        _statusMessage = $"Created: Scenes/{_sceneName}{SceneFileSuffix}";
    }

                                                                                
                                                                      
                                                                          
                                                                          
                                                                            
                                                                           
                                                                    
                                                                       
                                                                         
                                                                          
                                                                       
                                                                     
                                               
    private void OpenScene(string sceneName) => SwitchToScene(sceneName);

                                                                                
                                                                             
                                                                              
                                                      
    private string _hierarchyFilter = string.Empty;

    private bool PassesHierarchyFilter(string label) =>
        _hierarchyFilter.Length == 0 || label.Contains(_hierarchyFilter, StringComparison.OrdinalIgnoreCase);

    private void DrawHierarchyPanel()
    {
        ImGui.Begin("Scene Hierarchy");

        ImGui.SetNextItemWidth(-1);
        ImGui.InputTextWithHint("##hierarchyFilter", $"Search in \"{_sceneName}\"...", ref _hierarchyFilter, 64);
        ImGui.Separator();

        if (_level.Models.Count == 0 && _level.Interactables.Count == 0 && _level.Triggers.Count == 0
            && _level.Posters.Count == 0 && _level.MoodZones.Count == 0 && _level.MoodEvents.Count == 0)
        {
            ImGui.TextWrapped("Scene is empty.");
            ImGui.TextDisabled("Right-click to create an object, or drag an asset from Project into the Scene View.");
        }

                                                                        
                                                                       
                                                                             
                                                                          
                                                                           
                                                                         
                                                                           
                                                         
        DrawHierarchySection($"Objects ({_level.Models.Count})", _level.Models, (model, i) =>
        {
            var label = $"{model.ModelKey} #{i}";
            if (!PassesHierarchyFilter(label)) return;
            if (ImGui.Selectable(label, IsSelected(model)))
                ClickInHierarchy(model);
            if (ImGui.BeginPopupContextItem($"ObjCtx_{i}"))
            {
                if (ImGui.MenuItem("Delete")) { _level.RemoveModel(model); if (ReferenceEquals(_selectedModel, model)) _selectedModel = null; }
                ImGui.EndPopup();
            }
        });

                                                                              
                                                                             
                                                                      
                                                                            
                                                                  
                                                                         
                                                                             

                                                                         
                                                                         
                                                                             
                                                                              
                                      
        DrawHierarchySection($"Triggers ({_level.Triggers.Count})", _level.Triggers, (trig, i) =>
        {
            var movementSuffix = trig.Movement == MovementPattern.Static ? "" : $", {trig.Movement}";
            var label = $"{trig.Type} r={trig.Radius:0.#} ({trig.Commands.Count} cmd{movementSuffix}) #{i}";
            if (!PassesHierarchyFilter(label)) return;
            if (ImGui.Selectable(label, IsSelected(trig)))
                ClickInHierarchy(trig);
            if (ImGui.BeginPopupContextItem($"TrigCtx_{i}"))
            {
                if (ImGui.MenuItem("Delete")) { _level.RemoveTrigger(trig); if (ReferenceEquals(_selectedTrigger, trig)) _selectedTrigger = null; }
                ImGui.EndPopup();
            }
        });

        DrawHierarchySection($"Posters ({_level.Posters.Count})", _level.Posters, (poster, i) =>
        {
            var kind = poster.IsVideo ? "video" : "image";
            var fileName = poster.IsVideo ? poster.VideoPath : poster.ImagePath;
            var label = $"{fileName} ({kind}) #{i}";
            if (!PassesHierarchyFilter(label)) return;
            if (ImGui.Selectable(label, IsSelected(poster)))
                ClickInHierarchy(poster);
            if (ImGui.BeginPopupContextItem($"PosterCtx_{i}"))
            {
                if (ImGui.MenuItem("Delete")) { _level.RemovePoster(poster); if (ReferenceEquals(_selectedPoster, poster)) _selectedPoster = null; }
                ImGui.EndPopup();
            }
        });

        DrawHierarchySection($"Mood Zones ({_level.MoodZones.Count})", _level.MoodZones, (zone, i) =>
        {
            var label = $"r={zone.Radius:0.#} -> {zone.TargetValue:0} #{i}";
            if (!PassesHierarchyFilter(label)) return;
            if (ImGui.Selectable(label, IsSelected(zone)))
                ClickInHierarchy(zone);
            if (ImGui.BeginPopupContextItem($"MoodZoneCtx_{i}"))
            {
                if (ImGui.MenuItem("Delete")) { _level.RemoveMoodZone(zone); if (ReferenceEquals(_selectedMoodZone, zone)) _selectedMoodZone = null; }
                ImGui.EndPopup();
            }
        });

        DrawHierarchySection($"Mood Events ({_level.MoodEvents.Count})", _level.MoodEvents, (moodEvent, i) =>
        {
            var label = $"{moodEvent.Effect} r={moodEvent.Radius:0.#} #{i}";
            if (!PassesHierarchyFilter(label)) return;
            if (ImGui.Selectable(label, IsSelected(moodEvent)))
                ClickInHierarchy(moodEvent);
            if (ImGui.BeginPopupContextItem($"MoodEventCtx_{i}"))
            {
                if (ImGui.MenuItem("Delete")) { _level.RemoveMoodEvent(moodEvent); if (ReferenceEquals(_selectedMoodEvent, moodEvent)) _selectedMoodEvent = null; }
                ImGui.EndPopup();
            }
        });

                                                                          
                                                                             
                                                                            
                                                                                 
                               
        DrawHierarchyCreateMenu();

        ImGui.End();
    }

                                                                               
                                                                          
                                                                                
                                                                              
                                                                         
    private void DrawHierarchySection<T>(string header, IReadOnlyList<T> items, Action<T, int> drawRow)
    {
        if (items.Count == 0) return;

        if (ImGui.CollapsingHeader(header, ImGuiTreeNodeFlags.DefaultOpen))
        {
            ImGui.Indent();
            for (int i = 0; i < items.Count; i++)
                drawRow(items[i], i);
            ImGui.Unindent();
        }
    }

                                                                                
                                                                                 
                                                                   
                                                                              
    private Vector3? GetSelectedPosition()
    {
        if (_selectedModel is { } model) return model.Position;
        if (_selectedTrigger is { } trig) return trig.Center;
        if (_selectedPoster is { } poster) return poster.Position;
        if (_selectedMoodZone is { } zone) return zone.Center;
        if (_selectedMoodEvent is { } moodEvent) return moodEvent.Center;
        return null;
    }

                                                                            
                                                                           
                                                                       
                                                                           
                                                                         
                                                                  
    private void MoveSelectedTo(Vector3 newPosition)
    {
        if (_extraSelection.Count > 0 && GetSelectedPosition() is { } before)
            MoveExtrasBy(newPosition - before);

        if (_selectedModel is { } model) model.Position = newPosition;
        else if (_selectedTrigger is { } trig) trig.SetCenter(newPosition);
        else if (_selectedPoster is { } poster) poster.SetPosition(newPosition);
        else if (_selectedMoodZone is { } zone) zone.SetCenter(newPosition);
        else if (_selectedMoodEvent is { } moodEvent) moodEvent.SetCenter(newPosition);
    }

                                                                        
                                                                            
                                                                         
                                                                                 
    private float? GetSelectedScale()
    {
        if (_selectedModel is { } model) return model.Scale;
        if (_selectedPoster is { } poster) return poster.Scale;
        return null;
    }

    private void SetSelectedScale(float newScale)
    {
        newScale = MathF.Max(newScale, 0.05f);                                                                    
        if (_extraSelection.Count > 0 && GetSelectedScale() is { } oldScale && oldScale > 0.0001f)
            ScaleExtrasBy(newScale / oldScale);
        if (_selectedModel is { } model) model.Scale = newScale;
        else if (_selectedPoster is { } poster) poster.Scale = newScale;
    }

                                                                                    
                                                                                   
                                                                                  
                                                                               
                                                                                 
                                            
    private void DuplicateSingleSelected()
    {
        var source = GetSelectedObject();
        if (source == null) return;
        if (CloneInto(_level, source, new Vector3(0.5f, 0f, 0.5f)) is { } copy)
            SelectAny(copy);
    }

    private void SelectModel(ModelInstance model)
    {
        _extraSelection.Clear();
        _selectedModel = model;
        _selectedTrigger = null;
        _selectedPoster = null;
        _selectedMoodZone = null;
        _selectedMoodEvent = null;
        _selectedAsset = string.Empty;
    }

                                                                              
                                                                                            
    private void SelectPoster(PosterInstance poster)
    {
        _extraSelection.Clear();
        _selectedPoster = poster;
        _selectedModel = null;
        _selectedTrigger = null;
        _selectedMoodZone = null;
        _selectedMoodEvent = null;
        _selectedAsset = string.Empty;
        _posterScaleBuffer = poster.Scale;
    }

                                                                                          
                                              
    private void SelectMoodZone(MoodZone zone)
    {
        _extraSelection.Clear();
        _selectedMoodZone = zone;
        _selectedModel = null;
        _selectedTrigger = null;
        _selectedPoster = null;
        _selectedMoodEvent = null;
        _selectedAsset = string.Empty;
        _moodZoneRadiusBuffer = zone.Radius;
        _moodZoneTargetValueBuffer = zone.TargetValue;
        _moodZonePullRateBuffer = zone.PullRatePerSecond;
        _moodZoneReleaseSecondsBuffer = zone.ReleaseSeconds;
    }

                                                                             
                                                                           
    private void SelectMoodEvent(MoodEvent moodEvent)
    {
        _extraSelection.Clear();
        _selectedMoodEvent = moodEvent;
        _selectedModel = null;
        _selectedTrigger = null;
        _selectedPoster = null;
        _selectedMoodZone = null;
        _selectedAsset = string.Empty;
        _moodEventRadiusBuffer = moodEvent.Radius;
        _moodEventEffectBuffer = moodEvent.Effect;
        _moodEventCooldownBuffer = moodEvent.CooldownSeconds;
        _moodEventChanceBuffer = moodEvent.BaseChance;
        _moodEventSoundBuffer = moodEvent.SoundName;
    }

                                                                            
                                                                                              
    private void SelectTrigger(TriggerZone trig)
    {
        _extraSelection.Clear();
        _selectedTrigger = trig;
        _selectedModel = null;
        _selectedAsset = string.Empty;

        _triggerCommandsBuffer = new List<EventCommand>(trig.Commands);
        _triggerPagesBuffer.Clear();
        foreach (var page in trig.Pages)
        {
            _triggerPagesBuffer.Add(new TriggerPageBuffer
            {
                RequiredFlag = page.RequiredFlag ?? string.Empty,
                RequiredFlagValue = page.RequiredFlagValue,
                Commands = new List<EventCommand>(page.Commands),
            });
        }
        _activeTriggerPage = -1;
        _triggerType = trig.Type;
        _triggerRadius = trig.Radius;
        _triggerRunOnce = trig.RunOnce;
        _triggerRequiredFlag = trig.RequiredFlag ?? string.Empty;
        _triggerRequiredFlagValue = trig.RequiredFlagValue;
        _triggerChance = trig.ChancePerCheck;
        _triggerInterval = trig.CheckIntervalSeconds;
        _triggerMarkerPath = trig.MarkerPath;
        _triggerMarkerScale = trig.MarkerScale;
        _triggerMovement = trig.Movement;
        _triggerPatrolPointsBuffer = new List<Vector3>(trig.PatrolPoints);
        _pendingCommandKind = string.Empty;
        _selectedPoster = null;
        _selectedMoodZone = null;
        _selectedMoodEvent = null;
        _commandUndo.Clear();
    }

                                                                               
                                                                              
                                                                              
    private void DrawInspectorPanel()
    {
        ImGui.Begin("Inspector");

        if (IsMultiSelection)
        {
            DrawMultiInspector();
        }
        else if (_selectedModel is { } model)
        {
            DrawModelInspector(model);
        }
        else if (_selectedTrigger is { } trig)
        {
            DrawTriggerInspector(trig);
        }
        else if (_selectedPoster is { } poster)
        {
            DrawPosterInspector(poster);
        }
        else if (_selectedMoodZone is { } zone)
        {
            DrawMoodZoneInspector(zone);
        }
        else if (_selectedMoodEvent is { } moodEvent)
        {
            DrawMoodEventInspector(moodEvent);
        }
        else if (!string.IsNullOrEmpty(_selectedAsset))
        {
            DrawAssetInspector(_selectedAsset);
        }
        else
        {
            DrawSceneInspector();
        }

        ImGui.End();
    }

    private void DrawModelInspector(ModelInstance model)
    {
        ImGui.Text(model.ModelKey);
        ImGui.Separator();

        if (ImGui.CollapsingHeader("Transform", ImGuiTreeNodeFlags.DefaultOpen))
        {
            var pos = new System.Numerics.Vector3(model.Position.X, model.Position.Y, model.Position.Z);
            if (ImGui.DragFloat3("Position", ref pos, 0.05f))
                model.Position = new Vector3(pos.X, pos.Y, pos.Z);
            HelpMarker("Where the object is in the world (X right, Y up, Z forward).");

            var rot = new System.Numerics.Vector3(model.RotationDeg.X, model.RotationDeg.Y, model.RotationDeg.Z);
            if (ImGui.DragFloat3("Rotation", ref rot, 1f))
                model.RotationDeg = new Vector3(rot.X, rot.Y, rot.Z);
            HelpMarker("Rotation in degrees around each axis. Y turns the object left/right.");

            float scale = model.Scale;
            if (ImGui.DragFloat("Scale", ref scale, 0.02f, 0.01f, 100f))
                model.Scale = scale;
            HelpMarker("Uniform size multiplier. 1 = original size.");
        }

        if (ImGui.CollapsingHeader("Rendering & Physics", ImGuiTreeNodeFlags.DefaultOpen))
        {
            bool isSolid = model.IsSolid;
            if (ImGui.Checkbox("Solid", ref isSolid)) model.IsSolid = isSolid;
            HelpMarker("Solid objects block the player and raycasts (walls, floors). Turn off for decoration you can walk through.");

            bool visible = model.Visible;
            if (ImGui.Checkbox("Visible", ref visible)) model.Visible = visible;
            HelpMarker("Hidden objects are not drawn but still exist in the scene.");
        }

        DrawPlayerStartButton(model.Position, model.RotationDeg.Y);

        ImGui.Separator();
        if (ImGui.Button("Delete"))
        {
            _level.RemoveModel(model);
            _selectedModel = null;
        }
    }

                                                                              
                                                                             
                                                                          
                                                                            
                                                                      
                                                                         
                                                                          
                                                                          
                                      
    private void DrawPlayerStartButton(Vector3 position, float yawDegrees)
    {
        ImGui.Separator();
        bool isCurrentStart = _level.PlayerStartPosition.HasValue
            && Vector3.DistanceSquared(_level.PlayerStartPosition.Value, position) < 0.0001f;

        if (isCurrentStart)
        {
            ImGui.TextColored(new System.Numerics.Vector4(0.4f, 0.85f, 0.4f, 1f), "This is the Player Start.");
            if (ImGui.Button("Clear Player Start"))
            {
                _level.PlayerStartPosition = null;
                _level.PlayerStartYawDegrees = 0f;
            }
        }
        else if (ImGui.Button("Set as Player Start"))
        {
            _level.PlayerStartPosition = position;
            _level.PlayerStartYawDegrees = yawDegrees;
        }
    }

                                                                                 
                                                                               
                                                                             
                                                                                              
                                                                                   
                                                                                  
                                                                            
                                                                                 
                                                                              
    private void DrawPosterInspector(PosterInstance poster)
    {
        ImGui.TextDisabled($"Poster @ {poster.Position.X:0.0}, {poster.Position.Y:0.0}, {poster.Position.Z:0.0}");
        ImGui.Text(poster.IsVideo ? $"Video: {poster.VideoPath}" : $"Image: {poster.ImagePath}");
        ImGui.Separator();

        ImGui.DragFloat("Scale", ref _posterScaleBuffer, 0.02f, 0.05f, 20f);

        ImGui.Separator();
        if (ImGui.Button("Apply"))
        {
            poster.Scale = _posterScaleBuffer <= 0f ? 1f : _posterScaleBuffer;
            _statusMessage = "Poster updated.";
        }

        ImGui.SameLine();
        if (ImGui.Button("Delete Poster"))
        {
            _level.RemovePoster(poster);
            _selectedPoster = null;
        }
    }

                                                                                     
                                                                                
                                                                              
                                                                                   
    private void DrawMoodZoneInspector(MoodZone zone)
    {
        ImGui.TextDisabled($"Mood Zone @ {zone.Center.X:0.0}, {zone.Center.Y:0.0}, {zone.Center.Z:0.0}");
        ImGui.Separator();

        ImGui.DragFloat("Radius", ref _moodZoneRadiusBuffer, 0.1f, 0.1f, 200f);
        ImGui.DragFloat("Target value (0-100)", ref _moodZoneTargetValueBuffer, 0.5f, 0f, 100f);
        ImGui.DragFloat("Pull rate / sec", ref _moodZonePullRateBuffer, 0.1f, 0.01f, 100f);
        ImGui.DragFloat("Release seconds", ref _moodZoneReleaseSecondsBuffer, 0.1f, 0.01f, 60f);

        ImGui.Separator();
        if (ImGui.Button("Apply"))
        {
            _level.RemoveMoodZone(zone);
            var updated = new MoodZone(zone.Center, _moodZoneRadiusBuffer, _moodZoneTargetValueBuffer,
                _moodZonePullRateBuffer, _moodZoneReleaseSecondsBuffer);
            _level.AddMoodZone(updated);
            SelectMoodZone(updated);
            _statusMessage = "Mood zone updated.";
        }

        ImGui.SameLine();
        if (ImGui.Button("Delete Mood Zone"))
        {
            _level.RemoveMoodZone(zone);
            _selectedMoodZone = null;
        }
    }

                                                                                    
                                                                                   
                                                                               
                                                                                          
    private void DrawMoodEventInspector(MoodEvent moodEvent)
    {
        ImGui.TextDisabled($"Mood Event @ {moodEvent.Center.X:0.0}, {moodEvent.Center.Y:0.0}, {moodEvent.Center.Z:0.0}");
        ImGui.Separator();

        var effectNames = Enum.GetNames<MoodEventEffect>();
        int effectIndex = Array.IndexOf(effectNames, _moodEventEffectBuffer.ToString());
        if (effectIndex < 0) effectIndex = 0;
        if (ImGui.Combo("Effect", ref effectIndex, effectNames, effectNames.Length))
            _moodEventEffectBuffer = Enum.Parse<MoodEventEffect>(effectNames[effectIndex]);

        ImGui.DragFloat("Radius", ref _moodEventRadiusBuffer, 0.1f, 0.1f, 200f);
        ImGui.DragFloat("Base chance (0-1)", ref _moodEventChanceBuffer, 0.01f, 0f, 1f);
        ImGui.DragFloat("Cooldown seconds", ref _moodEventCooldownBuffer, 1f, 0f, 3600f);

        if (_moodEventEffectBuffer is MoodEventEffect.SoundStinger or MoodEventEffect.Both)
        {
            ImGui.InputText("Sound file (Sounds/)", ref _moodEventSoundBuffer, 128);
        }
        else
        {
            ImGui.TextDisabled("(Sound file not used for Ghost-only effect)");
        }

        ImGui.Separator();
        if (ImGui.Button("Apply"))
        {
            _level.RemoveMoodEvent(moodEvent);
            var soundName = _moodEventEffectBuffer is MoodEventEffect.SoundStinger or MoodEventEffect.Both
                ? _moodEventSoundBuffer.Trim() : string.Empty;
            var updated = new MoodEvent(moodEvent.Center, _moodEventRadiusBuffer, _moodEventEffectBuffer,
                _moodEventCooldownBuffer, _moodEventChanceBuffer, soundName);
            _level.AddMoodEvent(updated);
            SelectMoodEvent(updated);
            _statusMessage = "Mood event updated.";
        }

        ImGui.SameLine();
        if (ImGui.Button("Delete Mood Event"))
        {
            _level.RemoveMoodEvent(moodEvent);
            _selectedMoodEvent = null;
        }
    }

    private void DrawTriggerInspector(TriggerZone trig)
    {
        ImGui.TextDisabled($"Trigger @ {trig.Center.X:0.0}, {trig.Center.Y:0.0}, {trig.Center.Z:0.0}");
        ImGui.Separator();

        var typeNames = Enum.GetNames<TriggerType>().Where(n => n != nameof(TriggerType.Door)).ToArray();
        int typeIndex = Array.IndexOf(typeNames, _triggerType.ToString());
        if (typeIndex < 0) typeIndex = 0;
        if (ImGui.Combo("Type", ref typeIndex, typeNames, typeNames.Length))
            _triggerType = Enum.Parse<TriggerType>(typeNames[typeIndex]);

        ImGui.DragFloat("Radius", ref _triggerRadius, 0.1f, 0.1f, 50f);
        ImGui.Checkbox("Run once", ref _triggerRunOnce);

        if (_triggerType == TriggerType.Parallel)
        {
            ImGui.DragFloat("Chance per check", ref _triggerChance, 0.01f, 0f, 1f);
            ImGui.DragFloat("Check interval (s)", ref _triggerInterval, 0.1f, 0.1f, 120f);
        }

                                                                        
                                                                           
                                                                     
        ImGui.InputText("Required switch (empty = always)", ref _triggerRequiredFlag, 128);
        if (!string.IsNullOrWhiteSpace(_triggerRequiredFlag))
            ImGui.Checkbox("Switch value must be", ref _triggerRequiredFlagValue);

                                                                          
                                                                       
                                                                            
                                                                             
                                                                               
                                                           
        ImGui.Separator();
        ImGui.InputText("Model (Models/*.glb, empty = invisible)", ref _triggerMarkerPath, 128);
        ImGui.DragFloat("Model scale", ref _triggerMarkerScale, 0.02f, 0.01f, 20f);

        var movementNames = Enum.GetNames<MovementPattern>();
        int movementIndex = Array.IndexOf(movementNames, _triggerMovement.ToString());
        if (movementIndex < 0) movementIndex = 0;
        if (ImGui.Combo("Movement", ref movementIndex, movementNames, movementNames.Length))
            _triggerMovement = Enum.Parse<MovementPattern>(movementNames[movementIndex]);

        if (_triggerMovement == MovementPattern.PatrolRoute)
        {
            ImGui.Text($"Patrol points ({_triggerPatrolPointsBuffer.Count})");
            for (int i = 0; i < _triggerPatrolPointsBuffer.Count; i++)
            {
                ImGui.PushID(i);
                var p = _triggerPatrolPointsBuffer[i];
                var pv = new System.Numerics.Vector3(p.X, p.Y, p.Z);
                if (ImGui.DragFloat3($"Point {i + 1}", ref pv, 0.05f))
                    _triggerPatrolPointsBuffer[i] = new Vector3(pv.X, pv.Y, pv.Z);
                ImGui.SameLine();
                if (ImGui.SmallButton("Remove")) { _triggerPatrolPointsBuffer.RemoveAt(i); ImGui.PopID(); break; }
                ImGui.PopID();
            }
                                                                               
                                                                           
                                                                          
            if (ImGui.Button("Add point at camera"))
                _triggerPatrolPointsBuffer.Add(_flyCamera!.Camera.Position);
        }

        ImGui.Separator();
                                                                                    
                                                                               
                                                                                  
                                                                               
                                                                              
                                                       
        ImGui.Text("Pages (RPG Maker-style, first matching flag wins, else Base)");
        if (ImGui.SmallButton("Base")) _activeTriggerPage = -1;
        for (int i = 0; i < _triggerPagesBuffer.Count; i++)
        {
            ImGui.SameLine();
            ImGui.PushID($"pagebtn{i}");
            if (ImGui.SmallButton($"Page {i + 1}")) _activeTriggerPage = i;
            ImGui.PopID();
        }
        ImGui.SameLine();
        if (ImGui.SmallButton("+ Add Page"))
        {
            _triggerPagesBuffer.Add(new TriggerPageBuffer());
            _activeTriggerPage = _triggerPagesBuffer.Count - 1;
        }

        List<EventCommand> activeBuffer;
        if (_activeTriggerPage >= 0 && _activeTriggerPage < _triggerPagesBuffer.Count)
        {
            var page = _triggerPagesBuffer[_activeTriggerPage];
            ImGui.Text($"Editing Page {_activeTriggerPage + 1}");
            ImGui.InputText("Page flag (empty = always active)", ref page.RequiredFlag, 128);
            ImGui.Checkbox("Flag must be", ref page.RequiredFlagValue);
            ImGui.SameLine();
            ImGui.TextDisabled(page.RequiredFlagValue ? "ON" : "OFF");
            if (ImGui.SmallButton("Remove this page"))
            {
                _triggerPagesBuffer.RemoveAt(_activeTriggerPage);
                _activeTriggerPage = -1;
            }
            activeBuffer = _activeTriggerPage >= 0 && _activeTriggerPage < _triggerPagesBuffer.Count
                ? _triggerPagesBuffer[_activeTriggerPage].Commands
                : _triggerCommandsBuffer;
        }
        else
        {
            ImGui.Text("Editing Base (fallback when no page's flag matches)");
            activeBuffer = _triggerCommandsBuffer;
        }

        ImGui.Separator();
        ImGui.Text($"Commands ({activeBuffer.Count})");
        DrawCommandList(activeBuffer);

        ImGui.Separator();
        DrawAddCommandUI(activeBuffer);

        ImGui.Separator();
        if (ImGui.Button("Apply"))
        {
                                                                                
                                                                        
                                                                               
                                                                          
                                                                     
            _level.RemoveTrigger(trig);
            var pages = _triggerPagesBuffer.Count == 0
                ? null
                : (IReadOnlyList<EventPage>)_triggerPagesBuffer.Select(p => new EventPage(
                    string.IsNullOrWhiteSpace(p.RequiredFlag) ? null : p.RequiredFlag.Trim(),
                    p.RequiredFlagValue,
                    new List<EventCommand>(p.Commands))).ToList();
            var updated = new TriggerZone(trig.Center, _triggerRadius, _triggerType,
                new List<EventCommand>(_triggerCommandsBuffer),
                requiredFlag: string.IsNullOrWhiteSpace(_triggerRequiredFlag) ? null : _triggerRequiredFlag.Trim(),
                requiredFlagValue: _triggerRequiredFlagValue,
                runOnce: _triggerRunOnce,
                chancePerCheck: _triggerChance,
                checkIntervalSeconds: _triggerInterval,
                pages: pages,
                markerPath: _triggerMarkerPath.Trim(),
                markerScale: _triggerMarkerScale <= 0f ? 1f : _triggerMarkerScale,
                movement: _triggerMovement,
                patrolPoints: _triggerPatrolPointsBuffer.ToArray());
            _level.AddTrigger(updated);
            SelectTrigger(updated);
            _statusMessage = "Trigger updated.";
        }

        ImGui.SameLine();
        if (ImGui.Button("Delete Trigger"))
        {
            _level.RemoveTrigger(trig);
            _selectedTrigger = null;
        }

        DrawPlayerStartButton(trig.Center, trig.YawDegrees);
    }

                                                                           
                                                                              
                                                                              
                                                                               
                                                                          
                                    
    private void DrawCommandList(List<EventCommand> buffer)
    {
        ImGui.BeginDisabled(_commandUndo.Count == 0);
        if (ImGui.SmallButton("Undo (Ctrl+Z)")) UndoCommandChange(buffer);
        ImGui.EndDisabled();

        if (buffer.Count == 0)
        {
            ImGui.TextDisabled("No commands yet - add one below.");
            return;
        }

        for (int i = 0; i < buffer.Count; i++)
        {
            ImGui.PushID(i);
            ImGui.TextWrapped($"{i + 1}. {EventCommandRegistry.Preview(buffer[i])}");
            ImGui.SameLine();
            if (ImGui.SmallButton("Up") && i > 0)
            {
                SaveCommandUndo(buffer);
                (buffer[i - 1], buffer[i]) = (buffer[i], buffer[i - 1]);
            }
            ImGui.SameLine();
            if (ImGui.SmallButton("Down") && i < buffer.Count - 1)
            {
                SaveCommandUndo(buffer);
                (buffer[i + 1], buffer[i]) = (buffer[i], buffer[i + 1]);
            }
            ImGui.SameLine();
            if (ImGui.SmallButton("Remove"))
            {
                SaveCommandUndo(buffer);
                buffer.RemoveAt(i);
                ImGui.PopID();
                break;                                                                             
            }
            ImGui.PopID();
        }
    }

                                                                                
                                                                 
                                                     
    private void SaveCommandUndo(List<EventCommand> buffer) => _commandUndo.Push(new List<EventCommand>(buffer));

    private void UndoCommandChange(List<EventCommand> buffer)
    {
        if (_commandUndo.Count == 0) return;
        var previous = _commandUndo.Pop();
        buffer.Clear();
        buffer.AddRange(previous);
    }

                                                                
                                                                           
                                                                            
                                                                           
                                                                              
                                                                             
                                                                                          
                                                                              
                                                                       
                                                                      
                                                                     
                                                                              
                                                                             
                          
       
                                                                        
                                                                           
                                                                           
                                                                     
                                                                            
                                                                                        
    private void DrawAddCommandUI(List<EventCommand> buffer)
    {
        ImGui.Text("Add command");

        var descriptors = EventCommandRegistry.All
            .OrderBy(d => d.Category, StringComparer.Ordinal).ThenBy(d => d.DisplayName, StringComparer.Ordinal)
            .ToArray();
        if (descriptors.Length == 0)
        {
            ImGui.TextDisabled("No command types registered.");
            return;
        }

        var categories = descriptors.Select(d => d.Category).Distinct().ToArray();
        if (string.IsNullOrEmpty(_pendingCommandCategory) || !categories.Contains(_pendingCommandCategory))
            _pendingCommandCategory = categories[0];

        int categoryIndex = Array.IndexOf(categories, _pendingCommandCategory);
        if (ImGui.Combo("Category", ref categoryIndex, categories, categories.Length))
            _pendingCommandCategory = categories[categoryIndex];

        var inCategory = descriptors.Where(d => d.Category == _pendingCommandCategory).ToArray();

        var current = EventCommandRegistry.FindByKind(_pendingCommandKind);
        if (current == null || current.Category != _pendingCommandCategory)
        {
            current = inCategory[0];
            SelectPendingCommand(current);
        }

        var labels = inCategory.Select(d => d.DisplayName).ToArray();
        int commandIndex = Array.IndexOf(inCategory, current);
        if (commandIndex < 0) commandIndex = 0;
        if (ImGui.Combo("Command", ref commandIndex, labels, labels.Length))
        {
            current = inCategory[commandIndex];
            SelectPendingCommand(current);
        }

        for (int i = 0; i < current.TypedParameters.Count; i++)
        {
            var param = current.TypedParameters[i];
            ImGui.PushID(i);
            switch (param.Type)
            {
                case EventCommandParameterType.Boolean:
                    bool b = _pendingCommandArgs[i].Equals("true", StringComparison.OrdinalIgnoreCase);
                    if (ImGui.Checkbox(param.Name, ref b)) _pendingCommandArgs[i] = b ? "true" : "false";
                    break;
                case EventCommandParameterType.Integer:
                    int iv = int.TryParse(_pendingCommandArgs[i], NumberStyles.Integer, CultureInfo.InvariantCulture, out var pi) ? pi : 0;
                    if (ImGui.InputInt(param.Name, ref iv)) _pendingCommandArgs[i] = iv.ToString(CultureInfo.InvariantCulture);
                    break;
                case EventCommandParameterType.Float:
                    float fv = float.TryParse(_pendingCommandArgs[i], NumberStyles.Float, CultureInfo.InvariantCulture, out var pf) ? pf : 0f;
                    if (ImGui.InputFloat(param.Name, ref fv)) _pendingCommandArgs[i] = fv.ToString(CultureInfo.InvariantCulture);
                    break;
                default:
                    string sv = _pendingCommandArgs[i];
                    if (ImGui.InputText(param.Name, ref sv, 512)) _pendingCommandArgs[i] = sv;
                    break;
            }
            ImGui.PopID();
        }

        if (current.IsVariadic)
        {
            ImGui.Text($"{current.VariadicParameterName} ({_pendingVariadicArgs.Count})");
            for (int i = 0; i < _pendingVariadicArgs.Count; i++)
            {
                ImGui.PushID(1000 + i);                                                                 
                var s = _pendingVariadicArgs[i];
                if (ImGui.InputText($"{current.VariadicParameterName} {i + 1}", ref s, 256))
                    _pendingVariadicArgs[i] = s;
                ImGui.SameLine();
                if (ImGui.SmallButton("Remove")) { _pendingVariadicArgs.RemoveAt(i); ImGui.PopID(); break; }
                ImGui.PopID();
            }
            if (ImGui.Button($"Add {current.VariadicParameterName}"))
                _pendingVariadicArgs.Add(string.Empty);
        }

        if (ImGui.Button("Add"))
        {
            try
            {
                var args = current.IsVariadic
                    ? _pendingCommandArgs.Concat(_pendingVariadicArgs).ToArray()
                    : _pendingCommandArgs;
                var built = current.Build(args);
                EventCommandRegistry.RegisterNameHistory(built);
                SaveCommandUndo(buffer);
                buffer.Add(built);
            }
            catch (Exception ex)
            {
                _statusMessage = $"Could not add command: {ex.Message}";
            }
        }
    }

                                                                                
                                                                               
                                                                      
                                                                               
                                                                          
                                                                  
    private void SelectPendingCommand(EventCommandDescriptor descriptor)
    {
        _pendingCommandKind = descriptor.Kind;
        _pendingCommandArgs = descriptor.TypedParameters.Select(p => p.DefaultValue).ToArray();
        _pendingVariadicArgs = descriptor.IsVariadic ? new List<string> { string.Empty, string.Empty } : new List<string>();
    }

                                                                                  
                                                                                 
                                                                      
                                                                          
                                                                           
                                                            
                                                                  
    private void DrawSceneViewPanel()
    {
        ImGui.Begin("Scene View");

        DrawSceneViewHeader();

        var contentRegion = ImGui.GetContentRegionAvail();
        int width = Math.Max(1, (int)contentRegion.X);
        int height = Math.Max(1, (int)contentRegion.Y);

        _sceneFramebuffer!.Resize(width, height);
        var activeCamera = _playMode ? _playCamera! : _flyCamera!.Camera;
        activeCamera.AspectRatio = width / (float)height;

        _sceneFramebuffer.Bind();
        _viewportRenderer!.Render(activeCamera, CurrentViewportBackground(), drawGrid: !_playMode && _showGrid);
        PrepareModelShaderForViewport();
        _viewportRenderer.RenderModels(activeCamera, _modelShader!, _level.Models);
        RenderEventModels(_modelShader!);
        FinishModelShaderForViewport();
        if (!_playMode && _showGizmos) _viewportRenderer.RenderMarkers(activeCamera, BuildViewportMarkers());

                                                                                 
                                                                                
                                                                               
                                                                                 
                                                            
        if (!_playMode && _showGizmos && _editMode == EditMode.Select && GetSelectedPosition() is { } gizmoOrigin)
        {
            float gizmoLength = Vector3.Distance(activeCamera.Position, gizmoOrigin) * 0.15f;
            gizmoLength = MathF.Max(gizmoLength, 0.3f);

            if (_gizmoMode == GizmoMode.Rotate && _selectedModel != null)
                _viewportRenderer.RenderRotateGizmo(activeCamera, gizmoOrigin, gizmoLength, _gizmoDragAxis ?? _gizmoHoverAxis);
            else
                _viewportRenderer.RenderGizmo(activeCamera, gizmoOrigin, gizmoLength, _gizmoDragAxis ?? _gizmoHoverAxis);
        }

        SampleViewportBrightness();
        SceneFramebuffer.BindDefault(ClientSize.X, ClientSize.Y);

        ImGui.Image((IntPtr)_sceneFramebuffer.ColorTexture, contentRegion,
            new System.Numerics.Vector2(0, 1), new System.Numerics.Vector2(1, 0));
        DrawSceneViewDropTarget();

                                                                        
                                                                               
                                                        
        var imageMin = ImGui.GetItemRectMin();
        _viewportOrigin = new Vector2(imageMin.X, imageMin.Y);
        _viewportSize = new Vector2(contentRegion.X, contentRegion.Y);

        DrawViewportOverlays(imageMin, contentRegion);

        if (_playMode)
        {
                                                                            
                                                                               
                                                                      
                                                                         
            var overlayPos = new System.Numerics.Vector2(imageMin.X + 8f, imageMin.Y + contentRegion.Y - 16f * _playLog.Count - 8f);
            foreach (var line in _playLog)
            {
                ImGui.SetCursorScreenPos(overlayPos);
                ImGui.TextColored(new System.Numerics.Vector4(1f, 1f, 1f, 0.9f), line);
                overlayPos.Y += 16f;
            }
        }

        bool hovered = ImGui.IsWindowHovered();
        ImGui.End();

        HandleViewportHotkeys(hovered);

        if (_playMode)
            UpdatePlayMode((float)_lastFrameDelta, hovered);
        else
            _flyCamera!.Update((float)_lastFrameDelta, this, hovered);

        HandleViewportPicking(hovered);
    }

                                                                        
                                                                       
                                                                            
                                                                            
                                                                              
                                                                            
                                                                             
                                                                      
                                                  
    private void UpdatePlayMode(float dt, bool viewportHovered)
    {
        var camera = _playCamera!;
        var mouse = MouseState;
        var kb = KeyboardState;

        float yawDelta = 0f, pitchDelta = 0f;
        bool rotating = viewportHovered && mouse.IsButtonDown(MouseButton.Right);
        if (rotating)
        {
            if (_playFirstMouse) { _playLastMouse = mouse.Position; _playFirstMouse = false; }
            else
            {
                var delta = mouse.Position - _playLastMouse;
                _playLastMouse = mouse.Position;
                const float sensitivity = 0.12f;
                yawDelta = delta.X * sensitivity;
                pitchDelta = -delta.Y * sensitivity;
            }
        }
        else
        {
            _playFirstMouse = true;
        }

                                                                  
                                                                      
                                                                        
                                                                      
                                                                         
                                                                        
                                            
        bool eventBusy = _playEventRunner.IsBusy || (_battle != null && (_battle.IsActive || _battle.Outcome != null));

        var forward = new Vector3(camera.Front.X, 0, camera.Front.Z);
        if (forward.LengthSquared > 0f) forward = Vector3.Normalize(forward);
        var right = new Vector3(camera.Right.X, 0, camera.Right.Z);
        if (right.LengthSquared > 0f) right = Vector3.Normalize(right);

                                                                        
                                                                          
                                                                       
        const float moveSpeed = 4f;
        var move = Vector3.Zero;
        if (!eventBusy)
        {
            if (kb.IsKeyDown(Keys.W)) move += forward;
            if (kb.IsKeyDown(Keys.S)) move -= forward;
            if (kb.IsKeyDown(Keys.D)) move += right;
            if (kb.IsKeyDown(Keys.A)) move -= right;
        }

        if (move.LengthSquared > 0f)
        {
            move = Vector3.Normalize(move) * moveSpeed * dt;
                                                                            
                                                                         
                                                         
            if (!_level.Raycast(_playerPosition + Vector3.UnitY * 0.9f, move, move.Length + 0.3f, out _, out _))
                _playerPosition += move;
        }

        _playCameraRig.Update(dt, camera, _playerPosition, yawDelta, pitchDelta, _level);
        _playEventRunner.Tick(dt);
        _battle?.Tick(dt);

                                                                      
                                                                        
                                                                       
        if (!_playEventRunner.IsBusy)
            _dialogueState?.ClearDialogue();

                                                                              
                                                                              
                                                                         
                                                       
        foreach (var trig in _level.Triggers)
            trig.UpdateMovement(dt, _level, _playerPosition);

        if (!eventBusy && kb.IsKeyPressed(Keys.F))
        {
            var trigger = _level.Triggers.FirstOrDefault(t => t.Type == TriggerType.OnInteract
                && (t.Center - _playerPosition).Length <= t.Radius);
            if (trigger != null)
                trigger.TryInteract(_playerPosition, _playEventRunner, _playEventContext!);
        }

        if (!eventBusy && kb.IsKeyPressed(Keys.E))
        {
            var target = _level.Interactables
                .Where(o => (o.Position - _playerPosition).Length <= o.InteractRadius)
                .OrderBy(o => (o.Position - _playerPosition).Length)
                .FirstOrDefault();
            var result = target?.Interact();
            if (!string.IsNullOrEmpty(result))
            {
                _playLog.Add(result!);
                if (_playLog.Count > 6) _playLog.RemoveAt(0);
            }
        }

        if (kb.IsKeyPressed(Keys.Escape))
            StopPlayMode();
    }

    private void StartPlayMode()
    {
        HistoryCommitNow();
        _envPreview = null;
        _paletteOpen = false;
                                                                           
                                                                            
                                           
        _editSceneSnapshotJson = EditorSceneIO.SerializeToJson(_level, _sceneName, _nextEventNumber);

        _keepPlayChangesOnStop = false;
        _playerPosition = _playFromHere ? ComputePlayFromHerePosition() : ComputePlayStartPosition();

                                                                         
                                                                    
                                                                            
                                                                             
                                                                    
                                                                      
                                                                            
                                                                            
                                      
        if (_playFromHere)
        {
            _playCamera = new Camera(_playerPosition + Vector3.UnitY * 1.6f, yaw: _flyCamera!.Camera.Yaw, pitch: 0f)
            {
                AspectRatio = _flyCamera.Camera.AspectRatio,
            };
        }
        else if (_level.PlayerStartPosition.HasValue)
        {
            _playCamera = new Camera(_playerPosition + Vector3.UnitY * 1.6f,
                yaw: _level.PlayerStartYawDegrees - 90f, pitch: 0f)
            {
                AspectRatio = _flyCamera!.Camera.AspectRatio,
            };
        }
        else
        {
            _playCamera = new Camera(_playerPosition + new Vector3(0f, 1.6f, 4f), yaw: -90f, pitch: -10f)
            {
                AspectRatio = _flyCamera!.Camera.AspectRatio,
            };
        }
        _playFirstMouse = true;
        _playLog.Clear();
        _dialogueState = new DialogueState();
        _battle = new BattleState();
        _playEventContext = new EditorPlayEventContext(
            pos => _playerPosition = pos, _level, _playCamera,
            msg => { _playLog.Add(msg); if (_playLog.Count > 6) _playLog.RemoveAt(0); },
            _dialogueState, _battle, _database, _party, _scripts);

        _selectedModel = null;
        _selectedTrigger = null;
        _selectedPoster = null;
        _selectedMoodZone = null;
        _selectedMoodEvent = null;
        _selectedAsset = string.Empty;

        _playMode = true;
        _statusMessage = "Play Mode started - scene snapshotted, Stop will revert runtime changes.";
    }

    private void StopPlayMode()
    {
        _playMode = false;
        _playFromHere = false;
        bool keep = _keepPlayChangesOnStop;
        _keepPlayChangesOnStop = false;

        if (keep)
        {
            _editSceneSnapshotJson = null;                                                 
        }
        else if (_editSceneSnapshotJson != null)
        {
            _level = EditorSceneIO.DeserializeFromJson(_editSceneSnapshotJson, _sceneName, _projectPath, _modelCache, out _nextEventNumber);
            _editSceneSnapshotJson = null;
        }

        _historyCounts = CountObjects();
        _historyEnv = _level.Environment;
        _historyWasInteracting = false;
        ClearSceneSelection();
        if (_activeTab != null) _activeTab.Level = _level;

        _playCamera = null;
        _playEventContext = null;
        _dialogueState = null;
        _battle = null;
        if (keep)
        {
            _history.Commit(EditorSceneIO.SerializeToJson(_level, _sceneName, _nextEventNumber), "Keep Play Changes");
            _historyCounts = CountObjects();
            _statusMessage = "Play Mode stopped - changes made during Play were KEPT (Ctrl+Z to undo).";
        }
        else _statusMessage = "Play Mode stopped - scene reverted to its pre-Play state.";
    }

    private bool _playFromHere;
    private bool _keepPlayChangesOnStop;

                                                                                                                         
    private void PlayFromHere()
    {
        if (_playMode) return;
        _playFromHere = true;
        StartPlayMode();
    }

    private Vector3 ComputePlayFromHerePosition()
    {
        var cam = _flyCamera!.Camera;
                                                                                           
        if (_level.Raycast(cam.Position, cam.Front, 60f, out var hit, out _)) return hit + Vector3.UnitY * 0.1f;
        if (_level.Raycast(cam.Position + Vector3.UnitY * 1f, -Vector3.UnitY, 60f, out var floorHit, out _))
            return floorHit + Vector3.UnitY * 0.05f;
        return new Vector3(cam.Position.X, 0f, cam.Position.Z);
    }

                                                                              
                                                                                
                                                                          
                                                                               
                                                                     
                                                                  
    private Vector3 ComputePlayStartPosition()
    {
        if (_level.PlayerStartPosition is { } start) return start;

        var camPos = _flyCamera!.Camera.Position;
        if (_level.Raycast(camPos + Vector3.UnitY * 5f, -Vector3.UnitY, 50f, out var floorHit, out _))
            return floorHit + Vector3.UnitY * 0.05f;
        return new Vector3(camPos.X, 0f, camPos.Z);
    }

                                                                            
                                                                         
                                                                             
                                                                              
                                                                            
                                                                        
                                                                            
                                                                            
                                                                            
                                
    private List<EditorViewportRenderer.ViewportMarker> BuildViewportMarkers()
    {
        var markers = new List<EditorViewportRenderer.ViewportMarker>(
            _level.Interactables.Count + _level.Triggers.Count);

        foreach (var evt in _level.Interactables)
        {
            markers.Add(new EditorViewportRenderer.ViewportMarker(
                evt.Position, new Vector3(0.85f, 0.75f, 0.2f), Selected: false));
        }

        foreach (var trig in _level.Triggers)
        {
            markers.Add(new EditorViewportRenderer.ViewportMarker(
                trig.Center, new Vector3(0.25f, 0.6f, 0.9f), ReferenceEquals(trig, _selectedTrigger), trig.Radius));
        }

                                                                              
                                                                           
                                                 
        if (_level.PlayerStartPosition is { } start)
        {
            markers.Add(new EditorViewportRenderer.ViewportMarker(
                start, new Vector3(0.2f, 1f, 0.3f), Selected: true));
        }

        if (IsMultiSelection)
        {
            foreach (var o in AllSelected())
                if (PositionOf(o) is { } pos)
                    markers.Add(new EditorViewportRenderer.ViewportMarker(pos, new Vector3(1f, 0.55f, 0.1f), Selected: true));
        }

        return markers;
    }

                                                                                
                                                                          
                                                                           
                                                                    
                                                                               
                                                                          
                                                                             
                                                                                        
                                                                                 
                                                                                
                                                                  
                                                                             
                                                                           
                                                                            
                                                                      
    private static GizmoAxis? PickGizmoAxis(Camera camera, Vector3 origin, float axisLength,
        Vector2 mouseScreen, Vector2 viewportOrigin, Vector2 viewportSize)
    {
        const float clickThresholdPx = 10f;

        GizmoAxis? best = null;
        float bestDistance = clickThresholdPx;

        void TryAxis(GizmoAxis axis, Vector3 direction)
        {
            if (!ViewportRay.TryWorldToScreen(camera, origin, viewportOrigin, viewportSize, out var screenA)) return;
            if (!ViewportRay.TryWorldToScreen(camera, origin + direction * axisLength, viewportOrigin, viewportSize, out var screenB)) return;

            float dist = ViewportRay.DistancePointToSegment(mouseScreen, screenA, screenB);
            if (dist < bestDistance)
            {
                bestDistance = dist;
                best = axis;
            }
        }

        TryAxis(GizmoAxis.X, Vector3.UnitX);
        TryAxis(GizmoAxis.Y, Vector3.UnitY);
        TryAxis(GizmoAxis.Z, Vector3.UnitZ);
        return best;
    }

                                                                          
                                                                                 
                                                                                
                                                                                
                                                                           
                                 
    private const int RotateRingPickSamples = 24;

    private static GizmoAxis? PickRotateAxis(Camera camera, Vector3 origin, float radius,
        Vector2 mouseScreen, Vector2 viewportOrigin, Vector2 viewportSize)
    {
        const float clickThresholdPx = 10f;

        GizmoAxis? best = null;
        float bestDistance = clickThresholdPx;

        void TryRing(GizmoAxis axis, Vector3 ringAxis)
        {
            Vector3 perpA = Vector3.Normalize(ringAxis == Vector3.UnitY ? Vector3.UnitX : Vector3.UnitY);
            perpA = Vector3.Normalize(perpA - ringAxis * Vector3.Dot(perpA, ringAxis));
            Vector3 perpB = Vector3.Cross(ringAxis, perpA);

            Vector2? prevScreen = null;
            for (int i = 0; i <= RotateRingPickSamples; i++)
            {
                float angle = i / (float)RotateRingPickSamples * MathF.Tau;
                var worldPoint = origin + (perpA * MathF.Cos(angle) + perpB * MathF.Sin(angle)) * radius;
                if (!ViewportRay.TryWorldToScreen(camera, worldPoint, viewportOrigin, viewportSize, out var screenPoint))
                {
                    prevScreen = null;                                                                       
                    continue;
                }

                if (prevScreen is { } prev)
                {
                    float dist = ViewportRay.DistancePointToSegment(mouseScreen, prev, screenPoint);
                    if (dist < bestDistance)
                    {
                        bestDistance = dist;
                        best = axis;
                    }
                }
                prevScreen = screenPoint;
            }
        }

        TryRing(GizmoAxis.X, Vector3.UnitX);
        TryRing(GizmoAxis.Y, Vector3.UnitY);
        TryRing(GizmoAxis.Z, Vector3.UnitZ);
        return best;
    }

    private void HandleViewportPicking(bool viewportHovered)
    {
                                                                               
                                                                            
                                                                             
                                                                          
                                                                           
        if (_playMode || !viewportHovered) return;

        const float reach = 20f;
        var camera = _flyCamera!.Camera;
        bool hasRay = ViewportRay.TryCompute(camera, MouseState.Position, _viewportOrigin, _viewportSize,
            out var rayOrigin, out var rayDirection);

                                                                                 
                                                                             
                                                                                
                                                                               
                                                                        
                                                                                 
        _gizmoHoverAxis = null;
        bool useRotateGizmo = _gizmoMode == GizmoMode.Rotate && _selectedModel != null;
        bool useScaleGizmo = _gizmoMode == GizmoMode.Scale && GetSelectedScale() != null;
        if (_showGizmos && _editMode == EditMode.Select && !_isDraggingSelection && !_isDraggingPlayerStart
            && _gizmoDragAxis == null && GetSelectedPosition() is { } gizmoOrigin)
        {
            float gizmoRadius = Vector3.Distance(camera.Position, gizmoOrigin) * 0.15f;
            _gizmoHoverAxis = useRotateGizmo
                ? PickRotateAxis(camera, gizmoOrigin, gizmoRadius, MouseState.Position, _viewportOrigin, _viewportSize)
                : PickGizmoAxis(camera, gizmoOrigin, gizmoRadius, MouseState.Position, _viewportOrigin, _viewportSize);
        }

        bool navigationBlocksClick = _flyCamera.AltHeld || _flyCamera.IsNavigating;
        if (!_showGizmos) _gizmoHoverAxis = null;

        if (hasRay && !navigationBlocksClick && MouseState.IsButtonPressed(MouseButton.Left) && _gizmoHoverAxis is { } pressedAxis
            && GetSelectedPosition() is { } dragOrigin)
        {
                                                                                
                                                                             
                     
            if (useRotateGizmo && _selectedModel is { } dragModel
                && ViewportRay.TryWorldToScreen(camera, dragOrigin, _viewportOrigin, _viewportSize, out var screenOrigin))
            {
                var mouse = MouseState.Position;
                _gizmoDragAxis = pressedAxis;
                _gizmoDragMode = GizmoMode.Rotate;
                _gizmoDragStartScreenAngle = MathF.Atan2(mouse.Y - screenOrigin.Y, mouse.X - screenOrigin.X);
                _gizmoDragStartRotationDeg = dragModel.RotationDeg;
            }
            else if (useScaleGizmo && GetSelectedScale() is { } startScale)
            {
                var axisDir = pressedAxis switch
                {
                    GizmoAxis.X => Vector3.UnitX,
                    GizmoAxis.Y => Vector3.UnitY,
                    _ => Vector3.UnitZ,
                };
                if (ViewportRay.TryClosestPointOnLineToRay(dragOrigin, axisDir, rayOrigin, rayDirection, out var scaleStartPoint))
                {
                    _gizmoDragAxis = pressedAxis;
                    _gizmoDragMode = GizmoMode.Scale;
                    _gizmoDragAxisWorldStart = scaleStartPoint;
                    _gizmoDragStartScale = startScale;
                    _gizmoDragStartAxisLength = MathF.Max(Vector3.Distance(camera.Position, dragOrigin) * 0.15f, 0.3f);
                }
            }
            else if (!useRotateGizmo && !useScaleGizmo)
            {
                var axisDir = pressedAxis switch
                {
                    GizmoAxis.X => Vector3.UnitX,
                    GizmoAxis.Y => Vector3.UnitY,
                    _ => Vector3.UnitZ,
                };
                if (ViewportRay.TryClosestPointOnLineToRay(dragOrigin, axisDir, rayOrigin, rayDirection, out var startPoint))
                {
                    _gizmoDragAxis = pressedAxis;
                    _gizmoDragMode = GizmoMode.Move;
                    _gizmoDragAxisWorldStart = startPoint;
                    _gizmoDragEntityStart = dragOrigin;
                }
            }
        }
        else if (hasRay && !navigationBlocksClick && MouseState.IsButtonPressed(MouseButton.Left))
        {
                                                                             
                                                                        
                                                                             
                                                                           
                                                                         
                                                                
            switch (_editMode)
            {
                case EditMode.Objects when !string.IsNullOrEmpty(_selectedAsset):
                    PlaceSelectedAsset(rayOrigin, rayDirection);
                    break;
                case EditMode.Events when _eventPlacementKind == EventPlacementKind.Point:
                    PlaceEventAtRay(rayOrigin, rayDirection);
                    break;
                case EditMode.Events:
                    PlaceTriggerAtRay(rayOrigin, rayDirection);
                    break;
                default:
                    var beforePrimary = GetSelectedObject();
                    var beforeExtras = _extraSelection.ToList();
                    bool additiveClick = KeyboardState.IsKeyDown(Keys.LeftControl) || KeyboardState.IsKeyDown(Keys.RightControl)
                        || KeyboardState.IsKeyDown(Keys.LeftShift) || KeyboardState.IsKeyDown(Keys.RightShift);
                    if (ObjectStatePicker.TryPick(_level, rayOrigin, rayDirection, reach, out var picked) && picked != null)
                    {
                        SelectModel(picked);
                        _isDraggingSelection = true;                                                                             
                    }
                    else
                    {
                        var pickedTrigger = _level.Triggers
                            .Where(t => RaySphereHit(rayOrigin, rayDirection, t.Center, MathF.Max(t.Radius, 0.3f), out _))
                            .OrderBy(t => (t.Center - rayOrigin).Length)
                            .FirstOrDefault();
                        if (pickedTrigger != null)
                        {
                            SelectTrigger(pickedTrigger);
                            _isDraggingSelection = true;
                        }
                        else if (_level.Posters
                            .Where(p => RaySphereHit(rayOrigin, rayDirection, p.Position, MathF.Max(0.75f * p.Scale, 0.3f), out _))
                            .OrderBy(p => (p.Position - rayOrigin).Length)
                            .FirstOrDefault() is { } pickedPoster)
                        {
                                                                                          
                                                                                              
                            SelectPoster(pickedPoster);
                        }
                        else if (_level.MoodZones
                            .Where(z => RaySphereHit(rayOrigin, rayDirection, z.Center, MathF.Max(z.Radius, 0.3f), out _))
                            .OrderBy(z => (z.Center - rayOrigin).Length)
                            .FirstOrDefault() is { } pickedZone)
                        {
                            SelectMoodZone(pickedZone);
                        }
                        else if (_level.MoodEvents
                            .Where(m => RaySphereHit(rayOrigin, rayDirection, m.Center, MathF.Max(m.Radius, 0.3f), out _))
                            .OrderBy(m => (m.Center - rayOrigin).Length)
                            .FirstOrDefault() is { } pickedMoodEvent)
                        {
                            SelectMoodEvent(pickedMoodEvent);
                        }
                        else if (_level.PlayerStartPosition is { } startPos
                            && RaySphereHit(rayOrigin, rayDirection, startPos, 0.5f, out _))
                        {
                                                                                
                                                                           
                                                                                
                                                                                
                                                                           
                            _isDraggingPlayerStart = true;
                        }
                    }
                    if (additiveClick && GetSelectedObject() is { } clicked && !ReferenceEquals(clicked, beforePrimary))
                    {
                                                                                                        
                        _isDraggingSelection = false;
                        _isDraggingPlayerStart = false;
                        if (beforePrimary != null)
                        {
                            SelectAny(beforePrimary);
                            _extraSelection.AddRange(beforeExtras);
                            ToggleInSelection(clicked);
                        }
                    }
                    _dragMouseStart = MouseState.Position;
                    break;
            }
        }

                                                                               
                                                                          
                                                                              
                                                                         
                                                                            
                                                                        
                                                               
        if (_gizmoDragAxis is { } activeDragAxis && _gizmoDragMode == GizmoMode.Move)
        {
            if (!MouseState.IsButtonDown(MouseButton.Left))
            {
                _gizmoDragAxis = null;
            }
            else if (hasRay)
            {
                var axisDir = activeDragAxis switch
                {
                    GizmoAxis.X => Vector3.UnitX,
                    GizmoAxis.Y => Vector3.UnitY,
                    _ => Vector3.UnitZ,
                };
                if (ViewportRay.TryClosestPointOnLineToRay(_gizmoDragAxisWorldStart, axisDir, rayOrigin, rayDirection, out var nowPoint))
                {
                    var delta = nowPoint - _gizmoDragAxisWorldStart;

                                                                                 
                                                                               
                                                                           
                                                                                 
                                                                           
                                                                               
                              
                    if (SnapActive())
                    {
                        float snapStep = _snapMove;
                        float signedLength = Vector3.Dot(delta, axisDir);
                        float snappedLength = MathF.Round(signedLength / snapStep) * snapStep;
                        delta = axisDir * snappedLength;
                    }

                    MoveSelectedTo(_gizmoDragEntityStart + delta);
                }
            }
        }
        else if (_gizmoDragAxis is { } activeRotateAxis && _gizmoDragMode == GizmoMode.Rotate)
        {
            if (!MouseState.IsButtonDown(MouseButton.Left))
            {
                _gizmoDragAxis = null;
            }
            else if (_selectedModel is { } dragModel
                && ViewportRay.TryWorldToScreen(camera, dragModel.Position, _viewportOrigin, _viewportSize, out var screenOrigin))
            {
                var mouse = MouseState.Position;
                float nowAngle = MathF.Atan2(mouse.Y - screenOrigin.Y, mouse.X - screenOrigin.X);

                                                                             
                                                                               
                                                                            
                                      
                float deltaRad = MathF.Atan2(MathF.Sin(nowAngle - _gizmoDragStartScreenAngle), MathF.Cos(nowAngle - _gizmoDragStartScreenAngle));
                float deltaDeg = deltaRad * (180f / MathF.PI);

                if (SnapActive())
                {
                    float snapStepDeg = _snapRotate;
                    deltaDeg = MathF.Round(deltaDeg / snapStepDeg) * snapStepDeg;
                }

                var newRotation = _gizmoDragStartRotationDeg;
                switch (activeRotateAxis)
                {
                    case GizmoAxis.X: newRotation.X += deltaDeg; break;
                    case GizmoAxis.Y: newRotation.Y += deltaDeg; break;
                    default: newRotation.Z += deltaDeg; break;
                }
                RotateExtrasBy(newRotation - dragModel.RotationDeg);
                dragModel.RotationDeg = newRotation;
            }
        }
        else if (_gizmoDragAxis is { } activeScaleAxis && _gizmoDragMode == GizmoMode.Scale)
        {
            if (!MouseState.IsButtonDown(MouseButton.Left))
            {
                _gizmoDragAxis = null;
            }
            else if (hasRay)
            {
                var axisDir = activeScaleAxis switch
                {
                    GizmoAxis.X => Vector3.UnitX,
                    GizmoAxis.Y => Vector3.UnitY,
                    _ => Vector3.UnitZ,
                };
                if (ViewportRay.TryClosestPointOnLineToRay(_gizmoDragAxisWorldStart, axisDir, rayOrigin, rayDirection, out var nowPoint))
                {
                                                                                  
                                                                               
                                                                                    
                                                                            
                                                                         
                                                                         
                                                                              
                    float signedLength = Vector3.Dot(nowPoint - _gizmoDragAxisWorldStart, axisDir);
                    float multiplier = 1f + signedLength / _gizmoDragStartAxisLength;

                    if (SnapActive())
                    {
                        float snapStep = _snapScale;
                        multiplier = MathF.Round(multiplier / snapStep) * snapStep;
                    }

                    SetSelectedScale(_gizmoDragStartScale * multiplier);
                }
            }
        }

        if (_isDraggingPlayerStart)
        {
            if (!MouseState.IsButtonDown(MouseButton.Left))
                _isDraggingPlayerStart = false;
            else if (hasRay && _level.PlayerStartPosition is { } currentStart
                && (MouseState.Position - _dragMouseStart).Length > 3f
                && TryIntersectHorizontalPlane(rayOrigin, rayDirection, currentStart.Y, out var hit))
            {
                _level.PlayerStartPosition = new Vector3(hit.X, currentStart.Y, hit.Z);
            }
        }

                                                                       
                                                                       
                                                                       
                                                                      
                                                                               
                                                                            
                                                                      
                                                                            
                                               
        if (_isDraggingSelection && _editMode == EditMode.Select)
        {
            if (!MouseState.IsButtonDown(MouseButton.Left))
            {
                _isDraggingSelection = false;
            }
            else if (hasRay && (MouseState.Position - _dragMouseStart).Length > 3f)
            {
                float planeY = _selectedModel?.Position.Y ?? _selectedTrigger?.Center.Y ?? 0f;
                if (TryIntersectHorizontalPlane(rayOrigin, rayDirection, planeY, out var hit))
                {
                    if (_selectedModel != null)
                    {
                        var target = new Vector3(hit.X, _selectedModel.Position.Y, hit.Z);
                        MoveExtrasBy(target - _selectedModel.Position);
                        _selectedModel.Position = target;
                    }
                    else if (_selectedTrigger != null)
                    {
                        var target = new Vector3(hit.X, _selectedTrigger.Center.Y, hit.Z);
                        MoveExtrasBy(target - _selectedTrigger.Center);
                        _selectedTrigger.SetCenter(target);
                    }
                }
            }
        }

                                                                                                
        bool ctrlHeld = KeyboardState.IsKeyDown(Keys.LeftControl) || KeyboardState.IsKeyDown(Keys.RightControl);
        bool shiftHeld = KeyboardState.IsKeyDown(Keys.LeftShift) || KeyboardState.IsKeyDown(Keys.RightShift);

                                                                                
                                                                              
                                                                       
                                                                        
                                                      
        if (ctrlHeld && KeyboardState.IsKeyPressed(Keys.D) && _editMode == EditMode.Select
            && _gizmoDragAxis == null && !_isDraggingSelection && !_isDraggingPlayerStart)
        {
            DuplicateSelected();
        }

        if (hasRay && shiftHeld && !ctrlHeld && KeyboardState.IsKeyPressed(Keys.P) && !string.IsNullOrEmpty(_selectedAsset))
        {
            PlaceSelectedAsset(rayOrigin, rayDirection);
        }

        if (hasRay && shiftHeld && !ctrlHeld && KeyboardState.IsKeyPressed(Keys.E))
        {
            PlaceEventAtRay(rayOrigin, rayDirection);
        }

        if (hasRay && shiftHeld && !ctrlHeld && KeyboardState.IsKeyPressed(Keys.T))
        {
            PlaceTriggerAtRay(rayOrigin, rayDirection);
        }

        if (hasRay && shiftHeld && !ctrlHeld && KeyboardState.IsKeyPressed(Keys.M))
        {
            PlaceMoodZoneAtRay(rayOrigin, rayDirection);
        }

        if (hasRay && shiftHeld && !ctrlHeld && KeyboardState.IsKeyPressed(Keys.G))
        {
            PlaceMoodEventAtRay(rayOrigin, rayDirection);
        }

                                                                               
                                                                          
                                                                              
                                                                            
                                                                            
                                                                           
                                                                            
                                                                           
                                                        
        if (hasRay && MouseState.IsButtonPressed(MouseButton.Right))
        {
            _rightClickStartPos = MouseState.Position;
        }
        if (hasRay && !_flyCamera.AltHeld && MouseState.IsButtonReleased(MouseButton.Right)
            && (MouseState.Position - _rightClickStartPos).Length < 4f)
        {
            _pendingCreateRayOrigin = rayOrigin;
            _pendingCreateRayDirection = rayDirection;
            ImGui.OpenPopup(ViewportCreateMenuId);
        }
    }

                                                                             
                                                                         
                                                                       
                                                                           
                                                                           
                                                                       
                                                                                 
    private bool TryResolvePlacementPoint(Vector3 origin, Vector3 direction, out Vector3 point)
    {
        const float reach = 20f;

        if (_level.Raycast(origin, direction, reach, out var hitPoint, out _))
        {
            point = hitPoint;
            return true;
        }

        if (MathF.Abs(direction.Y) < 1e-4f) { point = default; return false; }                                                    

        float t = -origin.Y / direction.Y;
        if (t <= 0f) { point = default; return false; }                                                       

        point = origin + direction * t;
        return true;
    }

                                                                              
                                                                           
                                                                              
                                                                           
                                                                         
                                                                            
                                                                 
    private static bool TryIntersectHorizontalPlane(Vector3 origin, Vector3 direction, float planeY, out Vector3 point)
    {
        if (MathF.Abs(direction.Y) < 1e-4f) { point = default; return false; }

        float t = (planeY - origin.Y) / direction.Y;
        if (t <= 0f) { point = default; return false; }

        point = origin + direction * t;
        return true;
    }

                                                                             
                                                                          
                                                                        
                                                                      
                                                                           
                                                                        
                                                                       
                                                                               
    private static bool RaySphereHit(Vector3 origin, Vector3 direction, Vector3 center, float radius, out float distance)
    {
        var toCenter = center - origin;
        float tClosest = Vector3.Dot(toCenter, direction);
        if (tClosest < 0f) { distance = 0f; return false; }

        var closestPoint = origin + direction * tClosest;
        float distSq = (closestPoint - center).LengthSquared;
        if (distSq > radius * radius) { distance = 0f; return false; }

        distance = tClosest;
        return true;
    }

                                                                             
                                                                            
                                                                        
                                                                       
                                                                            
    private int _nextEventNumber = 1;

                                                                            
                                                                        
                                                                            
                                                                             
                                                                        
                                                                        
                                                                           
                                                                             
                                                                          
                                                                           
                                                          
       
                                                                            
                                                                           
                                                                              
                                                          
    private void PlaceEventAtRay(Vector3 origin, Vector3 direction) =>
        PlaceTriggerAtRay(origin, direction, radius: 0.75f);

                                                                               
                                                                                   
                                                                             
                                                                                   
                               
    private void PlaceMoodZoneAtRay(Vector3 origin, Vector3 direction)
    {
        if (!TryResolvePlacementPoint(origin, direction, out var hitPoint)) return;

        var zone = new MoodZone(hitPoint, radius: 5f, targetValue: 50f, pullRatePerSecond: 5f, releaseSeconds: 3f);
        _level.AddMoodZone(zone);
        SelectMoodZone(zone);
    }

                                                                                 
                                                                                   
                                                                                 
                                                                                 
    private void PlaceMoodEventAtRay(Vector3 origin, Vector3 direction)
    {
        if (!TryResolvePlacementPoint(origin, direction, out var hitPoint)) return;

        var moodEvent = new MoodEvent(hitPoint, radius: 8f, MoodEventEffect.Ghost,
            cooldownSeconds: 120f, baseChance: 0.3f);
        _level.AddMoodEvent(moodEvent);
        SelectMoodEvent(moodEvent);
    }

                                                                         
                                                                      
                                                                             
                                                                                 
    private void PlaceTriggerAtRay(Vector3 origin, Vector3 direction, float radius = 2f)
    {
        if (!TryResolvePlacementPoint(origin, direction, out var hitPoint)) return;

        var trig = new TriggerZone(hitPoint, radius, TriggerType.OnInteract, new List<EventCommand>());
        _level.AddTrigger(trig);
        SelectTrigger(trig);
    }

                                                                                 
                                                                                
                                                                     
                                                                               
                                                                            
                                                     
       
                                                                            
                                                                       
                                                                              
                                                                                 
    private void PlaceAssetAt(string assetKey, Vector3 origin, Vector3 direction)
    {
        if (string.IsNullOrEmpty(assetKey)) return;

        if (assetKey.EndsWith(PrefabSuffix, StringComparison.OrdinalIgnoreCase))
        {
            if (TryResolvePlacementPoint(origin, direction, out var prefabPoint)) InstantiatePrefabAt(assetKey, prefabPoint);
            return;
        }

        bool isPrimitive = assetKey.StartsWith(EditorSceneIO.PrimitivePrefix, StringComparison.Ordinal);

                                                                            
                                                                                    
                                                                                    
                                                                                  
        if (!isPrimitive && IsPosterAsset(assetKey, out bool isVideo))
        {
            PlacePosterAt(assetKey, isVideo, origin, direction);
            return;
        }

        if (!TryResolvePlacementPoint(origin, direction, out var hitPoint)) return;

        var modelKey = isPrimitive ? assetKey : Path.GetFileNameWithoutExtension(assetKey);
        var model = EditorSceneIO.ResolveModel(_projectPath, modelKey, _modelCache);
        if (model == null) return;

        var instance = _level.AddModel(model, modelKey, hitPoint);
        if (isPrimitive) instance.IsSolid = true;

        _selectedModel = instance;
        _selectedTrigger = null;
        _selectedPoster = null;
        _selectedMoodZone = null;
        _selectedMoodEvent = null;
    }

                                                                                 
                                                                              
                                                                         
                                                                      
                                                                         
                                                                              
                                                                             
                                                                           
                            
    private static readonly HashSet<string> IgnoredVideoExtensions =
        new(StringComparer.OrdinalIgnoreCase) { ".txt", ".ini", ".db", ".ds_store", ".url", ".lnk" };

    private static bool IsPosterAsset(string path, out bool isVideo)
    {
        var dirName = Path.GetFileName(Path.GetDirectoryName(path)) ?? string.Empty;
        var ext = Path.GetExtension(path);

        if (dirName.Equals("Images", StringComparison.Ordinal) && ext.Equals(".png", StringComparison.OrdinalIgnoreCase))
        {
            isVideo = false;
            return true;
        }

        if (dirName.Equals("Videos", StringComparison.Ordinal) && !IgnoredVideoExtensions.Contains(ext))
        {
            isVideo = true;
            return true;
        }

        isVideo = false;
        return false;
    }

                                                                              
                                                                                  
                                                                           
                                                                                  
                                                                             
                                                                                        
    private void PlacePosterAt(string assetPath, bool isVideo, Vector3 origin, Vector3 direction)
    {
        const float reach = 20f;
        if (!_level.Raycast(origin, direction, reach, out var hitPoint, out var hitNormal)) return;

        var fileName = Path.GetFileName(assetPath);
        var poster = isVideo
            ? new PosterInstance(imagePath: null, videoPath: fileName, hitPoint, hitNormal)
            : new PosterInstance(imagePath: fileName, videoPath: null, hitPoint, hitNormal);
        _level.AddPoster(poster);

        _selectedPoster = poster;
        _selectedModel = null;
        _selectedTrigger = null;
        _selectedMoodZone = null;
        _selectedMoodEvent = null;
        _posterScaleBuffer = poster.Scale;
    }

                                                                          
                                                                           
                                                                        
                                                                  
                                                                         
                                                                        
                                                                               
                                                                          
                                                                             
                                                                              
                                                                 
    private void RenderEventModels(Shader shader)
    {
        foreach (var trig in _level.Triggers)
        {
            if (string.IsNullOrWhiteSpace(trig.MarkerPath)) continue;

            var model = EditorSceneIO.ResolveModel(_projectPath, trig.MarkerPath, _modelCache);
            if (model == null) continue;

            var matrix = Matrix4.CreateScale(trig.MarkerScale)
                * Matrix4.CreateRotationY(MathHelper.DegreesToRadians(trig.YawDegrees))
                * Matrix4.CreateTranslation(trig.Center);
            model.Render(shader, matrix);
        }
    }

    private void PlaceSelectedAsset(Vector3 origin, Vector3 direction) => PlaceAssetAt(_selectedAsset, origin, direction);

                                                                                                                    
    private void DrawAssetInspector(string assetKey)
    {
        bool isPrimitive = assetKey.StartsWith(EditorSceneIO.PrimitivePrefix, StringComparison.Ordinal);
        if (isPrimitive)
        {
            ImGui.Text(assetKey[EditorSceneIO.PrimitivePrefix.Length..].ToUpperInvariant());
            ImGui.TextDisabled("Built-in solid shape.");
        }
        else
        {
            ImGui.Text(Path.GetFileName(assetKey));
            ImGui.TextDisabled(Path.GetFileName(Path.GetDirectoryName(assetKey)) ?? string.Empty);
            ImGui.Separator();
            foreach (var line in EditorAssetInfo.Describe(assetKey)) ImGui.TextUnformatted(line);
        }

        ImGui.Separator();
        bool placeable = isPrimitive || IsPosterAsset(assetKey, out _) || assetKey.EndsWith(".glb", StringComparison.OrdinalIgnoreCase) || assetKey.EndsWith(PrefabSuffix, StringComparison.OrdinalIgnoreCase);
        if (placeable)
        {
            if (ImGui.Button("Place in front of camera"))
            {
                var (origin, direction) = CameraRay();
                PlaceAssetAt(assetKey, origin, direction);
            }
            Tip("Or drag this asset from Project into the Scene View.");
        }
        if (assetKey.EndsWith(".lua", StringComparison.OrdinalIgnoreCase) && ImGui.Button("Open in Script Editor"))
            OpenScriptInEditor(assetKey);
    }

                                                                                                                 
    private void DrawSceneInspector()
    {
        ImGui.Text($"Scene: {_sceneName}");
        ImGui.TextDisabled("Nothing selected - this is the scene itself.");
        ImGui.Separator();

        ImGui.TextUnformatted($"Objects: {_level.Models.Count}    Events: {_level.Triggers.Count}    Posters: {_level.Posters.Count}");
        ImGui.TextUnformatted($"Mood zones: {_level.MoodZones.Count}    Mood events: {_level.MoodEvents.Count}");
        ImGui.TextUnformatted(_level.PlayerStartPosition.HasValue ? "Player Start: set" : "Player Start: not set");

        ImGui.Spacing();
        if (ImGui.Button("Environment...")) _showEnvironmentWindow = true;
        Tip("Sky, ambient light, fog and colour grading of this scene.");
        ImGui.SameLine();
        if (ImGui.Button("Diagnostics...")) _showDiagnosticsWindow = true;
        ImGui.SameLine();
        if (ImGui.Button("Scenes...")) _showScenesPanel = true;

        ImGui.Spacing();
        ImGui.TextDisabled("Click an object in the Scene View or Hierarchy to edit its properties.");
    }
}
