using ImGuiNET;

namespace RainCore.EditorApp;

             
                                                                              
                                                                            
                                                                               
                                                                               
                                                                         
                         
   
                                                                                  
                                                                        
                                                                           
                                                                        
                                                                           
                                                                           
                           
              
public sealed partial class EditorWindow
{
    private void DrawPlayModeDialogueOverlay()
    {
        var dlg = _dialogueState;
        if (dlg == null) return;

        var viewport = ImGui.GetMainViewport();
        const float boxWidth = 700f;
        const float boxHeight = 110f;
        var pos = new System.Numerics.Vector2(
            viewport.WorkPos.X + (viewport.WorkSize.X - boxWidth) * 0.5f,
            viewport.WorkPos.Y + viewport.WorkSize.Y - boxHeight - 24f);

        const ImGuiWindowFlags boxFlags = ImGuiWindowFlags.NoDecoration | ImGuiWindowFlags.NoDocking
            | ImGuiWindowFlags.NoResize | ImGuiWindowFlags.NoMove | ImGuiWindowFlags.NoSavedSettings
            | ImGuiWindowFlags.NoFocusOnAppearing | ImGuiWindowFlags.NoNav;

        if (dlg.HasDialogue)
        {
            ImGui.SetNextWindowPos(pos);
            ImGui.SetNextWindowSize(new System.Numerics.Vector2(boxWidth, boxHeight));
            ImGui.SetNextWindowBgAlpha(0.85f);
            ImGui.Begin("##PlayDialogue", boxFlags);
            if (!string.IsNullOrEmpty(dlg.Speaker))
            {
                ImGui.TextColored(new System.Numerics.Vector4(1f, 0.85f, 0.4f, 1f), dlg.Speaker);
                ImGui.Separator();
            }
            ImGui.TextWrapped(dlg.Text!);
            ImGui.End();
        }
        else if (dlg.HasNote)
        {
            ImGui.SetNextWindowPos(pos);
            ImGui.SetNextWindowSize(new System.Numerics.Vector2(boxWidth, boxHeight));
            ImGui.SetNextWindowBgAlpha(0.85f);
            ImGui.Begin("##PlayNote", boxFlags);
            ImGui.TextColored(new System.Numerics.Vector4(0.6f, 0.85f, 1f, 1f), $"[Note] {dlg.NoteTitle}");
            ImGui.Separator();
            ImGui.TextWrapped(dlg.NoteText!);
            ImGui.End();
        }

                                                                               
                                                                            
                                                                    
        if (dlg.IsChoicePending)
        {
            var options = dlg.ChoiceOptions!;
            var choiceHeight = 44f + options.Count * 34f;
            var choicePos = new System.Numerics.Vector2(pos.X, pos.Y - choiceHeight - 12f);

            ImGui.SetNextWindowPos(choicePos);
            ImGui.SetNextWindowSize(new System.Numerics.Vector2(boxWidth, choiceHeight));
            ImGui.SetNextWindowBgAlpha(0.9f);
            const ImGuiWindowFlags choiceFlags = ImGuiWindowFlags.NoDecoration | ImGuiWindowFlags.NoDocking
                | ImGuiWindowFlags.NoResize | ImGuiWindowFlags.NoMove | ImGuiWindowFlags.NoSavedSettings;
            ImGui.Begin("##PlayChoice", choiceFlags);

            if (!string.IsNullOrEmpty(dlg.ChoicePrompt))
            {
                ImGui.TextWrapped(dlg.ChoicePrompt);
                ImGui.Separator();
            }
            for (int i = 0; i < options.Count; i++)
            {
                ImGui.PushID(i);
                if (ImGui.Button(options[i], new System.Numerics.Vector2(boxWidth - 20f, 0)))
                    dlg.ResolveChoice(i);
                ImGui.PopID();
            }
            ImGui.End();
        }
    }

                                                                                
                                                                             
                                                                            
                                                                              
                                                                              
                                                      
    private void DrawBattleOverlay()
    {
        var battle = _battle!;
        var viewport = ImGui.GetMainViewport();

        var pos = new System.Numerics.Vector2(viewport.WorkPos.X + 40f, viewport.WorkPos.Y + 40f);
        var size = new System.Numerics.Vector2(viewport.WorkSize.X - 80f, viewport.WorkSize.Y - 80f);
        ImGui.SetNextWindowPos(pos);
        ImGui.SetNextWindowSize(size);
        ImGui.SetNextWindowBgAlpha(0.92f);
        const ImGuiWindowFlags flags = ImGuiWindowFlags.NoDecoration | ImGuiWindowFlags.NoDocking
            | ImGuiWindowFlags.NoResize | ImGuiWindowFlags.NoMove | ImGuiWindowFlags.NoSavedSettings;
        ImGui.Begin("##Battle", flags);

        ImGui.Columns(2, "##BattleColumns", false);

        ImGui.TextColored(new System.Numerics.Vector4(0.6f, 0.85f, 1f, 1f), "Party");
        foreach (var c in battle.Party)
            ImGui.TextWrapped($"{c.Name}   HP {c.Hp}/{c.MaxHp}   MP {c.Mp}/{c.MaxMp}{(c.IsAlive ? "" : "  (down)")}");

        ImGui.NextColumn();

        ImGui.TextColored(new System.Numerics.Vector4(1f, 0.6f, 0.6f, 1f), "Enemies");
        foreach (var c in battle.Enemies)
            ImGui.TextWrapped($"{c.Name}   HP {c.Hp}/{c.MaxHp}{(c.IsAlive ? "" : "  (defeated)")}");

        ImGui.Columns(1);
        ImGui.Separator();

        ImGui.BeginChild("##BattleLog", new System.Numerics.Vector2(0, 140), ImGuiChildFlags.Border);
        foreach (var line in battle.Log) ImGui.TextWrapped(line);
        ImGui.SetScrollHereY(1f);
        ImGui.EndChild();

        if (battle.Outcome is { } outcome)
        {
            ImGui.Separator();
            ImGui.TextUnformatted(outcome switch
            {
                BattleOutcome.Victory => "Victory!",
                BattleOutcome.Defeat => "Defeat...",
                _ => "Escaped.",
            });
            if (ImGui.Button("Continue")) battle.ConsumeOutcome();
        }
        else if (battle.AwaitingActionFrom is { } actor)
        {
            ImGui.Separator();
            ImGui.Text($"{actor.Name}'s turn");
            if (ImGui.Button("Attack")) battle.SubmitPlayerAction(BattleActionKind.Attack);
            ImGui.SameLine();
            if (ImGui.Button("Flee")) battle.SubmitPlayerAction(BattleActionKind.Flee);

            foreach (var skillId in actor.SkillIds)
            {
                var skill = _database.FindSkill(skillId);
                if (skill == null) continue;
                ImGui.SameLine();
                if (ImGui.Button($"{skill.Name} ({skill.MpCost} MP)") && actor.Mp >= skill.MpCost)
                    battle.SubmitPlayerAction(BattleActionKind.Skill, skill.Id);
            }
        }

        ImGui.End();
    }
}
