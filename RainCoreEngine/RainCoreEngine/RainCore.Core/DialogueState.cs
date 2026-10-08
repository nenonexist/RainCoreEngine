namespace RainCore;

             
                                                                     
                                                                           
                                                                          
                                                                             
                                                                             
                                                                            
                                                                          
                                                                            
                                                                             
                            
   
                                                                           
                                                                             
                                                                              
                                    
              
public sealed class DialogueState
{
    public string? Speaker { get; private set; }
    public string? Text { get; private set; }
    public bool HasDialogue => Text != null;

    public string? NoteTitle { get; private set; }
    public string? NoteText { get; private set; }
    public bool HasNote => NoteText != null;

    public string? ChoicePrompt { get; private set; }
    public IReadOnlyList<string>? ChoiceOptions { get; private set; }
    private int? _chosenIndex;

    public bool IsChoicePending => ChoiceOptions != null;

    public void ShowDialogue(string speaker, string text)
    {
        Speaker = speaker;
        Text = text;
        NoteTitle = null;                                                                                
        NoteText = null;
    }

                                                                           
                                                                               
                                                                   
                                                                              
                                                                                  
    public void ClearDialogue()
    {
        Speaker = null;
        Text = null;
    }

    public void ShowNote(string title, string text)
    {
        NoteTitle = title;
        NoteText = text;
        Speaker = null;
        Text = null;
    }

    public void ClearNote()
    {
        NoteTitle = null;
        NoteText = null;
    }

    public void BeginChoice(string prompt, IReadOnlyList<string> choices)
    {
        ChoicePrompt = prompt;
        ChoiceOptions = choices;
        _chosenIndex = null;
    }

                                                                
                                                                          
                                                                               
                                                                             
                                                                               
    public void ResolveChoice(int index)
    {
        _chosenIndex = index;
        ChoicePrompt = null;
        ChoiceOptions = null;
    }

    public int? ConsumeChoice()
    {
        var result = _chosenIndex;
        _chosenIndex = null;
        return result;
    }
}
