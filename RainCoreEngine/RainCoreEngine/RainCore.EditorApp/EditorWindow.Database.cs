using ImGuiNET;

namespace RainCore.EditorApp;

             
                                                                         
                                                                              
                                                                         
                                                                              
                                                                          
                                                                             
                                                                         
                                                                           
                                                                         
                                                                           
   
                                                                         
                                                                           
                                                                           
                                                                           
                                    
              
public sealed partial class EditorWindow
{
    private int _dbTab;
    private string _dbSelectedId = string.Empty;

    private void DrawDatabaseWindow()
    {
        if (!_showDatabaseWindow) return;

        ImGui.SetNextWindowSize(new System.Numerics.Vector2(720, 480), ImGuiCond.FirstUseEver);
        ImGui.Begin("Database", ref _showDatabaseWindow);

        if (ImGui.BeginTabBar("##DatabaseTabs"))
        {
            if (ImGui.BeginTabItem("Actors")) { _dbTab = 0; DrawActorsTab(); ImGui.EndTabItem(); }
            if (ImGui.BeginTabItem("Skills")) { _dbTab = 1; DrawSkillsTab(); ImGui.EndTabItem(); }
            if (ImGui.BeginTabItem("Items")) { _dbTab = 2; DrawItemsTab(); ImGui.EndTabItem(); }
            if (ImGui.BeginTabItem("Enemies")) { _dbTab = 3; DrawEnemiesTab(); ImGui.EndTabItem(); }
            if (ImGui.BeginTabItem("Troops")) { _dbTab = 4; DrawTroopsTab(); ImGui.EndTabItem(); }
            ImGui.EndTabBar();
        }

        ImGui.Separator();
        if (ImGui.Button("Save Database")) _database.Save(_projectPath);
        ImGui.SameLine();
        ImGui.TextDisabled("Saved to <project>/Database/database.json");

        ImGui.End();
    }

                                                                           
                                                                             
                                                                             
                                                                                   
    private void DrawDatabaseListAndForm(IReadOnlyList<(string Id, string Name)> items,
        Action onNew, Action<string> drawForm)
    {
        ImGui.BeginChild("##DbList", new System.Numerics.Vector2(180, 0), ImGuiChildFlags.Border);
        foreach (var (id, name) in items)
        {
            if (ImGui.Selectable($"{name}##{id}", _dbSelectedId == id))
                _dbSelectedId = id;
        }
        ImGui.Separator();
        if (ImGui.Button("New", new System.Numerics.Vector2(-1, 0))) onNew();
        ImGui.EndChild();

        ImGui.SameLine();
        ImGui.BeginChild("##DbForm", System.Numerics.Vector2.Zero, ImGuiChildFlags.Border);
        if (items.Any(i => i.Id == _dbSelectedId))
            drawForm(_dbSelectedId);
        else
            ImGui.TextDisabled("Select an entry on the left, or create a new one.");
        ImGui.EndChild();
    }

    private static string NextId(string prefix, IEnumerable<string> existingIds)
    {
        var existing = new HashSet<string>(existingIds);
        int n = 1;
        while (existing.Contains($"{prefix}_{n}")) n++;
        return $"{prefix}_{n}";
    }

    private void DrawActorsTab()
    {
        var items = _database.Actors.Select(a => (a.Id, a.Name)).ToList();
        DrawDatabaseListAndForm(items,
            onNew: () =>
            {
                var id = NextId("actor", _database.Actors.Select(a => a.Id));
                _database.Actors.Add(new ActorData(id, id, 100, 20, 10, 10, 10, 10, 10, 10, new List<string>()));
                _dbSelectedId = id;
            },
            drawForm: id =>
            {
                int idx = _database.Actors.FindIndex(a => a.Id == id);
                var a = _database.Actors[idx];

                var name = a.Name;
                if (ImGui.InputText("Name", ref name, 128)) _database.Actors[idx] = a with { Name = name };

                DrawStatFields(
                    a.MaxHp, v => _database.Actors[idx] = _database.Actors[idx] with { MaxHp = v },
                    a.MaxMp, v => _database.Actors[idx] = _database.Actors[idx] with { MaxMp = v },
                    a.Attack, v => _database.Actors[idx] = _database.Actors[idx] with { Attack = v },
                    a.Defense, v => _database.Actors[idx] = _database.Actors[idx] with { Defense = v },
                    a.MagicAttack, v => _database.Actors[idx] = _database.Actors[idx] with { MagicAttack = v },
                    a.MagicDefense, v => _database.Actors[idx] = _database.Actors[idx] with { MagicDefense = v },
                    a.Agility, v => _database.Actors[idx] = _database.Actors[idx] with { Agility = v },
                    a.Luck, v => _database.Actors[idx] = _database.Actors[idx] with { Luck = v });

                ImGui.Separator();
                ImGui.Text("Skills");
                foreach (var skill in _database.Skills)
                {
                    var current = _database.Actors[idx];
                    bool has = current.SkillIds.Contains(skill.Id);
                    if (ImGui.Checkbox(skill.Name, ref has))
                    {
                        var ids = new List<string>(current.SkillIds);
                        if (has) ids.Add(skill.Id); else ids.Remove(skill.Id);
                        _database.Actors[idx] = current with { SkillIds = ids };
                    }
                }

                ImGui.Separator();
                if (ImGui.Button("Delete")) { _database.Actors.RemoveAt(idx); _dbSelectedId = string.Empty; }
            });
    }

