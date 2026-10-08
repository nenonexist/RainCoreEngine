using System.Text.Json;

namespace RainCore;

public sealed record TurnBasedActorData(
    string Id, string Name,
    int MaxHp, int MaxMp, int Attack, int Defense, int MagicAttack, int MagicDefense, int Agility, int Luck,
    List<string> SkillIds, string? IconPath = null);

public enum TurnBasedSkillTargetKind { OneEnemy, AllEnemies, OneAlly, AllAllies, Self }

public sealed record TurnBasedSkillData(
    string Id, string Name, int MpCost, int Power, TurnBasedSkillTargetKind Target, string Description, string? IconPath = null);

public sealed record TurnBasedItemData(
    string Id, string Name, int Price, int HealHp, int HealMp, string Description, string? IconPath = null);

public enum TurnBasedTemperament { Aggressive, Greedy, Prideful, Fearful, Calm }

public sealed record TurnBasedEnemyData(
    string Id, string Name,
    int MaxHp, int MaxMp, int Attack, int Defense, int MagicAttack, int MagicDefense, int Agility, int Luck,
    List<string> SkillIds, int ExpReward, int GoldReward, string? SpritePath = null,
                                      
    TurnBasedTemperament Temperament = TurnBasedTemperament.Calm,
    bool Recruitable = false,
    string? RecruitActorTemplateId = null,
    int GoldOffer = 0,
    string? ItemOfferId = null);

public sealed record TurnBasedTroopData(
    string Id, string Name, List<string> EnemyIds, string? BackgroundPath = null, string? MusicPath = null);

                                                                                                  
public sealed class TurnBasedDatabase
{
    public List<TurnBasedActorData> Actors { get; set; } = new();
                                                                                            
                                                                                      
                                                                        
    public List<TurnBasedActorData> ActorTemplates { get; set; } = new();
    public List<TurnBasedSkillData> Skills { get; set; } = new();
    public List<TurnBasedItemData> Items { get; set; } = new();
    public List<TurnBasedEnemyData> Enemies { get; set; } = new();
    public List<TurnBasedTroopData> Troops { get; set; } = new();

    private static readonly JsonSerializerOptions Options = new() { WriteIndented = true };
    private static string FilePath => Path.Combine(AppContext.BaseDirectory, "Database", "database.json");

    public static TurnBasedDatabase Load()
    {
        if (!File.Exists(FilePath))
            return CreateDefaults();

        try
        {
            var database = JsonSerializer.Deserialize<TurnBasedDatabase>(File.ReadAllText(FilePath), Options) ?? CreateDefaults();
            database.EnsureDefaults();
            return database;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[TURN-BASED Database] Failed to read {FilePath}: {ex.Message}");
            return CreateDefaults();
        }
    }

    public void Save()
    {
        Directory.CreateDirectory(Path.GetDirectoryName(FilePath)!);
        File.WriteAllText(FilePath, JsonSerializer.Serialize(this, Options));
    }

    public void EnsureDefaults()
    {
        if (Actors.Count == 0 || Skills.Count == 0 || Items.Count == 0 || Enemies.Count == 0 || Troops.Count == 0)
        {
            var defaults = CreateDefaults();
            if (Actors.Count == 0) Actors.AddRange(defaults.Actors);
            if (Skills.Count == 0) Skills.AddRange(defaults.Skills);
            if (Items.Count == 0) Items.AddRange(defaults.Items);
            if (Enemies.Count == 0) Enemies.AddRange(defaults.Enemies);
            if (Troops.Count == 0) Troops.AddRange(defaults.Troops);
        }
    }

    private static TurnBasedDatabase CreateDefaults()
    {
        var database = new TurnBasedDatabase();
        database.Actors.Add(new TurnBasedActorData("dreamer", "Parsifal", 120, 30, 18, 10, 16, 12, 14, 8, new() { "snow_lance" }, "icon_dreamer.png"));
        database.Skills.Add(new TurnBasedSkillData("snow_lance", "Snow Lance", 5, 24, TurnBasedSkillTargetKind.OneEnemy, "A cold strike against a single target.", "icon_snow_lance.png"));
        database.Items.Add(new TurnBasedItemData("snow_herb", "Snow Herb", 20, 30, 0, "Restores health.", "icon_snow_herb.png"));
        database.Enemies.Add(new TurnBasedEnemyData("snow_slime", "Snow Slime", 48, 0, 11, 5, 0, 3, 8, 2, new(), 12, 8, "enemy_wisp.png"));
        database.Enemies.Add(new TurnBasedEnemyData("snow_wolf", "Snow Wolf", 72, 0, 15, 7, 0, 4, 13, 3, new(), 20, 14, "enemy_wisp.png"));
        database.Troops.Add(new TurnBasedTroopData("dungeon_slimes", "Snowfield Slimes", new() { "snow_slime", "snow_slime" }, "battle_frost_ruins.png", string.Empty));
        database.Troops.Add(new TurnBasedTroopData("dungeon_wolf", "Snow Wolf", new() { "snow_wolf" }, "battle_ice_gate.png", string.Empty));
        return database;
    }

    public TurnBasedActorData? FindActor(string id) => Actors.FirstOrDefault(x => x.Id == id);
    public TurnBasedActorData? FindActorTemplate(string id) => ActorTemplates.FirstOrDefault(x => x.Id == id);
    public TurnBasedSkillData? FindSkill(string id) => Skills.FirstOrDefault(x => x.Id == id);
    public TurnBasedEnemyData? FindEnemy(string id) => Enemies.FirstOrDefault(x => x.Id == id);
    public TurnBasedTroopData? FindTroop(string id) => Troops.FirstOrDefault(x => x.Id == id);
}

public sealed class TurnBasedPartyState
{
    private readonly Dictionary<string, (int Hp, int Mp)> _current = new();

    public void Initialize(TurnBasedDatabase database)
    {
        _current.Clear();
        foreach (var actor in database.Actors)
            _current[actor.Id] = (actor.MaxHp, actor.MaxMp);
    }

    public IReadOnlyList<TurnBasedActorData> Actors(TurnBasedDatabase database) => database.Actors;

    internal (int Hp, int Mp) CurrentOf(TurnBasedActorData actor) =>
        _current.TryGetValue(actor.Id, out var state) ? state : (actor.MaxHp, actor.MaxMp);

    internal void Commit(IEnumerable<TurnBasedCombatant> party)
    {
        foreach (var member in party.Where(x => !x.IsEnemy))
            _current[member.Id] = (member.Hp, member.Mp);
    }
}
