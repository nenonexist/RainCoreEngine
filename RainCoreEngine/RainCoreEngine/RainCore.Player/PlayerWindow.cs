using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;
using OpenTK.Windowing.GraphicsLibraryFramework;

namespace RainCore.Player;

             
                                                                            
                                                                            
                                                                          
                                                                             
                                                                      
   
                                                                             
                                                           
                                                                      
                                                                           
                                                                               
                                                                               
                                                                     
   
                                                                     
                                                                            
                                                                   
                                                                           
   
                                                              
                                                                      
                                                                           
                                                  
                                                                           
                                                                          
              
public sealed class PlayerWindow : GameWindow, IMoodEventSink
{
    private const int LowResWidth = 1024;
    private const int LowResHeight = 480;

    private readonly string _projectPath;
    private readonly ProjectDescriptor _project;

    private Shader _shader = null!;
    private Shader _blitShader = null!;
    private int _lowResFbo, _lowResColorTex, _lowResDepthRbo;
    private int _blitVao, _blitVbo;

    private MeshRoomLevel _level = null!;
    private Camera _camera = null!;
    private readonly ICameraRig _cameraRig = new FirstPersonCameraRig();
    private readonly Dictionary<string, GlbModel> _modelCache = new();

    private readonly EventRunner _eventRunner = new();
    private IEventContext _eventContext = null!;
    private readonly DialogueState _dialogueState = new();
                                                                           
                                                                         
                                                                        
    private GameDatabase _database = new();
    private readonly PartyState _party = new();
    private readonly BattleState _battle = new();
    private string? _battleMenuPrintedFor;                                                                                      
    private int _battleLogPrinted;
    private DialogueOverlay _dialogueOverlay = null!;

    private Vector3 _playerPosition;
    private Vector2 _lastMouse;
    private bool _firstMouse = true;

                                                                  
                                                                              
                                                                                
                                                                              
                                                                        
                                                                          
    private readonly AudioManager _audio = new();
    private ScriptHost _scripts = null!;
    private readonly MoodTracker _moodTracker = new();
    private readonly Random _moodRng = new();

    private readonly List<string> _log = new();

                                                                        
                                                                            
                                                                              
                                                                           
                                                                           
                                                                            
                                                                                  
    private IGameModule? _gameModule;

    public PlayerWindow(GameWindowSettings gws, NativeWindowSettings nws, string projectPath, ProjectDescriptor project)
        : base(gws, nws)
    {
        _projectPath = projectPath;
        _project = project;
    }

    protected override void OnLoad()
    {
        base.OnLoad();

        GL.ClearColor(0.05f, 0.05f, 0.07f, 1f);
        GL.Enable(EnableCap.DepthTest);

        _audio.Init();
        _scripts = new ScriptHost(_projectPath, LogLine);

        var shadersDir = Path.Combine(AppContext.BaseDirectory, "Shaders");
        _shader = new Shader(Path.Combine(shadersDir, "shader.vert"), Path.Combine(shadersDir, "shader.frag"));
        _blitShader = new Shader(Path.Combine(shadersDir, "blit.vert"), Path.Combine(shadersDir, "blit.frag"));
        _dialogueOverlay = new DialogueOverlay(shadersDir);
        BuildLowResTarget();
        BuildBlitQuad();

        var scenes = ProjectDescriptor.ListScenes(_projectPath);
        var startScene = scenes.Contains(_project.StartScene, StringComparer.OrdinalIgnoreCase)
            ? _project.StartScene
            : scenes.FirstOrDefault() ?? _project.StartScene;

        _level = EditorSceneIO.LoadOrCreate(_projectPath, startScene, _modelCache, out _);
        if (scenes.Count == 0)
            Console.WriteLine($"[Player] В \"{_projectPath}/Scenes\" нет сцен - запущена пустая \"{startScene}\".");

        _gameModule = _project.TryLoadGameModule(_projectPath);
        _gameModule?.Initialize(_level, _projectPath);

                                                                              
                                                                           
                                            
        _database = GameDatabase.Load(_projectPath);
        _party.InitializeFromDatabase(_database);

                                                                     
                                                                            
                                                                          
                                                                            
                                                                
                                                             
        if (_level.PlayerStartPosition is { } start)
        {
            _playerPosition = start;
            _camera = new Camera(start + Vector3.UnitY * 1.6f, yaw: _level.PlayerStartYawDegrees - 90f, pitch: 0f)
            {
                AspectRatio = LowResWidth / (float)LowResHeight,
            };
        }
        else
        {
            _camera = new Camera(new Vector3(0, 1.6f, 3), yaw: -90f, pitch: -10f)
            {
                AspectRatio = LowResWidth / (float)LowResHeight,
            };
            _playerPosition = ComputeStartPosition();
        }

        _eventContext = new PlayerEventContext(pos => _playerPosition = pos, _level, _camera, LogLine,
            _dialogueState, _battle, _database, _party, _audio, _scripts);

        CursorState = CursorState.Grabbed;
        Console.WriteLine($"[Player] {_project.Name} - сцена \"{startScene}\" загружена. WASD - движение, ПКМ - поворот камеры, E/F - события, 1-9 - выбор варианта, Esc - выход.");
    }

