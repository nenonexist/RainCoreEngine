namespace RainCore;

             
                                                                       
                                                                        
                                                                   
                                                                         
                                                                  
   
                                                                    
                                                                            
                                                                        
                                        
              
public class BackgroundEventRunner
{
    private class Thread
    {
        public Queue<EventCommand> Queue = null!;
        public IEnumerator<EventWait>? CurrentStep;
        public float WaitRemaining;
        public IEventContext Ctx = null!;
    }

    private readonly List<Thread> _threads = new();

    public bool IsBusy => _threads.Count > 0;

                                                                                                               
    public void Start(IEnumerable<EventCommand> commands, IEventContext ctx)
    {
        _threads.Add(new Thread
        {
            Queue = new Queue<EventCommand>(commands),
            Ctx = ctx,
        });
    }

                                                                                                                
    public void Tick(float dt)
    {
        for (int i = _threads.Count - 1; i >= 0; i--)
        {
            var t = _threads[i];

            if (t.WaitRemaining > 0f)
            {
                t.WaitRemaining -= dt;
                continue;
            }

            while (true)
            {
                if (t.CurrentStep == null)
                {
                    if (t.Queue.Count == 0)
                    {
                        _threads.RemoveAt(i);                                          
                        break;
                    }
                    t.CurrentStep = t.Queue.Dequeue().Run(t.Ctx);
                }

                if (t.CurrentStep.MoveNext())
                {
                    t.WaitRemaining = t.CurrentStep.Current.Seconds;
                    break;                                                                    
                }

                t.CurrentStep = null;                                                             
            }
        }
    }
}
