namespace RainCore;

public interface IBattleApi
{
    bool IsBattleActive { get; }
    bool StartBattle(string troopId);
}

public sealed class StartBattleCommand : EventCommand
{
    public string TroopId { get; }

    public StartBattleCommand(string troopId)
    {
        TroopId = troopId;
    }

    public override IEnumerator<EventWait> Run(IEventContext ctx)
    {
        if (ctx is not IBattleApi battle || !battle.StartBattle(TroopId))
            yield break;

        while (battle.IsBattleActive)
            yield return EventWait.OneFrame;
    }
}
