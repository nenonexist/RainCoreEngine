namespace RainCore;

public enum BattleOutcome { Victory, Defeat, Escape }
public enum BattleActionKind { Attack, Skill, Flee }

             
                                                                           
                                                                             
                                                                  
                                                                     
                                                    
   
                                                                    
                                                                        
                                                                           
                                                                 
              
public sealed class BattleState
{
    private const float ActionDelaySeconds = 0.7f;                                                                                      

    public bool IsActive { get; private set; }
    public IReadOnlyList<Combatant> Party { get; private set; } = Array.Empty<Combatant>();
    public IReadOnlyList<Combatant> Enemies { get; private set; } = Array.Empty<Combatant>();
    private List<Combatant> _turnOrder = new();
    private int _turnIndex;
    private double _actionTimer;
    private (BattleActionKind Kind, string? SkillId)? _pendingPlayerAction;

    public List<string> Log { get; } = new();
    public BattleOutcome? Outcome { get; private set; }

    private GameDatabase _database = null!;
    private PartyState _partyState = null!;

                                                                               
                                                                         
                                                                          
                                                      
    public Combatant? AwaitingActionFrom =>
        IsActive && Outcome == null && _pendingPlayerAction == null
        && _turnIndex < _turnOrder.Count && !_turnOrder[_turnIndex].IsEnemy && _turnOrder[_turnIndex].IsAlive
            ? _turnOrder[_turnIndex] : null;

    public void Start(GameDatabase database, string troopId, PartyState partyState)
    {
        _database = database;
        _partyState = partyState;

        var troop = database.FindTroop(troopId);
        var enemyIds = troop?.EnemyIds ?? new List<string>();

        Party = partyState.BuildParty(database).Where(c => c.IsAlive).ToList();
        Enemies = enemyIds.Select(database.FindEnemy).Where(e => e != null).Select(e => Combatant.FromEnemy(e!)).ToList();

        _turnOrder = Party.Concat(Enemies).OrderByDescending(c => c.Agility).ToList();
        _turnIndex = 0;
        _actionTimer = 0;
        _pendingPlayerAction = null;
        Outcome = null;
        Log.Clear();
        Log.Add(troop != null ? $"Battle start: {troop.Name}!" : "Battle start!");

        IsActive = Party.Count > 0 && Enemies.Count > 0;
        if (!IsActive) Log.Add("Battle could not start (empty party or troop).");
    }

                                                                                   
                                                                           
                                                                                      
    public void SubmitPlayerAction(BattleActionKind kind, string? skillId = null)
    {
        if (AwaitingActionFrom == null) return;
        _pendingPlayerAction = (kind, skillId);
    }

    public void Tick(double dt)
    {
        if (!IsActive || Outcome != null) return;
        if (AwaitingActionFrom != null) return;                                     

        _actionTimer -= dt;
        if (_actionTimer > 0) return;
        _actionTimer = ActionDelaySeconds;

        AdvancePastDead();
        if (_turnIndex >= _turnOrder.Count) { _turnIndex = 0; AdvancePastDead(); }
        if (Outcome != null) return;

        var actor = _turnOrder[_turnIndex];
        if (!actor.IsAlive) { _turnIndex++; return; }

        if (actor.IsEnemy)
            RunEnemyTurn(actor);
        else if (_pendingPlayerAction is { } action)
        {
            RunPlayerAction(actor, action.Kind, action.SkillId);
            _pendingPlayerAction = null;
        }
        else
        {
            return;                                                                    
        }

        CheckOutcome();
        _turnIndex++;
    }

    private void AdvancePastDead()
    {
        int guard = 0;
        while (_turnIndex < _turnOrder.Count && !_turnOrder[_turnIndex].IsAlive && guard++ < _turnOrder.Count)
            _turnIndex++;
    }

    private void RunEnemyTurn(Combatant enemy)
    {
        var target = Party.Where(p => p.IsAlive).OrderBy(p => p.Hp).FirstOrDefault();
        if (target == null) return;

        int dmg = Math.Max(1, enemy.Attack - target.Defense);
        target.Hp = Math.Max(0, target.Hp - dmg);
        Log.Add($"{enemy.Name} attacks {target.Name} for {dmg} damage.");
        if (!target.IsAlive) Log.Add($"{target.Name} is down!");
    }

    private void RunPlayerAction(Combatant actor, BattleActionKind kind, string? skillId)
    {
        switch (kind)
        {
            case BattleActionKind.Attack:
            {
                var target = Enemies.Where(e => e.IsAlive).OrderBy(e => e.Hp).FirstOrDefault();
                if (target == null) return;
                int dmg = Math.Max(1, actor.Attack - target.Defense);
                target.Hp = Math.Max(0, target.Hp - dmg);
                Log.Add($"{actor.Name} attacks {target.Name} for {dmg} damage.");
                if (!target.IsAlive) Log.Add($"{target.Name} is defeated!");
                break;
            }
            case BattleActionKind.Skill:
            {
                var skill = skillId != null ? _database.FindSkill(skillId) : null;
                if (skill == null || actor.Mp < skill.MpCost)
                {
                    Log.Add($"{actor.Name} could not use the skill - falling back to Attack.");
                    RunPlayerAction(actor, BattleActionKind.Attack, null);
                    return;
                }
                actor.Mp -= skill.MpCost;
                var target = Enemies.Where(e => e.IsAlive).OrderBy(e => e.Hp).FirstOrDefault();
                if (target == null) return;
                int dmg = Math.Max(1, skill.Power + actor.MagicAttack - target.MagicDefense);
                target.Hp = Math.Max(0, target.Hp - dmg);
                Log.Add($"{actor.Name} uses {skill.Name} on {target.Name} for {dmg} damage.");
                if (!target.IsAlive) Log.Add($"{target.Name} is defeated!");
                break;
            }
            case BattleActionKind.Flee:
            {
                float partyAgi = (float)Party.Where(p => p.IsAlive).Average(p => p.Agility);
                float enemyAgi = Enemies.Count > 0 ? (float)Enemies.Where(e => e.IsAlive).Average(e => e.Agility) : 0f;
                bool success = Random.Shared.NextDouble() < partyAgi / MathF.Max(1f, partyAgi + enemyAgi);
                if (success)
                {
                    Log.Add($"{actor.Name} fled the battle.");
                    End(BattleOutcome.Escape);
                }
                else
                {
                    Log.Add($"{actor.Name} failed to flee!");
                }
                break;
            }
        }
    }

    private void CheckOutcome()
    {
        if (Outcome != null) return;
        if (Enemies.All(e => !e.IsAlive)) End(BattleOutcome.Victory);
        else if (Party.All(p => !p.IsAlive)) End(BattleOutcome.Defeat);
    }

    private void End(BattleOutcome outcome)
    {
        Outcome = outcome;
        IsActive = false;
        _partyState.CommitFromCombatants(Party);

        if (outcome == BattleOutcome.Victory)
        {
            int exp = 0, gold = 0;
            foreach (var e in Enemies)
            {
                var data = _database.FindEnemy(e.Id);
                if (data == null) continue;
                exp += data.ExpReward;
                gold += data.GoldReward;
            }
            Log.Add($"Victory! +{exp} EXP, +{gold} gold.");
        }
        else if (outcome == BattleOutcome.Defeat)
        {
            Log.Add("Defeat...");
        }
    }

                                                                       
                                                                          
                                                                            
                                                                      
                                           
    public BattleOutcome? ConsumeOutcome()
    {
        var result = Outcome;
        Outcome = null;
        return result;
    }
}
