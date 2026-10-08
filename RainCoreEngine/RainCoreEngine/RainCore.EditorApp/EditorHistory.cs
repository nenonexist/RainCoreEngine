namespace RainCore.EditorApp;

             
                                                                                               
                                                                                        
                                                                                         
                                                                                          
                                                                                       
                                                         
                                                                                
              
public sealed class EditorHistory
{
    public const int MaxDepth = 100;

    private readonly record struct Entry(string Json, string Label);

                                                                                       
    private readonly List<Entry> _undo = new();
    private readonly List<Entry> _redo = new();
    private string _current = string.Empty;
    private string _saved = string.Empty;

    public bool HasBaseline => _current.Length > 0;
    public bool IsDirty => HasBaseline && _current != _saved;
    public int UndoCount => _undo.Count;
    public int RedoCount => _redo.Count;
    public string Current => _current;

    public string? NextUndoLabel => _undo.Count > 0 ? _undo[^1].Label : null;
    public string? NextRedoLabel => _redo.Count > 0 ? _redo[^1].Label : null;

                                                                                                      
    public IEnumerable<string> UndoLabels()
    {
        for (int i = _undo.Count - 1; i >= 0; i--) yield return _undo[i].Label;
    }

    public IEnumerable<string> RedoLabels()
    {
        for (int i = _redo.Count - 1; i >= 0; i--) yield return _redo[i].Label;
    }

                                                                                                               
    public void Reset(string json)
    {
        _undo.Clear();
        _redo.Clear();
        _current = json;
        _saved = json;
    }

    public void MarkSaved() => _saved = _current;

                                                                                                  
                                                                                                            
    public bool Commit(string json, string label)
    {
        if (!HasBaseline) { Reset(json); return false; }
        if (json == _current) return false;

        _undo.Add(new Entry(_current, label));
        if (_undo.Count > MaxDepth) _undo.RemoveAt(0);
        _redo.Clear();
        _current = json;
        return true;
    }

                                                                                                      
                                                               
    public string? Undo(int steps = 1)
    {
        if (_undo.Count == 0) return null;
        steps = Math.Clamp(steps, 1, _undo.Count);
        for (int i = 0; i < steps; i++)
        {
            var entry = _undo[^1];
            _undo.RemoveAt(_undo.Count - 1);
            _redo.Add(new Entry(_current, entry.Label));
            _current = entry.Json;
        }
        return _current;
    }

    public string? Redo(int steps = 1)
    {
        if (_redo.Count == 0) return null;
        steps = Math.Clamp(steps, 1, _redo.Count);
        for (int i = 0; i < steps; i++)
        {
            var entry = _redo[^1];
            _redo.RemoveAt(_redo.Count - 1);
            _undo.Add(new Entry(_current, entry.Label));
            _current = entry.Json;
        }
        return _current;
    }
}
