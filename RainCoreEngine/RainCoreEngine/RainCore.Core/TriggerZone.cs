using OpenTK.Mathematics;

namespace RainCore;

public enum TriggerType
{
    OnEnter,                                                                        
    OnInteract,                                                
    Parallel,                                                                                            
                                                                                                                         

    Auto,                                                                                  

                                                                                 
                                                                         
                                                                             
                                                                                  
    Door,
}

             
                                                                           
                                                                          
                                                                  
              
public class TriggerZone
{
                                                                          
                                                                           
                                                                           
                                                                             
                                                                              
                                                        
    public Vector3 Center { get; private set; }
    public float Radius { get; }
    public TriggerType Type { get; }
    public string? RequiredFlag { get; }
    public bool RequiredFlagValue { get; }
    public EventCondition Condition { get; }
    public bool RunOnce { get; }

                                                                                                                      
    public string MarkerPath { get; }

                                                                                          
                                                                                        
                                             
    public float MarkerScale { get; }

                                                                                  
                                                                                 
                                                                   
    public float YawDegrees { get; private set; }

                                                                            
                                                                        
                                                                              
                                                                                    
    public MovementPattern Movement { get; }
    public IReadOnlyList<Vector3> PatrolPoints { get; }
    private readonly MovementPatternRunner _movement;

                                                                                                                       
    public float ChancePerCheck { get; }
                                                                                                      
    public float CheckIntervalSeconds { get; }

                                                                                  
                                                                                    
                                                                       
                                                       
    public string DoorInteriorId { get; }

                                                                                   
                                                                                  
                                                                
    public int DoorWidth { get; }
    public int DoorHeight { get; }
    public int DoorDepth { get; }

    private readonly List<EventCommand> _commands;
    private readonly IReadOnlyList<EventPage>? _pages;

                                                                                   
                                                                            
                                                                                       
    public IReadOnlyList<EventCommand> Commands => _commands;

                                                                                 
                                                                                  
                                                                                      
                                                                                          
    public IReadOnlyList<EventPage> Pages => _pages ?? Array.Empty<EventPage>();
    private bool _hasFired;
    private bool _playerWasInside;
    private float _parallelElapsed;
    private readonly Dictionary<string, bool> _selfSwitches = new(StringComparer.OrdinalIgnoreCase);

    public TriggerZone(Vector3 center, float radius, TriggerType type,
        List<EventCommand> commands, string? requiredFlag = null, bool requiredFlagValue = true, bool runOnce = true,
        float chancePerCheck = 1f, float checkIntervalSeconds = 5f, EventCondition? condition = null,
        IReadOnlyList<EventPage>? pages = null, string markerPath = "", float markerScale = 1f,
        string doorInteriorId = "", int doorWidth = 9, int doorHeight = 5, int doorDepth = 9,
        MovementPattern movement = MovementPattern.Static, IReadOnlyList<Vector3>? patrolPoints = null)
    {
        Center = center;
        Radius = radius;
        Type = type;
        _commands = commands;
        _pages = pages;
        RequiredFlag = requiredFlag;
        RequiredFlagValue = requiredFlagValue;
        Condition = condition ?? (requiredFlag == null
            ? EventCondition.Always()
            : EventCondition.GlobalSwitch(requiredFlag, requiredFlagValue));
        RunOnce = runOnce;
        ChancePerCheck = chancePerCheck;
        CheckIntervalSeconds = checkIntervalSeconds;
        MarkerPath = markerPath;
        MarkerScale = markerScale <= 0f ? 1f : markerScale;
        DoorInteriorId = doorInteriorId;
        DoorWidth = doorWidth;
        DoorHeight = doorHeight;
        DoorDepth = doorDepth;
        Movement = movement;
        PatrolPoints = patrolPoints ?? Array.Empty<Vector3>();
        _movement = new MovementPatternRunner(center);
    }

                                                                                
                                                                               
                                                                               
                                                                              
                                                                              
                                                      
    public void UpdateMovement(double dt, ILevel level, Vector3 playerPos)
    {
        if (Movement == MovementPattern.Static) return;

        var center = Center;
        var yaw = YawDegrees;
        _movement.Update(dt, Movement, PatrolPoints, level, playerPos, ref center, ref yaw);
        Center = center;
        YawDegrees = yaw;
    }

                                                                   
                                                                
                                                                            
                                                                                
                                                                            
                                                                                     
    public void SetCenter(Vector3 center) => Center = center;

    public bool GetSelfSwitch(string name)
    {
        return _selfSwitches.TryGetValue(name, out var value) && value;
    }

    public void SetSelfSwitch(string name, bool value)
    {
        _selfSwitches[name] = value;
    }

    private bool ConditionMet(IEventContext ctx)
    {
        return Condition.Evaluate(new ConditionContext(this, ctx));
    }

    private IReadOnlyList<EventCommand> ActiveCommands(IEventContext ctx)
    {
        if (_pages != null)
        {
            var context = new ConditionContext(this, ctx);
            var page = _pages.FirstOrDefault(candidate => candidate.Condition.Evaluate(context));
            if (page != null) return page.Commands;
        }
        return _commands;
    }

    private IEventContext ScopedContext(IEventContext ctx)
    {
        return new TriggerContext(ctx, this);
    }


                                                                                                             
    public void CheckEnter(Vector3 playerPos, EventRunner runner, IEventContext ctx)
    {
        if (Type != TriggerType.OnEnter) return;
        if (RunOnce && _hasFired) return;

        bool inside = (playerPos - Center).LengthFast <= Radius;
        if (inside && !_playerWasInside && ConditionMet(ctx) && !runner.IsBusy)
        {
            runner.Start(ActiveCommands(ctx), ScopedContext(ctx));
            _hasFired = true;
        }
        _playerWasInside = inside;
    }

                                                                                         
    public bool TryInteract(Vector3 playerPos, EventRunner runner, IEventContext ctx)
    {
        if (Type != TriggerType.OnInteract) return false;
        if (RunOnce && _hasFired) return false;
        if ((playerPos - Center).LengthFast > Radius) return false;
        if (!ConditionMet(ctx) || runner.IsBusy) return false;

        runner.Start(ActiveCommands(ctx), ScopedContext(ctx));
        _hasFired = true;
        return true;
    }

