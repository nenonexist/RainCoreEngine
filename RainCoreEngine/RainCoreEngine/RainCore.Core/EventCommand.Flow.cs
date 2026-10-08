namespace RainCore;


using OpenTK.Mathematics;

                                                                
                                                             


                                                                                                    
public class WaitCommand : EventCommand
{
    private readonly float _seconds;
    public float Seconds => _seconds;
    public WaitCommand(float seconds)
    {
        _seconds = seconds;
    }


    public override IEnumerator<EventWait> Run(IEventContext ctx)
    {
        yield return new EventWait(_seconds);
    }
}


public sealed class IfVariableCommand : EventCommand
{
    private readonly string _name;
    private readonly VariableComparison _comparison;
    private readonly int _expected;
    private readonly IReadOnlyList<EventCommand> _thenCommands;
    private readonly IReadOnlyList<EventCommand> _elseCommands;
    public string Name => _name;
    public VariableComparison Comparison => _comparison;
    public int Expected => _expected;
    public IReadOnlyList<EventCommand> ThenCommands => _thenCommands;
    public IReadOnlyList<EventCommand> ElseCommands => _elseCommands;

    public IfVariableCommand(string name, VariableComparison comparison, int expected,
        IEnumerable<EventCommand> thenCommands, IEnumerable<EventCommand>? elseCommands = null)
    {
        _name = name;
        _comparison = comparison;
        _expected = expected;
        _thenCommands = thenCommands.ToList();
        _elseCommands = elseCommands == null
            ? new List<EventCommand>()
            : elseCommands.ToList();
    }

    public override IEnumerator<EventWait> Run(IEventContext ctx)
    {
        var commands = _comparison.Matches(GameFlags.GetVar(_name), _expected) ? _thenCommands : _elseCommands;
        foreach (var command in commands)
        {
            var step = command.Run(ctx);
            while (step.MoveNext())
                yield return step.Current;
        }
    }
}


public sealed class CallCommonEventCommand : EventCommand
{
    private readonly string _name;
    public string Name => _name;

    public CallCommonEventCommand(string name) { _name = name; }

    public override IEnumerator<EventWait> Run(IEventContext ctx)
    {
        if (!CommonEvents.TryGet(_name, out var commands)) yield break;
        foreach (var command in commands)
        {
            var step = command.Run(ctx);
            while (step.MoveNext()) yield return step.Current;
        }
    }
}


public sealed class CommentCommand : EventCommand
{
    private readonly string _text;
    public string Text => _text;
    public CommentCommand(string text) { _text = text; }
    public override IEnumerator<EventWait> Run(IEventContext ctx)
    {
        yield return EventWait.OneFrame;
    }
}


public sealed class LoopCommand : EventCommand
{
    private readonly int _count;
    private readonly IReadOnlyList<EventCommand> _commands;
    public int Count => _count;
    public IReadOnlyList<EventCommand> Commands => _commands;

    public LoopCommand(int count, IEnumerable<EventCommand> commands)
    {
        _count = Math.Max(1, count);
        _commands = commands.ToList();
    }

    public override IEnumerator<EventWait> Run(IEventContext ctx)
    {
        for (int iteration = 0; iteration < _count; iteration++)
            foreach (var command in _commands)
            {
                var step = command.Run(ctx);
                while (step.MoveNext()) yield return step.Current;
            }
    }
}
