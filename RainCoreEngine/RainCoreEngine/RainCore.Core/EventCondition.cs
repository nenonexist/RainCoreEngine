namespace RainCore;

using OpenTK.Mathematics;

                                                                                  
public interface IEventConditionContext
{
    bool GetGlobalSwitch(string name);
    bool GetSelfSwitch(string name);
    int GetVariable(string name);

                                                                
    Vector3 GetCameraPosition();
                                                                             
    Vector3 GetCameraForward();
                                                                                           
    Vector3? GetNpcPosition(string name);
                                                                                        
                                                                                
                                                                                            
    MoodLevel GetMoodLevel();
                                                                                        
                                                                                    
                                                   
    float GetMoodValue();
}

                                                                                               
public interface ITriggerSelfSwitchContext
{
    void SetSelfSwitch(string name, bool value);
}

             
                                                                           
                                                                            
              
public abstract class EventCondition
{
    public abstract bool Evaluate(IEventConditionContext context);

    public static EventCondition Always() => new PredicateCondition(_ => true);

    public static EventCondition GlobalSwitch(string name, bool expected = true) =>
        new PredicateCondition(context => context.GetGlobalSwitch(name) == expected);

    public static EventCondition SelfSwitch(string name, bool expected = true) =>
        new PredicateCondition(context => context.GetSelfSwitch(name) == expected);

    public static EventCondition Variable(string name, VariableComparison comparison, int expected) =>
        new PredicateCondition(context => comparison.Matches(context.GetVariable(name), expected));

                 
                                                                           
                                                                             
                                                                        
                  
    public static EventCondition HasItem(string itemName, int amount = 1) =>
        new PredicateCondition(_ => Inventory.Get(itemName) >= amount);

                 
                                                                        
                                                                          
                                                                          
                                                                                
                                                                          
                                                                                 
                  
    public static EventCondition LookAt(Vector3 targetPosition, float maxAngleDegrees = 20f, float maxDistance = 0f) =>
        new PredicateCondition(context =>
            IsLookingAt(context.GetCameraPosition(), context.GetCameraForward(), targetPosition, maxAngleDegrees, maxDistance));

                 
                                                                          
                                                                             
                                                                         
                                                       
                  
    public static EventCondition LookAtNpc(string npcName, float maxAngleDegrees = 20f, float maxDistance = 0f) =>
        new PredicateCondition(context =>
        {
            var target = context.GetNpcPosition(npcName);
            return target.HasValue &&
                   IsLookingAt(context.GetCameraPosition(), context.GetCameraForward(), target.Value, maxAngleDegrees, maxDistance);
        });

    private static bool IsLookingAt(Vector3 cameraPos, Vector3 cameraForward, Vector3 target, float maxAngleDegrees, float maxDistance)
    {
        var toTarget = target - cameraPos;
        var distance = toTarget.Length;
        if (distance < 0.0001f) return true;                                 
        if (maxDistance > 0f && distance > maxDistance) return false;

        var forward = cameraForward.LengthSquared > 0.0001f ? Vector3.Normalize(cameraForward) : cameraForward;
        var dot = Vector3.Dot(forward, toTarget / distance);
        var angleDeg = MathHelper.RadiansToDegrees(MathF.Acos(Math.Clamp(dot, -1f, 1f)));
        return angleDeg <= maxAngleDegrees;
    }

                 
                                                                                     
                                                                                 
                                                                                
                                                    
                  
    public static EventCondition Mood(MoodLevel expectedLevel) =>
        new PredicateCondition(context => context.GetMoodLevel() == expectedLevel);

                 
                                                                                    
                                                                                    
                                                                                     
                  
    public static EventCondition Mood(VariableComparison comparison, float expectedValue) =>
        new PredicateCondition(context => comparison.Matches(context.GetMoodValue(), expectedValue));

                 
                                                                                      
                                                                                  
                                                                                  
                                                                            
                  
    public static EventCondition QuestStatus(string questId, QuestState expected, int? step = null) =>
        new PredicateCondition(_ =>
            QuestTracker.GetState(questId) == expected &&
            (step == null || QuestTracker.GetStep(questId) == step.Value));

    public static EventCondition All(params EventCondition[] conditions) =>
        new CompositeCondition(conditions, requireAll: true);

    public static EventCondition Any(params EventCondition[] conditions) =>
        new CompositeCondition(conditions, requireAll: false);

    public static EventCondition Not(EventCondition condition) =>
        new NotCondition(condition);

    private sealed class PredicateCondition : EventCondition
    {
        private readonly Func<IEventConditionContext, bool> _predicate;

        public PredicateCondition(Func<IEventConditionContext, bool> predicate)
        {
            _predicate = predicate;
        }

        public override bool Evaluate(IEventConditionContext context) => _predicate(context);
    }

    private sealed class CompositeCondition : EventCondition
    {
        private readonly EventCondition[] _conditions;
        private readonly bool _requireAll;

        public CompositeCondition(EventCondition[] conditions, bool requireAll)
        {
            _conditions = conditions;
            _requireAll = requireAll;
        }

        public override bool Evaluate(IEventConditionContext context)
        {
            return _requireAll
                ? _conditions.All(condition => condition.Evaluate(context))
                : _conditions.Any(condition => condition.Evaluate(context));
        }
    }

    private sealed class NotCondition : EventCondition
    {
        private readonly EventCondition _condition;

        public NotCondition(EventCondition condition)
        {
            _condition = condition;
        }

        public override bool Evaluate(IEventConditionContext context) => !_condition.Evaluate(context);
    }
}

public enum VariableComparison
{
    Equal,
    NotEqual,
    Greater,
    Less,
    GreaterOrEqual,
    LessOrEqual,
}

public static class VariableComparisonExtensions
{
    public static bool Matches(this VariableComparison comparison, int actual, int expected) => comparison switch
    {
        VariableComparison.Equal => actual == expected,
        VariableComparison.NotEqual => actual != expected,
        VariableComparison.Greater => actual > expected,
        VariableComparison.Less => actual < expected,
        VariableComparison.GreaterOrEqual => actual >= expected,
        VariableComparison.LessOrEqual => actual <= expected,
        _ => false,
    };

                                                                                  
                                                                                        
                                                                                     
    public static bool Matches(this VariableComparison comparison, float actual, float expected) => comparison switch
    {
        VariableComparison.Equal => Math.Abs(actual - expected) < 0.001f,
        VariableComparison.NotEqual => Math.Abs(actual - expected) >= 0.001f,
        VariableComparison.Greater => actual > expected,
        VariableComparison.Less => actual < expected,
        VariableComparison.GreaterOrEqual => actual >= expected,
        VariableComparison.LessOrEqual => actual <= expected,
        _ => false,
    };

    public static string Symbol(this VariableComparison comparison) => comparison switch
    {
        VariableComparison.Equal => "==",
        VariableComparison.NotEqual => "!=",
        VariableComparison.Greater => ">",
        VariableComparison.Less => "<",
        VariableComparison.GreaterOrEqual => ">=",
        VariableComparison.LessOrEqual => "<=",
        _ => "?",
    };
}