    private void DrawSkillsTab()
    {
        var items = _database.Skills.Select(s => (s.Id, s.Name)).ToList();
        DrawDatabaseListAndForm(items,
            onNew: () =>
            {
                var id = NextId("skill", _database.Skills.Select(s => s.Id));
                _database.Skills.Add(new SkillData(id, id, 5, 10, SkillTargetKind.OneEnemy, string.Empty));
                _dbSelectedId = id;
            },
            drawForm: id =>
            {
                int idx = _database.Skills.FindIndex(s => s.Id == id);
                var s = _database.Skills[idx];

                var name = s.Name;
                if (ImGui.InputText("Name", ref name, 128)) _database.Skills[idx] = s with { Name = name };

                int mpCost = s.MpCost;
                if (ImGui.InputInt("MP Cost", ref mpCost)) _database.Skills[idx] = _database.Skills[idx] with { MpCost = Math.Max(0, mpCost) };

                int power = s.Power;
                if (ImGui.InputInt("Power", ref power)) _database.Skills[idx] = _database.Skills[idx] with { Power = power };

                var targetNames = Enum.GetNames<SkillTargetKind>();
                int targetIndex = Array.IndexOf(targetNames, s.Target.ToString());
                if (ImGui.Combo("Target", ref targetIndex, targetNames, targetNames.Length))
                    _database.Skills[idx] = _database.Skills[idx] with { Target = Enum.Parse<SkillTargetKind>(targetNames[targetIndex]) };

                var desc = s.Description;
                if (ImGui.InputTextMultiline("Description", ref desc, 512, new System.Numerics.Vector2(0, 60)))
                    _database.Skills[idx] = _database.Skills[idx] with { Description = desc };

                ImGui.Separator();
                if (ImGui.Button("Delete")) { _database.Skills.RemoveAt(idx); _dbSelectedId = string.Empty; }
            });
    }

    private void DrawItemsTab()
    {
        var items = _database.Items.Select(i => (i.Id, i.Name)).ToList();
        DrawDatabaseListAndForm(items,
            onNew: () =>
            {
                var id = NextId("item", _database.Items.Select(i => i.Id));
                _database.Items.Add(new ItemData(id, id, 0, 0, 0, string.Empty));
                _dbSelectedId = id;
            },
            drawForm: id =>
            {
                int idx = _database.Items.FindIndex(i => i.Id == id);
                var it = _database.Items[idx];

                var name = it.Name;
                if (ImGui.InputText("Name", ref name, 128)) _database.Items[idx] = it with { Name = name };

                int price = it.Price;
                if (ImGui.InputInt("Price", ref price)) _database.Items[idx] = _database.Items[idx] with { Price = Math.Max(0, price) };

                int healHp = it.HealHp;
                if (ImGui.InputInt("Heal HP", ref healHp)) _database.Items[idx] = _database.Items[idx] with { HealHp = healHp };

                int healMp = it.HealMp;
                if (ImGui.InputInt("Heal MP", ref healMp)) _database.Items[idx] = _database.Items[idx] with { HealMp = healMp };

                var desc = it.Description;
                if (ImGui.InputTextMultiline("Description", ref desc, 512, new System.Numerics.Vector2(0, 60)))
                    _database.Items[idx] = _database.Items[idx] with { Description = desc };

                ImGui.Separator();
                if (ImGui.Button("Delete")) { _database.Items.RemoveAt(idx); _dbSelectedId = string.Empty; }
            });
    }

