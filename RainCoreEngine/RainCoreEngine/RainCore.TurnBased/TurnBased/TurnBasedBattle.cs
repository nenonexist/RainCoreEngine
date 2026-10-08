namespace RainCore;

public enum TurnBasedBattleOutcome { Victory, Defeat, Escape }
public enum TurnBasedBattleAction { Attack, Skill, Item, Talk, Flee }

                                                             
                                                                                 
public enum TurnBasedNegotiationLine { Flatter, Intimidate, Joke, BegForMercy }

public sealed class TurnBasedCombatant
{
    public string Id { get; }
    public string Name { get; }
    public bool IsEnemy { get; }
    public int MaxHp { get; }
    public int Hp { get; set; }
    public int MaxMp { get; }
    public int Mp { get; set; }
    public int Attack { get; }
    public int Defense { get; }
    public int MagicAttack { get; }
    public int MagicDefense { get; }
    public int Agility { get; }
    public IReadOnlyList<string> SkillIds { get; }
    public bool IsAlive => Hp > 0;

                                                                                                           
    public int Mood { get; set; }
                                                                                
                                                                                       
    public bool Insulted { get; set; }
                                                                                 
                                                                               
                                                                              
                                                                         
    public bool NegotiationSettled { get; set; }
                                                                               
                                                                              
                                                             
    public bool IsOut => !IsAlive || NegotiationSettled;

    private TurnBasedCombatant(string id, string name, bool isEnemy, int maxHp, int maxMp,
        int attack, int defense, int magicAttack, int magicDefense, int agility, IReadOnlyList<string> skillIds)
    {
        Id = id; Name = name; IsEnemy = isEnemy;
        MaxHp = maxHp; Hp = maxHp; MaxMp = maxMp; Mp = maxMp;
        Attack = attack; Defense = defense; MagicAttack = magicAttack;
        MagicDefense = magicDefense; Agility = agility; SkillIds = skillIds;
    }

    public static TurnBasedCombatant FromActor(TurnBasedActorData data, (int Hp, int Mp) current) => new(
        data.Id, data.Name, false, data.MaxHp, data.MaxMp, data.Attack, data.Defense,
        data.MagicAttack, data.MagicDefense, data.Agility, data.SkillIds)
    { Hp = current.Hp, Mp = current.Mp };

    public static TurnBasedCombatant FromEnemy(TurnBasedEnemyData data) => new(
        data.Id, data.Name, true, data.MaxHp, data.MaxMp, data.Attack, data.Defense,
        data.MagicAttack, data.MagicDefense, data.Agility, data.SkillIds);
}

                                                                               
                                                                                                 
public sealed class TurnBasedBattleState
{
    private readonly List<TurnBasedCombatant> _turnOrder = new();
    private TurnBasedDatabase? _database;
    private TurnBasedPartyState? _partyState;
    private int _turnIndex;
    private (TurnBasedBattleAction Action, string? SkillId)? _pending;
    private double _delay;

    public bool IsActive { get; private set; }
    public TurnBasedBattleOutcome? Outcome { get; private set; }
    public IReadOnlyList<TurnBasedCombatant> Party { get; private set; } = Array.Empty<TurnBasedCombatant>();
    public IReadOnlyList<TurnBasedCombatant> Enemies { get; private set; } = Array.Empty<TurnBasedCombatant>();
    public List<string> Log { get; } = new();

                                                                              
                                                                              
                                                                            
                                                                            
                          
    public List<TurnBasedCombatant> RecruitedThisBattle { get; } = new();
    public int GoldGainedFromTalk { get; private set; }
    public List<string> ItemsGainedFromTalk { get; } = new();

    public TurnBasedCombatant? AwaitingPlayer => IsActive && Outcome == null && _pending == null &&
        _turnIndex < _turnOrder.Count && !_turnOrder[_turnIndex].IsEnemy && !_turnOrder[_turnIndex].IsOut
            ? _turnOrder[_turnIndex] : null;

                                                                           
                                                                              
                                                         
    public IEnumerable<TurnBasedCombatant> TalkableEnemies => Enemies.Where(x => x.IsAlive && !x.IsOut && !x.Insulted);

    public void Start(TurnBasedDatabase database, string troopId, TurnBasedPartyState partyState)
    {
        _database = database;
        _partyState = partyState;
        var troop = database.FindTroop(troopId);
        Party = database.Actors.Select(actor => TurnBasedCombatant.FromActor(actor, partyState.CurrentOf(actor))).Where(x => x.IsAlive).ToList();
        Enemies = (troop?.EnemyIds ?? new List<string>()).Select(database.FindEnemy).Where(x => x != null).Select(x => TurnBasedCombatant.FromEnemy(x!)).ToList();
        _turnOrder.Clear();
        _turnOrder.AddRange(Party.Concat(Enemies).OrderByDescending(x => x.Agility));
        _turnIndex = 0;
        _pending = null;
        _delay = 0;
        Outcome = null;
        Log.Clear();
        RecruitedThisBattle.Clear();
        GoldGainedFromTalk = 0;
        ItemsGainedFromTalk.Clear();
        Log.Add(troop == null ? "The battle begins." : $"{troop.Name}: the battle begins.");
        IsActive = Party.Count > 0 && Enemies.Count > 0;
    }

                                                                                
                                                                      
                                                               
                                                                                             
