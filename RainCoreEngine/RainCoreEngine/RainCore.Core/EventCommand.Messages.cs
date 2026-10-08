namespace RainCore;


using OpenTK.Mathematics;

                                                                    


                                                                                                     
public class ShowMessage : EventCommand
{
    private readonly string _speaker, _text;
    private readonly float _holdSeconds;
    public string Speaker => _speaker;
    public string Text => _text;
    public float HoldSeconds => _holdSeconds;

    public ShowMessage(string speaker, string text, float holdSeconds = 3f)
    {
        _speaker = speaker;
        _text = text;
        _holdSeconds = holdSeconds;
    }

    public override IEnumerator<EventWait> Run(IEventContext ctx)
    {
        ctx.ShowDialogue(_speaker, _text);
        yield return new EventWait(_holdSeconds);
    }
}


             
                                                                            
                                                                          
                                                                            
                                        
              
public class ShowNoteCommand : EventCommand
{
    private readonly string _title, _text;
    public string Title => _title;
    public string Text => _text;

    public ShowNoteCommand(string title, string text)
    {
        _title = title;
        _text = text;
    }

    public override IEnumerator<EventWait> Run(IEventContext ctx)
    {
        ctx.ShowNote(_title, _text);
        yield return new EventWait(3f);
    }
}


public sealed class ShowChoicesCommand : EventCommand
{
    private readonly string _prompt;
    private readonly string[] _choices;
    public string Prompt => _prompt;
    public IReadOnlyList<string> Choices => _choices;

    public ShowChoicesCommand(string prompt, params string[] choices)
    {
        _prompt = prompt;
        _choices = choices.Length == 0 ? new[] { "Продолжить" } : choices;
    }

    public override IEnumerator<EventWait> Run(IEventContext ctx)
    {
        ctx.BeginChoice(_prompt, _choices);
        while (ctx.IsChoicePending)
            yield return EventWait.OneFrame;
    }
}
