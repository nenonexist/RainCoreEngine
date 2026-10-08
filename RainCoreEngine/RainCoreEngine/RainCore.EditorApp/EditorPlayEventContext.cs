using OpenTK.Mathematics;

namespace RainCore.EditorApp;

             
                                                                             
                                                                 
                                                                   
                                                                              
                                                                     
                                                                        
                                                                           
                                        
              
public sealed class EditorPlayEventContext : IEventContext
{
    private readonly Action<Vector3> _teleportPlayer;
    private readonly MeshRoomLevel _level;
    private readonly Camera _camera;
    private readonly Action<string> _log;
    private readonly DialogueState _dialogue;
    private readonly BattleState _battle;
    private readonly GameDatabase _database;
    private readonly PartyState _party;
    private readonly ScriptHost _scripts;

    public EditorPlayEventContext(Action<Vector3> teleportPlayer, MeshRoomLevel level, Camera camera,
        Action<string> log, DialogueState dialogue, BattleState battle, GameDatabase database, PartyState party, ScriptHost scripts)
    {
        _teleportPlayer = teleportPlayer;
        _level = level;
        _camera = camera;
        _log = log;
        _dialogue = dialogue;
        _battle = battle;
        _database = database;
        _party = party;
        _scripts = scripts;
    }

    public void TeleportPlayer(Vector3 pos) => _teleportPlayer(pos);

    public void SetWeather(bool active) => _log($"Погода: {(active ? "вкл" : "выкл")} (в Play Mode редактора нет системы погоды)");

                                                                               
                                                                             
                                                                                
                                                                              
    public void WakeFromRandomDream() => _log("WakeFromRandomDream (нет случайных снов в Play Mode редактора)");

    public void AddNpc(Npc npc) => _level.AddNpc(npc);
    public void RemoveNpc(Npc npc) => _level.RemoveNpc(npc);

    public bool TryMoveNpc(string name, Vector3 offset)
    {
        var npc = _level.Npcs.FirstOrDefault(n => n.Name.Equals(name, StringComparison.OrdinalIgnoreCase));
        return npc != null && npc.MoveBy(offset);
    }

    public Vector3? GetNpcPosition(string name)
    {
        var npc = _level.Npcs.FirstOrDefault(n => n.Name.Equals(name, StringComparison.OrdinalIgnoreCase));
        return npc?.Position;
    }

    public Vector3 GetCameraPosition() => _camera.Position;
    public Vector3 GetCameraForward() => _camera.Front;

    public void AddItem(string name, int amount) => Inventory.Add(name, amount);
    public bool RemoveItem(string name, int amount) => Inventory.Remove(name, amount);

    public void PlaySound(string name) => _log($"Звук: {name} (в Play Mode редактора нет аудио-системы)");

    public void RunScript(string scriptFile, string functionName, params string[] args) =>
        _scripts.CallFunction(this, scriptFile, functionName, args);

    public void ShowDialogue(string speaker, string text) => _dialogue.ShowDialogue(speaker, text);

    public void ShowNote(string title, string text) => _dialogue.ShowNote(title, text);

                                                                          
                                                                      
                                                                         
                                                                              
                                                                         
    public void BeginChoice(string prompt, IReadOnlyList<string> choices) => _dialogue.BeginChoice(prompt, choices);
    public bool IsChoicePending => _dialogue.IsChoicePending;
    public int? ConsumeChoice() => _dialogue.ConsumeChoice();

    public void FadeScreen(bool fadeOut, float seconds) => _log($"Fade {(fadeOut ? "out" : "in")} {seconds}с (экранные эффекты - TODO)");
    public void FlashScreen(float seconds) => _log($"Flash {seconds}с (экранные эффекты - TODO)");
    public void ShakeScreen(float seconds, float strength) => _log($"Shake {seconds}с x{strength} (экранные эффекты - TODO)");
    public void ShowPicture(int id, string fileName, float x, float y, float scale, float opacity) => _log($"ShowPicture #{id}: {fileName} (экранные эффекты - TODO)");
    public void HidePicture(int id) => _log($"HidePicture #{id}");

                                                                            
                                                                     
                                                                             
                                                                            
                                                             
    public bool StartBattle(string troopId)
    {
        _battle.Start(_database, troopId, _party);
        return _battle.IsActive;
    }
    public bool IsBattleActive => _battle.IsActive;
}
