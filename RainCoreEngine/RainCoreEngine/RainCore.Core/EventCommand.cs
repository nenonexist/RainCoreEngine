namespace RainCore;


using OpenTK.Mathematics;

                                                                                 
public interface IWorldApi
{
    void TeleportPlayer(Vector3 pos);
    void SetWeather(bool active);
    void AddNpc(Npc npc);
    void RemoveNpc(Npc npc);
    bool TryMoveNpc(string name, Vector3 offset);
                                                                                           
    Vector3? GetNpcPosition(string name);
                                                                                                  
    Vector3 GetCameraPosition();
                                                                             
    Vector3 GetCameraForward();
    void AddItem(string name, int amount);
    bool RemoveItem(string name, int amount);

                                                                                  
                                                                                            
    void WakeFromRandomDream();
}


                            
public interface IAudioApi
{
    void PlaySound(string name);
}


                                                                                        
                                                                          
                                                                       
                                                                              
                                                                     
                                                                          
                                                                   
                                                          
public interface IScriptApi
{
    void RunScript(string scriptFile, string functionName, params string[] args);
}


                                                      
public interface IDialogueApi
{
    void ShowDialogue(string speaker, string text);
    void ShowNote(string title, string text);
    void BeginChoice(string prompt, IReadOnlyList<string> choices);
    bool IsChoicePending { get; }
    int? ConsumeChoice();
}


                                                           
public interface IScreenFxApi
{
    void FadeScreen(bool fadeOut, float seconds);
    void FlashScreen(float seconds);
    void ShakeScreen(float seconds, float strength);

                                                                                         
                                                                                                
    void ShowPicture(int id, string fileName, float x, float y, float scale, float opacity);

                                                                                                     
    void HidePicture(int id);
}


             
                                                                             
                                                                        
                                                                            
                                                                    
              
public interface IEventContext : IWorldApi, IAudioApi, IDialogueApi, IScreenFxApi, IBattleApi, IScriptApi { }


             
                                                                       
                                                                        
                                                                       
                                                   
              
public abstract class EventCommand
{
    public abstract IEnumerator<EventWait> Run(IEventContext ctx);
}


                                                                                
public readonly struct EventWait
{
    public readonly float Seconds;
    public EventWait(float seconds)
    {
        Seconds = seconds;
    }


    public static readonly EventWait OneFrame = new(0f);
}


public interface ITeleportCommand
{
    IEnumerator<EventWait> Run(IEventContext ctx);
}