    private Vector3 ComputeStartPosition()
    {
        var probe = _camera.Position;
        if (_level.Raycast(probe + Vector3.UnitY * 5f, -Vector3.UnitY, 50f, out var floorHit, out _))
            return floorHit + Vector3.UnitY * 0.05f;
        return new Vector3(probe.X, 0f, probe.Z);
    }

    private void LogLine(string line)
    {
        Console.WriteLine($"[Player] {line}");
        _log.Add(line);
        if (_log.Count > 6) _log.RemoveAt(0);
    }

                                                  
                                                                   
                                                                             
                                      
    public void SpawnGhost(Vector3 worldPosition) =>
        LogLine($"(призрак у {worldPosition.X:0.0}, {worldPosition.Y:0.0}, {worldPosition.Z:0.0} - визуал ещё не подключён)");

    public void PlayStingerSound(string soundName) => _audio.PlaySound(soundName);

                                                                              
                                                                        
                                                                         
                                                                        
                                                                           
                         
    private void HandleBattleInput(KeyboardState kb)
    {
        if (_battle.Outcome is { } outcome)
        {
            var key = $"outcome:{outcome}";
            if (_battleMenuPrintedFor != key)
            {
                _battleMenuPrintedFor = key;
                LogLine(outcome switch
                {
                    BattleOutcome.Victory => "Victory! Press 1 to continue.",
                    BattleOutcome.Defeat => "Defeat... Press 1 to continue.",
                    _ => "Escaped. Press 1 to continue.",
                });
            }
            if (kb.IsKeyPressed(Keys.D1))
            {
                _battle.ConsumeOutcome();
                _battleMenuPrintedFor = null;
                _battleLogPrinted = 0;
            }
            return;
        }

        var actor = _battle.AwaitingActionFrom;
        if (actor == null) { _battleMenuPrintedFor = null; return; }

        var menuKey = $"turn:{actor.Id}";
        if (_battleMenuPrintedFor != menuKey)
        {
            _battleMenuPrintedFor = menuKey;
            var options = new List<string> { "Attack" };
            foreach (var skillId in actor.SkillIds)
            {
                var skill = _database.FindSkill(skillId);
                if (skill != null) options.Add($"{skill.Name} ({skill.MpCost} MP)");
            }
            options.Add("Flee");
            LogLine($"{actor.Name}'s turn - " + string.Join("  ", options.Select((o, i) => $"{i + 1}:{o}")));
        }

        var skillIds = actor.SkillIds;
        for (int i = 0; i < skillIds.Count + 2 && i < 9; i++)
        {
            if (!kb.IsKeyPressed(Keys.D1 + i)) continue;

            if (i == 0) _battle.SubmitPlayerAction(BattleActionKind.Attack);
            else if (i == skillIds.Count + 1) _battle.SubmitPlayerAction(BattleActionKind.Flee);
            else _battle.SubmitPlayerAction(BattleActionKind.Skill, skillIds[i - 1]);

            _battleMenuPrintedFor = null;
            break;
        }
    }

    protected override void OnResize(ResizeEventArgs e)
    {
        base.OnResize(e);
        GL.Viewport(0, 0, FramebufferSize.X, FramebufferSize.Y);
    }

    protected override void OnUnload()
    {
        _audio.Shutdown();
        base.OnUnload();
    }

