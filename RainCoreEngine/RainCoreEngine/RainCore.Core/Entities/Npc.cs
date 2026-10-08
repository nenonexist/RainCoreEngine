using System.Drawing;
using OpenTK.Mathematics;

namespace RainCore;

                                                                                  
                                                                       
public enum MovementPattern
{
    Static,                                     
    RandomWander,                                                            
    PatrolRoute,                                      
    FaceCamera,                                                                 
}

             
                                                                            
                                                                            
   
                                                                            
                                                                          
                                                                             
                                                                              
                                                                               
                                                                      
                                                                     
                                                               
                                                      
              
public class Npc : IInteractable
{
    public string Name { get; }
    public Vector3 Position { get; private set; }

                                                                                                    
    public Label3D Label { get; } = new();

                                                                                                      
    public string ModelName { get; set; } = "";

                                                                                          
                                                                                            
                                                                  
    public float ModelScale { get; set; } = 1f;

                                                                                       
                                                                                    
                                                 
    public GlbModel? Model { get; set; }

                                                                                                     
    public float YawDegrees { get; private set; }

    public MovementPattern Movement { get; set; } = MovementPattern.Static;
                                                                                                  
    public List<Vector3> PatrolPoints { get; } = new();

                                                                  
                                                                            
                                                                               
                                                                            
                                                           
    private readonly MovementPatternRunner _movement;

                                                                           
                                                                               
                                                                             
                                                                              
                                                          
    private readonly List<EventCommand> _interactCommands;
    private readonly IReadOnlyList<EventPage>? _interactPages;
    private readonly EventCondition _interactCondition;
    private readonly Dictionary<string, bool> _selfSwitches = new(StringComparer.OrdinalIgnoreCase);
    private bool _interactHasFired;

                                                                              
                                                                       
    public bool InteractRunOnce { get; }

                                                                                    
                                                                                
                                                                 
    public bool HasInteractEvent => _interactCommands.Count > 0 || (_interactPages?.Count ?? 0) > 0;

                                                                                       
                                                                                       
                                                                                          
                                                                                 
    public IReadOnlyList<EventCommand> InteractCommands => _interactCommands;

                            
    public float InteractRadius => 2.5f;
    public string DisplayName => Name;

                 
                                                                                  
                                                                                       
                                                                                 
                                                                                  
                                                                              
                  
    public string? Interact() => null;

    public bool MoveBy(Vector3 offset)
    {
        Position += offset;
        return true;
    }

                                                                                          
                                                                               
                                                                                         
                                                                     
    public Npc(string name, Vector3 position, string[]? lines = null,
        string modelName = "", MovementPattern movement = MovementPattern.Static, Vector3[]? patrolPoints = null,
        float modelScale = 1f,
        List<EventCommand>? interactCommands = null,
        IReadOnlyList<EventPage>? interactPages = null,
        EventCondition? interactCondition = null,
        bool interactRunOnce = false)
    {
        Name = name;
        Position = position;
        ModelName = modelName;
        ModelScale = modelScale <= 0f ? 1f : modelScale;
        Movement = movement;
        _movement = new MovementPatternRunner(position);
        if (patrolPoints != null) PatrolPoints.AddRange(patrolPoints);

        _interactCommands = interactCommands ?? new List<EventCommand>();
        _interactPages = interactPages;
        _interactCondition = interactCondition ?? EventCondition.Always();
        InteractRunOnce = interactRunOnce;
    }

    public bool GetSelfSwitch(string name)
    {
        return _selfSwitches.TryGetValue(name, out var value) && value;
    }

    public void SetSelfSwitch(string name, bool value)
    {
        _selfSwitches[name] = value;
    }

    private bool InteractConditionMet(IEventContext ctx)
    {
        return _interactCondition.Evaluate(new ConditionContext(this, ctx));
    }

    private IReadOnlyList<EventCommand> ActiveInteractCommands(IEventContext ctx)
    {
        if (_interactPages != null)
        {
            var context = new ConditionContext(this, ctx);
            var page = _interactPages.FirstOrDefault(candidate => candidate.Condition.Evaluate(context));
            if (page != null) return page.Commands;
        }
        return _interactCommands;
    }

    private IEventContext ScopedInteractContext(IEventContext ctx)
    {
        return new NpcInteractContext(ctx, this);
    }

                 
                                                                              
                                                                                    
                                                                              
                                                                
                  
    public bool TryRunInteractEvent(EventRunner runner, IEventContext ctx)
    {
        if (!HasInteractEvent) return false;
        if (InteractRunOnce && _interactHasFired) return false;
        if (runner.IsBusy) return false;
        if (!InteractConditionMet(ctx)) return false;

        runner.Start(ActiveInteractCommands(ctx), ScopedInteractContext(ctx));
        _interactHasFired = true;
        return true;
    }

                 
                                                                           
                                                                                   
                                                                                   
                                                                                 
                                     
                  
    public void UpdateLabel(double deltaSeconds)
    {
        Label.SetText(Name, Color.FromArgb(255, 190, 210, 255));                            
    }

                 
                                                                              
                                                                          
                                                                        
       
                                                                                
                                                                               
                                                                           
                                                                               
                                                                           
                                                                           
                                                                   
    public void UpdateMovement(double dt, ILevel level, Vector3 playerPos)
    {
        var position = Position;
        var yaw = YawDegrees;
        _movement.Update(dt, Movement, PatrolPoints, level, playerPos, ref position, ref yaw);
        Position = position;
        YawDegrees = yaw;
    }

    private sealed class ConditionContext : IEventConditionContext
    {
        private readonly Npc _npc;
        private readonly IEventContext _ctx;

        public ConditionContext(Npc npc, IEventContext ctx)
        {
            _npc = npc;
            _ctx = ctx;
        }

        public bool GetGlobalSwitch(string name) => GameFlags.Get(name);
        public bool GetSelfSwitch(string name) => _npc.GetSelfSwitch(name);
        public int GetVariable(string name) => GameFlags.GetVar(name);
        public Vector3 GetCameraPosition() => _ctx.GetCameraPosition();
        public Vector3 GetCameraForward() => _ctx.GetCameraForward();
        public Vector3? GetNpcPosition(string name) => _ctx.GetNpcPosition(name);
        public MoodLevel GetMoodLevel() => MoodState.Level;
        public float GetMoodValue() => MoodState.Value;
    }

    private sealed class NpcInteractContext : IEventContext, ITriggerSelfSwitchContext
    {
        private readonly IEventContext _inner;
        private readonly Npc _npc;

        public NpcInteractContext(IEventContext inner, Npc npc)
        {
            _inner = inner;
            _npc = npc;
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
        public void SetSelfSwitch(string name, bool value) => _npc.SetSelfSwitch(name, value);
        public Vector3 GetCameraPosition() => _inner.GetCameraPosition();
        public Vector3 GetCameraForward() => _inner.GetCameraForward();
        public Vector3? GetNpcPosition(string name) => _inner.GetNpcPosition(name);
    }
}
