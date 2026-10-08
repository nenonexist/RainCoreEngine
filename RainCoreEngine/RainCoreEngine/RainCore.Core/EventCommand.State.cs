namespace RainCore;


using OpenTK.Mathematics;

                                                                    
                                                                 


                                                                                         
public class SetFlagCommand : EventCommand
{
    private readonly string _name;
    private readonly bool _value;
    public string Name => _name;
    public bool Value => _value;
    public SetFlagCommand(string name, bool value = true) { _name = name; _value = value; }

    public override IEnumerator<EventWait> Run(IEventContext ctx)
    {
        GameFlags.Set(_name, _value);
        yield return EventWait.OneFrame;
    }
}


                                                                                            
public class SetSelfSwitchCommand : EventCommand
{
    private readonly string _name;
    private readonly bool _value;
    public string Name => _name;
    public bool Value => _value;

    public SetSelfSwitchCommand(string name, bool value = true)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Имя self-switch не может быть пустым.", nameof(name));

        _name = name;
        _value = value;
    }

    public override IEnumerator<EventWait> Run(IEventContext ctx)
    {
        if (ctx is not ITriggerSelfSwitchContext triggerContext)
            throw new InvalidOperationException("Self-switch команда должна выполняться внутри TriggerZone.");

        triggerContext.SetSelfSwitch(_name, _value);
        yield return EventWait.OneFrame;
    }
}


                                                                                                              
public class AddVariableCommand : EventCommand
{
    private readonly string _name;
    private readonly int _delta;
    public string Name => _name;
    public int Delta => _delta;
    public AddVariableCommand(string name, int delta) { _name = name; _delta = delta; }

    public override IEnumerator<EventWait> Run(IEventContext ctx)
    {
        GameFlags.AddVar(_name, _delta);
        yield return EventWait.OneFrame;
    }


}


public class SetVariableCommand : EventCommand
{
    private readonly string _name;
    private readonly int _value;
    public string Name => _name;
    public int Value => _value;

    public SetVariableCommand(string name, int value)
    {
        _name = name;
        _value = value;
    }

    public override IEnumerator<EventWait> Run(IEventContext ctx)
    {
        GameFlags.SetVar(_name, _value);
        yield return EventWait.OneFrame;
    }
}


public sealed class RandomVariableCommand : EventCommand
{
    private readonly string _name;
    private readonly int _min;
    private readonly int _max;
    private static readonly Random Rng = new();
    public string Name => _name;
    public int Min => _min;
    public int Max => _max;

    public RandomVariableCommand(string name, int min, int max)
    {
        _name = name;
        _min = Math.Min(min, max);
        _max = Math.Max(min, max);
    }

    public override IEnumerator<EventWait> Run(IEventContext ctx)
    {
        GameFlags.SetVar(_name, Rng.Next(_min, _max == int.MaxValue ? int.MaxValue : _max + 1));
        yield return EventWait.OneFrame;
    }
}


                                                                         
public class GiveItemCommand : EventCommand
{
    private readonly string _itemName;
    private readonly int _amount;
    public string ItemName => _itemName;
    public int Amount => _amount;

    public GiveItemCommand(string itemName, int amount = 1)
    {
        _itemName = itemName;
        _amount = amount;
    }

    public override IEnumerator<EventWait> Run(IEventContext ctx)
    {
        ctx.AddItem(_itemName, _amount);
        ctx.ShowDialogue("", $"Получено: {_itemName} x{_amount}");
        yield return new EventWait(2f);
    }
}


                                                                 
public class RemoveItemCommand : EventCommand
{
    private readonly string _itemName;
    private readonly int _amount;
    public string ItemName => _itemName;
    public int Amount => _amount;

    public RemoveItemCommand(string itemName, int amount = 1)
    {
        _itemName = itemName;
        _amount = amount;
    }

    public override IEnumerator<EventWait> Run(IEventContext ctx)
    {
        if (!ctx.RemoveItem(_itemName, _amount))
            ctx.ShowDialogue("", $"Не хватает: {_itemName} x{_amount}");
        yield return EventWait.OneFrame;
    }
}


                                                                                  
                                                                             
                                                                                
                                                                                
                                                                               
                                                         
public sealed class WakeUpCommand : EventCommand
{
    public override IEnumerator<EventWait> Run(IEventContext ctx)
    {
        ctx.WakeFromRandomDream();
        yield return EventWait.OneFrame;
    }
}

                                                                      
                                                                   
                                                                       
           

                                                                                              
public class StartQuestCommand : EventCommand
{
    private readonly string _questId;
    public string QuestId => _questId;
    public StartQuestCommand(string questId) { _questId = questId; }

    public override IEnumerator<EventWait> Run(IEventContext ctx)
    {
        QuestTracker.Start(_questId);
        yield return EventWait.OneFrame;
    }
}

                                                                                
                                                                                    
                                                                                 
                                                                   
public class AdvanceQuestCommand : EventCommand
{
    private readonly string _questId;
    public string QuestId => _questId;
    public AdvanceQuestCommand(string questId) { _questId = questId; }

    public override IEnumerator<EventWait> Run(IEventContext ctx)
    {
        QuestTracker.Advance(_questId);
        yield return EventWait.OneFrame;
    }
}

                                                                                       
public class CompleteQuestCommand : EventCommand
{
    private readonly string _questId;
    public string QuestId => _questId;
    public CompleteQuestCommand(string questId) { _questId = questId; }

    public override IEnumerator<EventWait> Run(IEventContext ctx)
    {
        QuestTracker.Complete(_questId);
        yield return EventWait.OneFrame;
    }
}

                                                                                                 
public class FailQuestCommand : EventCommand
{
    private readonly string _questId;
    public string QuestId => _questId;
    public FailQuestCommand(string questId) { _questId = questId; }

    public override IEnumerator<EventWait> Run(IEventContext ctx)
    {
        QuestTracker.Fail(_questId);
        yield return EventWait.OneFrame;
    }
}
