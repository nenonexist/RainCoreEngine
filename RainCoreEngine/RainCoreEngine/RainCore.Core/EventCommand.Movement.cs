namespace RainCore;


using OpenTK.Mathematics;

                                                                                      


public sealed class MoveRouteCommand : EventCommand
{
    private readonly string _npcName;
    private readonly Vector3 _offset;
    public string NpcName => _npcName;
    public Vector3 Offset => _offset;

    public MoveRouteCommand(string npcName, Vector3 offset)
    {
        _npcName = npcName;
        _offset = offset;
    }

    public override IEnumerator<EventWait> Run(IEventContext ctx)
    {
        ctx.TryMoveNpc(_npcName, _offset);
        yield return EventWait.OneFrame;
    }
}


public class TeleportCommand : EventCommand, ITeleportCommand

{
    private readonly Vector3 _pos;
    public Vector3 Position => _pos;
    public TeleportCommand(Vector3 pos)
    {
        _pos = pos;
    }

    public TeleportCommand(float x, float y, float z)
    {
        _pos = new Vector3(x, y, z);
    }


    public override IEnumerator<EventWait> Run(IEventContext ctx)
    {
        ctx.TeleportPlayer(_pos);
        yield return EventWait.OneFrame;
    }
}


                                                                                 
public class SpawnWatchNpcCommand : EventCommand
{
    private readonly Npc _npc;
    private readonly float _duration;

    public SpawnWatchNpcCommand(Npc npc, float duration = 5f)
    {
        _npc = npc;
        _duration = duration;
    }

    public override IEnumerator<EventWait> Run(IEventContext ctx)
    {
        ctx.AddNpc(_npc);
        yield return new EventWait(_duration);
        ctx.RemoveNpc(_npc);
        yield return EventWait.OneFrame;
    }
}