    protected override void OnUpdateFrame(FrameEventArgs args)
    {
        base.OnUpdateFrame(args);
        var dt = (float)args.Time;
        var kb = KeyboardState;
        var mouse = MouseState;

        if (kb.IsKeyPressed(Keys.Escape)) Close();

                                                                         
                                                                         
                                                                     
                                                                             
                                                                   
                                                                      
                                             
        if (_battle.IsActive || _battle.Outcome != null)
        {
            HandleBattleInput(kb);
            _battle.Tick(dt);
            _eventRunner.Tick(dt);                                                                                                                  

                                                                                  
                                                                            
                                                                              
                                         
            for (; _battleLogPrinted < _battle.Log.Count; _battleLogPrinted++)
                LogLine(_battle.Log[_battleLogPrinted]);

            return;
        }

                                                                              
                                                                             
                                                                              
                                                                            
                                                                           
                                                                               
        if (_dialogueState.IsChoicePending)
        {
            var options = _dialogueState.ChoiceOptions!;
            for (int i = 0; i < options.Count && i < 9; i++)
            {
                if (kb.IsKeyPressed(Keys.D1 + i))
                {
                    _dialogueState.ResolveChoice(i);
                    break;
                }
            }
            _eventRunner.Tick(dt);
            return;
        }

                                                                            
                                                                             
                                                                            
        bool eventBusy = _eventRunner.IsBusy;

        float yawDelta = 0f, pitchDelta = 0f;
        if (_firstMouse) { _lastMouse = mouse.Position; _firstMouse = false; }
        else
        {
            var delta = mouse.Position - _lastMouse;
            _lastMouse = mouse.Position;
            const float sensitivity = 0.12f;
            yawDelta = delta.X * sensitivity;
            pitchDelta = -delta.Y * sensitivity;
        }

        var forward = new Vector3(_camera.Front.X, 0, _camera.Front.Z);
        if (forward.LengthSquared > 0f) forward = Vector3.Normalize(forward);
        var right = new Vector3(_camera.Right.X, 0, _camera.Right.Z);
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

        _cameraRig.Update(dt, _camera, _playerPosition, yawDelta, pitchDelta, _level);
        _eventRunner.Tick(dt);

                                                                      
                                                                        
                                                                       
        if (!_eventRunner.IsBusy)
            _dialogueState.ClearDialogue();

                                                                              
                                                                                    
        foreach (var trig in _level.Triggers)
            trig.UpdateMovement(dt, _level, _playerPosition);

                                                                                     
                                                                                    
                                                                                      
        float horizontalSpeed = dt > 0f ? move.Length / dt : 0f;
        _moodTracker.Update(dt, horizontalSpeed);
        _audio.Update(dt);
        foreach (var moodEvent in _level.MoodEvents)
            moodEvent.Update(dt, _playerPosition, _moodTracker, this, _moodRng);

                                                                               
                                                                          
                                                                               
                                                                             
                                                                             
                                                         
        _gameModule?.Update(dt);

        if (!eventBusy && kb.IsKeyPressed(Keys.F))
        {
            var trigger = _level.Triggers.FirstOrDefault(t => t.Type == TriggerType.OnInteract
                && (t.Center - _playerPosition).Length <= t.Radius);
            trigger?.TryInteract(_playerPosition, _eventRunner, _eventContext);
        }

        if (!eventBusy && kb.IsKeyPressed(Keys.E))
        {
            var target = _level.Interactables
                .Where(o => (o.Position - _playerPosition).Length <= o.InteractRadius)
                .OrderBy(o => (o.Position - _playerPosition).Length)
                .FirstOrDefault();
            var result = target?.Interact();
            if (!string.IsNullOrEmpty(result)) LogLine(result!);
        }
    }

    protected override void OnRenderFrame(FrameEventArgs args)
    {
        base.OnRenderFrame(args);

        GL.BindFramebuffer(FramebufferTarget.Framebuffer, _lowResFbo);
        GL.Viewport(0, 0, LowResWidth, LowResHeight);

                                                                                                 
                                                                                                        
        var environment = _level.Environment;
        GL.ClearColor(environment.Background.X, environment.Background.Y, environment.Background.Z, 1f);
        GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

        _shader.Use();
        _shader.SetMatrix4("view", _camera.GetViewMatrix());
        _shader.SetMatrix4("projection", _camera.GetProjectionMatrix());
        environment.ApplyTo(_shader);
        foreach (var model in _level.Models)
            model.Render(_shader);
        RenderEventModels();

                                                                               
                                                                              
                                                                             
                                                                                 
        _gameModule?.Render();

                                                                              
                                                                          
                                                                            
                                                                              
                                                             
        _dialogueOverlay.Draw(_dialogueState);

        GL.BindFramebuffer(FramebufferTarget.Framebuffer, 0);
        GL.Viewport(0, 0, FramebufferSize.X, FramebufferSize.Y);
        GL.Disable(EnableCap.DepthTest);
        GL.Clear(ClearBufferMask.ColorBufferBit);

        _blitShader.Use();
        GL.ActiveTexture(TextureUnit.Texture0);
        GL.BindTexture(TextureTarget.Texture2D, _lowResColorTex);
        _blitShader.SetInt("uScene", 0);
        _blitShader.SetVector2("uScreenSize", new Vector2(LowResWidth, LowResHeight));
        _blitShader.SetFloat("uDitherStrength", environment.DitherStrength);
        _blitShader.SetFloat("uDitherLevels", MathF.Max(environment.DitherLevels, 2f));
        GL.BindVertexArray(_blitVao);
        GL.DrawArrays(PrimitiveType.Triangles, 0, 6);
        GL.Enable(EnableCap.DepthTest);

        SwapBuffers();
    }

                                                                              
                                                                        
                                                                                
                                                                                        