    private static readonly Random _rng = new();

                 
                                                                                
                                                                               
                                                                                  
                                                                                  
                                                                                
                                                                              
                  
    public void CheckParallel(Vector3 playerPos, float dt, BackgroundEventRunner background, IEventContext ctx)
    {
        if (Type != TriggerType.Parallel) return;
        if (RunOnce && _hasFired) return;
        if (CheckIntervalSeconds <= 0f) return;

        _parallelElapsed += dt;
        if (_parallelElapsed < CheckIntervalSeconds) return;
        _parallelElapsed = 0f;

        if (Radius > 0f && (playerPos - Center).LengthFast > Radius) return;
        if (!ConditionMet(ctx)) return;

        if (_rng.NextDouble() > ChancePerCheck) return;

        background.Start(ActiveCommands(ctx), ScopedContext(ctx));
        _hasFired = true;
    }

                 
                                                                              
                                                                          
                                                                                
                  
    public void CheckAuto(BackgroundEventRunner background, IEventContext ctx)
    {
        if (Type != TriggerType.Auto) return;
        if (_hasFired) return;
        if (!ConditionMet(ctx)) return;

        background.Start(ActiveCommands(ctx), ScopedContext(ctx));
        _hasFired = true;
    }

                                                                                     
                                                                                     
                                                                                    
                                                                                     
                                                                                                
    public bool CheckDoorEnter(Vector3 playerPos)
    {
        if (Type != TriggerType.Door) return false;
        bool inside = (playerPos - Center).LengthFast <= Radius;
        bool triggered = inside && !_playerWasInside;
        _playerWasInside = inside;
        return triggered;
    }

    private sealed class ConditionContext : IEventConditionContext
    {
        private readonly TriggerZone _trigger;
        private readonly IEventContext _ctx;

        public ConditionContext(TriggerZone trigger, IEventContext ctx)
        {
            _trigger = trigger;
            _ctx = ctx;
        }

        public bool GetGlobalSwitch(string name) => GameFlags.Get(name);
        public bool GetSelfSwitch(string name) => _trigger.GetSelfSwitch(name);
        public int GetVariable(string name) => GameFlags.GetVar(name);
        public Vector3 GetCameraPosition() => _ctx.GetCameraPosition();
        public Vector3 GetCameraForward() => _ctx.GetCameraForward();
        public Vector3? GetNpcPosition(string name) => _ctx.GetNpcPosition(name);
        public MoodLevel GetMoodLevel() => MoodState.Level;
        public float GetMoodValue() => MoodState.Value;
    }

    private sealed class TriggerContext : IEventContext, ITriggerSelfSwitchContext
    {
        private readonly IEventContext _inner;
        private readonly TriggerZone _trigger;

        public TriggerContext(IEventContext inner, TriggerZone trigger)
        {
            _inner = inner;
            _trigger = trigger;
        }

        public void ShowDialogue(string speaker, string text) => _inner.ShowDialogue(speaker, text);
        public void ShowNote(string title, string text) => _inner.ShowNote(title, text);
        public void PlaySound(string name) => _inner.PlaySound(name);
        public void RunScript(string scriptFile, string functionName, params string[] args) => _inner.RunScript(scriptFile, functionName, args);
        public void TeleportPlayer(Vector3 pos) => _inner.TeleportPlayer(pos);
        public void SetWeather(bool active) => _inner.SetWeather(active);
        public void AddNpc(Npc npc) => _inner.AddNpc(npc);
        public void RemoveNpc(Npc npc) => _inner.RemoveNpc(npc);
        public void AddItem(string name, int amount) => _inner.AddItem(name, amount);
        public bool RemoveItem(string name, int amount) => _inner.RemoveItem(name, amount);
        public void WakeFromRandomDream() => _inner.WakeFromRandomDream();
        public void BeginChoice(string prompt, IReadOnlyList<string> choices) => _inner.BeginChoice(prompt, choices);
        public bool IsChoicePending => _inner.IsChoicePending;
        public int? ConsumeChoice() => _inner.ConsumeChoice();
        public bool TryMoveNpc(string name, Vector3 offset) => _inner.TryMoveNpc(name, offset);
        public void FadeScreen(bool fadeOut, float seconds) => _inner.FadeScreen(fadeOut, seconds);
        public void FlashScreen(float seconds) => _inner.FlashScreen(seconds);
        public void ShakeScreen(float seconds, float strength) => _inner.ShakeScreen(seconds, strength);
        public void ShowPicture(int id, string fileName, float x, float y, float scale, float opacity) => _inner.ShowPicture(id, fileName, x, y, scale, opacity);
        public void HidePicture(int id) => _inner.HidePicture(id);
        public bool IsBattleActive => _inner.IsBattleActive;
        public bool StartBattle(string troopId) => _inner.StartBattle(troopId);
        public void SetSelfSwitch(string name, bool value) => _trigger.SetSelfSwitch(name, value);
        public Vector3 GetCameraPosition() => _inner.GetCameraPosition();
        public Vector3 GetCameraForward() => _inner.GetCameraForward();
        public Vector3? GetNpcPosition(string name) => _inner.GetNpcPosition(name);
    }
}
