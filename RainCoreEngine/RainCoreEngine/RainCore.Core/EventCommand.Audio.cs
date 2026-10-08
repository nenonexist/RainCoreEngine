namespace RainCore;


using OpenTK.Mathematics;

                                                             
                                                           


             
                                                                      
                                                                          
                                                                      
                                                                       
                                                                  
                            
              
public class PlaySoundCommand : EventCommand
{
    private readonly string _path;
    public string Path => _path;
    public PlaySoundCommand(string path)
    {
        _path = path;
    }


    public override IEnumerator<EventWait> Run(IEventContext ctx)
    {
        ctx.PlaySound(_path);
        yield return EventWait.OneFrame;
    }
}
