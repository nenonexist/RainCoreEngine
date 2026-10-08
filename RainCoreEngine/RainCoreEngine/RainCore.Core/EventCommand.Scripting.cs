namespace RainCore;

                                                                             
                                                                           
                                                                           
                                                                           

             
                                                                             
                                                                            
                                                                      
                                                                           
                                                     
              
public class RunScriptCommand : EventCommand
{
    private readonly string _scriptFile;
    private readonly string _functionName;
    private readonly string[] _args;

    public string ScriptFile => _scriptFile;
    public string FunctionName => _functionName;
    public string[] Args => _args;

    public RunScriptCommand(string scriptFile, string functionName, string[] args)
    {
        _scriptFile = scriptFile;
        _functionName = functionName;
        _args = args;
    }

    public override IEnumerator<EventWait> Run(IEventContext ctx)
    {
        ctx.RunScript(_scriptFile, _functionName, _args);
        yield return EventWait.OneFrame;
    }
}
