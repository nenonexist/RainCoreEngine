namespace RainCore;

                                                                              
                                                                               
                                                                        
                                                                        
                                                                         
                           
public sealed class Combatant
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

    public Combatant(string id, string name, bool isEnemy, int maxHp, int hp, int maxMp, int mp,
        int attack, int defense, int magicAttack, int magicDefense, int agility, IReadOnlyList<string> skillIds)
    {
        Id = id; Name = name; IsEnemy = isEnemy;
        MaxHp = maxHp; Hp = hp; MaxMp = maxMp; Mp = mp;
        Attack = attack; Defense = defense; MagicAttack = magicAttack; MagicDefense = magicDefense;
        Agility = agility; SkillIds = skillIds;
    }

    public static Combatant FromActor(ActorData a, int? currentHp = null, int? currentMp = null) => new(
        a.Id, a.Name, isEnemy: false, a.MaxHp, currentHp ?? a.MaxHp, a.MaxMp, currentMp ?? a.MaxMp,
        a.Attack, a.Defense, a.MagicAttack, a.MagicDefense, a.Agility, a.SkillIds);

    public static Combatant FromEnemy(EnemyData e) => new(
        e.Id, e.Name, isEnemy: true, e.MaxHp, e.MaxHp, e.MaxMp, e.MaxMp,
        e.Attack, e.Defense, e.MagicAttack, e.MagicDefense, e.Agility, e.SkillIds);
}

                                                                             
                                                                          
                                                                        
                                                                              
                                                                         
                                                                   
                           
public sealed class PartyState
{
    private readonly Dictionary<string, (int Hp, int Mp)> _current = new();

                                                                             
                                                                             
                                                                              
                                                               
    public void InitializeFromDatabase(GameDatabase database)
    {
        _current.Clear();
        foreach (var actor in database.Actors)
            _current[actor.Id] = (actor.MaxHp, actor.MaxMp);
    }

    public List<Combatant> BuildParty(GameDatabase database)
    {
        var result = new List<Combatant>();
        foreach (var actor in database.Actors)
        {
            var (hp, mp) = _current.TryGetValue(actor.Id, out var state) ? state : (actor.MaxHp, actor.MaxMp);
            result.Add(Combatant.FromActor(actor, hp, mp));
        }
        return result;
    }

                                                                              
                                                                              
                                                               
    public void CommitFromCombatants(IEnumerable<Combatant> partyCombatants)
    {
        foreach (var c in partyCombatants.Where(c => !c.IsEnemy))
            _current[c.Id] = (c.Hp, c.Mp);
    }
}