    private void RenderEventModels()
    {
        foreach (var trig in _level.Triggers)
        {
            if (string.IsNullOrWhiteSpace(trig.MarkerPath)) continue;

            var model = EditorSceneIO.ResolveModel(_projectPath, trig.MarkerPath, _modelCache);
            if (model == null) continue;

            var matrix = Matrix4.CreateScale(trig.MarkerScale)
                * Matrix4.CreateRotationY(MathHelper.DegreesToRadians(trig.YawDegrees))
                * Matrix4.CreateTranslation(trig.Center);
            model.Render(_shader, matrix);
        }
    }

    private void BuildLowResTarget()
    {
        _lowResColorTex = GL.GenTexture();
        GL.BindTexture(TextureTarget.Texture2D, _lowResColorTex);
        GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgb,
            LowResWidth, LowResHeight, 0, PixelFormat.Rgb, PixelType.UnsignedByte, IntPtr.Zero);
        GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Nearest);
        GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Nearest);
        GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)TextureWrapMode.ClampToEdge);
        GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)TextureWrapMode.ClampToEdge);

        _lowResDepthRbo = GL.GenRenderbuffer();
        GL.BindRenderbuffer(RenderbufferTarget.Renderbuffer, _lowResDepthRbo);
        GL.RenderbufferStorage(RenderbufferTarget.Renderbuffer, RenderbufferStorage.DepthComponent24, LowResWidth, LowResHeight);

        _lowResFbo = GL.GenFramebuffer();
        GL.BindFramebuffer(FramebufferTarget.Framebuffer, _lowResFbo);
        GL.FramebufferTexture2D(FramebufferTarget.Framebuffer, FramebufferAttachment.ColorAttachment0, TextureTarget.Texture2D, _lowResColorTex, 0);
        GL.FramebufferRenderbuffer(FramebufferTarget.Framebuffer, FramebufferAttachment.DepthAttachment, RenderbufferTarget.Renderbuffer, _lowResDepthRbo);

        var status = GL.CheckFramebufferStatus(FramebufferTarget.Framebuffer);
        if (status != FramebufferErrorCode.FramebufferComplete)
            Console.WriteLine($"[Player] Внимание: низкоразрешённый framebuffer не собрался ({status}).");

        GL.BindFramebuffer(FramebufferTarget.Framebuffer, 0);
    }

    private void BuildBlitQuad()
    {
        float[] vertices =
        {
            -1f, -1f,   0f, 0f,
             1f, -1f,   1f, 0f,
             1f,  1f,   1f, 1f,
            -1f, -1f,   0f, 0f,
             1f,  1f,   1f, 1f,
            -1f,  1f,   0f, 1f,
        };

        _blitVao = GL.GenVertexArray();
        _blitVbo = GL.GenBuffer();
        GL.BindVertexArray(_blitVao);
        GL.BindBuffer(BufferTarget.ArrayBuffer, _blitVbo);
        GL.BufferData(BufferTarget.ArrayBuffer, vertices.Length * sizeof(float), vertices, BufferUsageHint.StaticDraw);

        int stride = 4 * sizeof(float);
        GL.VertexAttribPointer(0, 2, VertexAttribPointerType.Float, false, stride, 0);
        GL.EnableVertexAttribArray(0);
        GL.VertexAttribPointer(1, 2, VertexAttribPointerType.Float, false, stride, 2 * sizeof(float));
        GL.EnableVertexAttribArray(1);
    }
}