    public void Submit(TurnBasedBattleAction action, string? skillId = null, string? itemId = null,
        string? targetId = null, TurnBasedNegotiationLine? talkLine = null)
    {
        if (AwaitingPlayer != null)
        {
            _pending = (action, skillId);
            _pendingItemId = itemId;
            _pendingTargetId = targetId;
            _pendingTalkLine = talkLine ?? TurnBasedNegotiationLine.Joke;
        }
    }

    private string? _pendingItemId;
    private string? _pendingTargetId;
    private TurnBasedNegotiationLine _pendingTalkLine;

    public void Tick(double dt)
    {
        if (!IsActive || Outcome != null) return;

        if (AwaitingPlayer != null)
        {
            if (_pending is not null)
            {
                var activeActor = AwaitingPlayer;
                var pending = _pending.Value;
                PlayerTurn(activeActor, pending.Action, pending.SkillId, _pendingItemId, _pendingTargetId, _pendingTalkLine);
                _pending = null;
                _pendingItemId = null;
                _pendingTargetId = null;
                CheckOutcome();
                _turnIndex++;
                if (_turnIndex >= _turnOrder.Count)
                {
                    _turnIndex = 0;
                    SkipDead();
                }
            }
            return;
        }

        _delay -= dt;
        if (_delay > 0) return;
        _delay = 0.7;
        SkipDead();
        if (_turnIndex >= _turnOrder.Count) { _turnIndex = 0; SkipDead(); }
        if (Outcome != null) return;
        var actor = _turnOrder[_turnIndex];
        if (actor.IsEnemy) EnemyTurn(actor);
        else return;
        CheckOutcome();
        _turnIndex++;
    }

    private void SkipDead()
    {
        int guard = 0;
        while (_turnIndex < _turnOrder.Count && !_turnOrder[_turnIndex].IsAlive && guard++ < _turnOrder.Count)
            _turnIndex++;
    }

    private void EnemyTurn(TurnBasedCombatant enemy)
    {
        var target = Party.Where(x => x.IsAlive).OrderBy(x => x.Hp).FirstOrDefault();
        if (target == null) return;
        int damage = Math.Max(1, enemy.Attack - target.Defense);
        target.Hp = Math.Max(0, target.Hp - damage);
        Log.Add($"{enemy.Name} deals {damage} damage to {target.Name}.");
    }

    private void PlayerTurn(TurnBasedCombatant actor, TurnBasedBattleAction action, string? skillId, string? itemId,
        string? targetId = null, TurnBasedNegotiationLine? talkLine = null)
    {
        if (action == TurnBasedBattleAction.Flee)
        {
            if (Random.Shared.NextDouble() < 0.5) End(TurnBasedBattleOutcome.Escape);
            else Log.Add($"{actor.Name} couldn't escape.");
            return;
        }

        if (action == TurnBasedBattleAction.Talk)
        {
            var target = Enemies.Where(x => x.IsAlive).OrderBy(x => x.Hp).FirstOrDefault();
            if (target == null)
            {
                Log.Add($"{actor.Name} tries to talk, but there are no enemies left.");
                return;
            }

                                                                                    
                                                                                          
                                                                                       
                                                                                          
            var line = $"{actor.Name} talks to {target.Name}: \"This monster looks pretty stubborn.\"";
            Log.Add(line);
            return;
        }

        if (action == TurnBasedBattleAction.Item)
        {
            var item = _database?.Items.FirstOrDefault(x => x.Id == itemId) ?? _database?.Items.FirstOrDefault();
            if (item == null)
            {
                Log.Add("There are no items in the bag.");
                return;
            }

            actor.Hp = Math.Min(actor.MaxHp, actor.Hp + item.HealHp);
            actor.Mp = Math.Min(actor.MaxMp, actor.Mp + item.HealMp);
            Log.Add($"{actor.Name} uses {item.Name}.");
            return;
        }

        var targetEnemy = Enemies.Where(x => x.IsAlive).OrderBy(x => x.Hp).FirstOrDefault();
        if (targetEnemy == null) return;
        int damage = Math.Max(1, actor.Attack - targetEnemy.Defense);
        if (action == TurnBasedBattleAction.Skill && skillId != null)
        {
            var skill = _database?.FindSkill(skillId);
            if (skill != null && actor.Mp >= skill.MpCost)
            {
                actor.Mp -= skill.MpCost;
                damage = Math.Max(1, skill.Power + actor.MagicAttack - targetEnemy.MagicDefense);
                Log.Add($"{actor.Name} uses {skill.Name}: {damage} damage.");
            }
            else if (skill != null)
            {
                Log.Add($"{actor.Name} doesn't have enough MP for {skill.Name}.");
                return;
            }
            else
            {
                Log.Add($"{actor.Name} tries to use a skill, but it wasn't found.");
                return;
            }
        }
        else Log.Add($"{actor.Name} attacks: {damage} damage.");
        targetEnemy.Hp = Math.Max(0, targetEnemy.Hp - damage);
    }

    private void CheckOutcome()
    {
        if (Enemies.All(x => !x.IsAlive)) End(TurnBasedBattleOutcome.Victory);
        else if (Party.All(x => !x.IsAlive)) End(TurnBasedBattleOutcome.Defeat);
    }

    private void End(TurnBasedBattleOutcome outcome)
    {
        Outcome = outcome;
        IsActive = false;
        _partyState?.Commit(Party);
        Log.Add(outcome switch
        {
            TurnBasedBattleOutcome.Victory => "Victory.",
            TurnBasedBattleOutcome.Defeat => "Defeat.",
            _ => "Escaped.",
        });
    }
}