    private void DrawEnemiesTab()
    {
        var items = _database.Enemies.Select(e => (e.Id, e.Name)).ToList();
        DrawDatabaseListAndForm(items,
            onNew: () =>
            {
                var id = NextId("enemy", _database.Enemies.Select(e => e.Id));
                _database.Enemies.Add(new EnemyData(id, id, 50, 0, 8, 8, 8, 8, 8, 8, new List<string>(), 10, 10));
                _dbSelectedId = id;
            },
            drawForm: id =>
            {
                int idx = _database.Enemies.FindIndex(e => e.Id == id);
                var e = _database.Enemies[idx];

                var name = e.Name;
                if (ImGui.InputText("Name", ref name, 128)) _database.Enemies[idx] = e with { Name = name };

                DrawStatFields(
                    e.MaxHp, v => _database.Enemies[idx] = _database.Enemies[idx] with { MaxHp = v },
                    e.MaxMp, v => _database.Enemies[idx] = _database.Enemies[idx] with { MaxMp = v },
                    e.Attack, v => _database.Enemies[idx] = _database.Enemies[idx] with { Attack = v },
                    e.Defense, v => _database.Enemies[idx] = _database.Enemies[idx] with { Defense = v },
                    e.MagicAttack, v => _database.Enemies[idx] = _database.Enemies[idx] with { MagicAttack = v },
                    e.MagicDefense, v => _database.Enemies[idx] = _database.Enemies[idx] with { MagicDefense = v },
                    e.Agility, v => _database.Enemies[idx] = _database.Enemies[idx] with { Agility = v },
                    e.Luck, v => _database.Enemies[idx] = _database.Enemies[idx] with { Luck = v });

                int exp = e.ExpReward;
                if (ImGui.InputInt("EXP reward", ref exp)) _database.Enemies[idx] = _database.Enemies[idx] with { ExpReward = Math.Max(0, exp) };

                int gold = e.GoldReward;
                if (ImGui.InputInt("Gold reward", ref gold)) _database.Enemies[idx] = _database.Enemies[idx] with { GoldReward = Math.Max(0, gold) };

                ImGui.Separator();
                ImGui.Text("Skills");
                foreach (var skill in _database.Skills)
                {
                    var current = _database.Enemies[idx];
                    bool has = current.SkillIds.Contains(skill.Id);
                    if (ImGui.Checkbox(skill.Name, ref has))
                    {
                        var ids = new List<string>(current.SkillIds);
                        if (has) ids.Add(skill.Id); else ids.Remove(skill.Id);
                        _database.Enemies[idx] = current with { SkillIds = ids };
                    }
                }

                ImGui.Separator();
                if (ImGui.Button("Delete")) { _database.Enemies.RemoveAt(idx); _dbSelectedId = string.Empty; }
            });
    }

    private void DrawTroopsTab()
    {
        var items = _database.Troops.Select(t => (t.Id, t.Name)).ToList();
        DrawDatabaseListAndForm(items,
            onNew: () =>
            {
                var id = NextId("troop", _database.Troops.Select(t => t.Id));
                _database.Troops.Add(new TroopData(id, id, new List<string>()));
                _dbSelectedId = id;
            },
            drawForm: id =>
            {
                int idx = _database.Troops.FindIndex(t => t.Id == id);
                var t = _database.Troops[idx];

                var name = t.Name;
                if (ImGui.InputText("Name", ref name, 128)) _database.Troops[idx] = t with { Name = name };

                ImGui.TextDisabled($"Id: {t.Id} - используется в команде события \"Battle...\" (Troop Id)");
                ImGui.Separator();
                ImGui.Text("Enemies in this troop");
                                                                          
                                                                            
                                                                             
                                                      
                foreach (var enemy in _database.Enemies)
                {
                    var current = _database.Troops[idx];
                    bool has = current.EnemyIds.Contains(enemy.Id);
                    if (ImGui.Checkbox(enemy.Name, ref has))
                    {
                        var ids = new List<string>(current.EnemyIds);
                        if (has) ids.Add(enemy.Id); else ids.Remove(enemy.Id);
                        _database.Troops[idx] = current with { EnemyIds = ids };
                    }
                }

                ImGui.Separator();
                if (ImGui.Button("Delete")) { _database.Troops.RemoveAt(idx); _dbSelectedId = string.Empty; }
            });
    }

                                                                         
                                                                               
                                                                              
    private static void DrawStatFields(
        int maxHp, Action<int> setMaxHp, int maxMp, Action<int> setMaxMp,
        int attack, Action<int> setAttack, int defense, Action<int> setDefense,
        int magicAttack, Action<int> setMagicAttack, int magicDefense, Action<int> setMagicDefense,
        int agility, Action<int> setAgility, int luck, Action<int> setLuck)
    {
        int v;
        v = maxHp; if (ImGui.InputInt("Max HP", ref v)) setMaxHp(Math.Max(1, v));
        v = maxMp; if (ImGui.InputInt("Max MP", ref v)) setMaxMp(Math.Max(0, v));
        v = attack; if (ImGui.InputInt("Attack", ref v)) setAttack(v);
        v = defense; if (ImGui.InputInt("Defense", ref v)) setDefense(v);
        v = magicAttack; if (ImGui.InputInt("Magic Attack", ref v)) setMagicAttack(v);
        v = magicDefense; if (ImGui.InputInt("Magic Defense", ref v)) setMagicDefense(v);
        v = agility; if (ImGui.InputInt("Agility", ref v)) setAgility(v);
        v = luck; if (ImGui.InputInt("Luck", ref v)) setLuck(v);
    }
}
