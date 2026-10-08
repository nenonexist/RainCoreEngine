namespace RainCore;

             
                                                                     
                                                                        
                                                                            
                                                      
              
public class EventRunner
{
    private Queue<EventCommand>? _queue;
    private IEnumerator<EventWait>? _currentStep;
    private float _waitRemaining;
    private IEventContext? _ctx;

    public bool IsBusy => _queue != null;

    public void Start(IEnumerable<EventCommand> commands, IEventContext ctx)
    {
        if (IsBusy) return;                       
        _queue = new Queue<EventCommand>(commands);
        _ctx = ctx;
        _currentStep = null;
        _waitRemaining = 0f;
    }

                                                                      
    public void Tick(float dt)
    {
        if (_queue == null) return;

        if (_waitRemaining > 0f)
        {
            _waitRemaining -= dt;
            return;
        }

        while (true)
        {
            if (_currentStep == null)
            {
                if (_queue.Count == 0)
                {
                    _queue = null;                              
                    return;
                }
                _currentStep = _queue.Dequeue().Run(_ctx!);
            }

            if (_currentStep.MoveNext())
            {
                _waitRemaining = _currentStep.Current.Seconds;
                return;                                                                    
            }

            _currentStep = null;                                                                 
        }
    }
}
