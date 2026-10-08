namespace RainCore;


using OpenTK.Mathematics;

                                                                                   


public sealed class FadeScreenCommand : EventCommand
{
    private readonly bool _fadeOut;
    private readonly float _seconds;
    public bool FadeOut => _fadeOut;
    public float Seconds => _seconds;
    public FadeScreenCommand(bool fadeOut, float seconds) { _fadeOut = fadeOut; _seconds = seconds; }
    public override IEnumerator<EventWait> Run(IEventContext ctx)
    {
        ctx.FadeScreen(_fadeOut, _seconds);
        yield return new EventWait(_seconds);
    }
}


public sealed class FlashScreenCommand : EventCommand
{
    private readonly float _seconds;
    public float Seconds => _seconds;
    public FlashScreenCommand(float seconds) { _seconds = seconds; }
    public override IEnumerator<EventWait> Run(IEventContext ctx)
    {
        ctx.FlashScreen(_seconds);
        yield return new EventWait(_seconds);
    }
}


public sealed class ShakeScreenCommand : EventCommand
{
    private readonly float _seconds;
    private readonly float _strength;
    public float Seconds => _seconds;
    public float Strength => _strength;
    public ShakeScreenCommand(float seconds, float strength) { _seconds = seconds; _strength = strength; }
    public override IEnumerator<EventWait> Run(IEventContext ctx)
    {
        ctx.ShakeScreen(_seconds, _strength);
        yield return new EventWait(_seconds);
    }
}


                                               
                                                                                                                  
public class SetWeatherCommand : EventCommand
{
    private readonly bool _active;
    private readonly float _waitSeconds;
    public bool Active => _active;
    public float WaitSeconds => _waitSeconds;
    public SetWeatherCommand(bool active, float waitSeconds = 0f) { _active = active; _waitSeconds = waitSeconds; }

    public override IEnumerator<EventWait> Run(IEventContext ctx)
    {
        ctx.SetWeather(_active);
        yield return _waitSeconds > 0f ? new EventWait(_waitSeconds) : EventWait.OneFrame;
    }
}


                                                                              
                                                                                
                                                                        
                                                                         
                                                                            
                                                                                          
public sealed class ShowPictureCommand : EventCommand
{
    private readonly string _fileName;
    private readonly int _id;
    private readonly float _x;
    private readonly float _y;
    private readonly float _scale;
    private readonly float _opacity;
    public string FileName => _fileName;
    public int Id => _id;
    public float X => _x;
    public float Y => _y;
    public float Scale => _scale;
    public float Opacity => _opacity;

    public ShowPictureCommand(string fileName, int id, float x, float y, float scale, float opacity)
    {
        _fileName = fileName;
        _id = id;
        _x = x;
        _y = y;
        _scale = scale;
        _opacity = opacity;
    }

    public override IEnumerator<EventWait> Run(IEventContext ctx)
    {
        ctx.ShowPicture(_id, _fileName, _x, _y, _scale, _opacity);
        yield return EventWait.OneFrame;
    }
}


                                                                           
                                                                           
public sealed class HidePictureCommand : EventCommand
{
    private readonly int _id;
    public int Id => _id;
    public HidePictureCommand(int id) { _id = id; }

    public override IEnumerator<EventWait> Run(IEventContext ctx)
    {
        ctx.HidePicture(_id);
        yield return EventWait.OneFrame;
    }
}