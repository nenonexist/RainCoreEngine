using MoonSharp.Interpreter;
using OpenTK.Mathematics;

namespace RainCore;

             
                                                                          
                                                                           
                                                                                     
                                                                           
                                                                     
                                                                             
                                                                             
                                                   
              
public sealed class ScriptHost
{
    private sealed class Loaded { public required Script Script; public required DateTime LastWriteUtc; }

    private readonly string _scriptsRoot;
    private readonly Action<string> _log;
    private readonly Dictionary<string, Loaded> _loaded = new(StringComparer.OrdinalIgnoreCase);
    private readonly Random _rng = new();
    private IEventContext? _ctx;

    public ScriptHost(string projectPath, Action<string> log)
    {
        _scriptsRoot = Path.GetFullPath(Path.Combine(projectPath, "Scripts"));
        _log = log;
    }

    public void CallFunction(IEventContext context, string scriptFile, string functionName, string[] args)
    {
        try
        {
            var script = GetOrLoad(scriptFile);
            if (script == null) return;
            var fn = script.Globals.Get(functionName);
            if (fn.Type != DataType.Function) { _log($"[Script] В {scriptFile} нет функции \"{functionName}\"."); return; }
            _ctx = context;
            script.Call(fn, args.Select(DynValue.NewString).ToArray());
        }
        catch (InterpreterException ex) { _log($"[Script] {scriptFile}:{functionName} - {ex.DecoratedMessage ?? ex.Message}"); }
        catch (Exception ex) { _log($"[Script] {scriptFile}:{functionName} - {ex.Message}"); }
        finally { _ctx = null; }
    }

                                                                                                      
                                                                                                     
                                                                                                              
    public static bool CheckSyntax(string source, out string message, out int line)
    {
        message = string.Empty;
        line = 0;
        try
        {
            var script = new Script(CoreModules.Preset_SoftSandbox);
            script.LoadString(source, null, "script");
            return true;
        }
        catch (InterpreterException ex)
        {
            message = ex.DecoratedMessage ?? ex.Message;
                                                                                                      
            var open = message.IndexOf('(');
            var comma = open >= 0 ? message.IndexOf(',', open) : -1;
            if (open >= 0 && comma > open && int.TryParse(message.AsSpan(open + 1, comma - open - 1), out var parsed))
                line = parsed;
            return false;
        }
        catch (Exception ex)
        {
            message = ex.Message;
            return false;
        }
    }

    private Script? GetOrLoad(string scriptFile)
    {
        var fullPath = Path.GetFullPath(Path.Combine(_scriptsRoot, scriptFile));
        if (!fullPath.StartsWith(_scriptsRoot, StringComparison.OrdinalIgnoreCase))
        { _log($"[Script] Путь \"{scriptFile}\" выходит за пределы Scripts/ - пропущен."); return null; }
        if (!File.Exists(fullPath)) { _log($"[Script] Файл не найден: {fullPath}"); return null; }

        var lastWrite = File.GetLastWriteTimeUtc(fullPath);
        if (_loaded.TryGetValue(fullPath, out var cached) && cached.LastWriteUtc == lastWrite) return cached.Script;

        var script = new Script(CoreModules.Preset_SoftSandbox);
        script.Options.DebugPrint = text => _log($"[Lua] {text}");
        script.Globals.Set("Game", DynValue.NewTable(BuildGameApi(script)));
        script.DoString(File.ReadAllText(fullPath), null, Path.GetFileName(fullPath));
        _loaded[fullPath] = new Loaded { Script = script, LastWriteUtc = lastWrite };
        return script;
    }

    private Table BuildGameApi(Script script)
    {
        var game = new Table(script);
        void Add(string name, Func<CallbackArguments, DynValue> body) => game.Set(name, DynValue.NewCallback((_, a) => body(a)));

        Add("GetFlag", a => DynValue.NewBoolean(GameFlags.Get(a[0].CastToString() ?? "")));
        Add("SetFlag", a => { GameFlags.Set(a[0].CastToString() ?? "", a[1].CastToBool()); return DynValue.Nil; });
        Add("GetVar", a => DynValue.NewNumber(GameFlags.GetVar(a[0].CastToString() ?? "")));
        Add("SetVar", a => { GameFlags.SetVar(a[0].CastToString() ?? "", (int)Math.Round(a[1].CastToNumber() ?? 0d)); return DynValue.Nil; });
        Add("Random", a =>
        {
            int min = (int)Math.Round(a[0].CastToNumber() ?? 0d), max = (int)Math.Round(a[1].CastToNumber() ?? 0d);
            if (max < min) (min, max) = (max, min);
            return DynValue.NewNumber(_rng.Next(min, max + 1));
        });
        Add("Log", a => { _log($"[Lua] {a[0].CastToString() ?? ""}"); return DynValue.Nil; });
        Add("PlaySound", a => { _ctx?.PlaySound(a[0].CastToString() ?? ""); return DynValue.Nil; });
        Add("Teleport", a =>
        {
            _ctx?.TeleportPlayer(new Vector3((float)(a[0].CastToNumber() ?? 0d), (float)(a[1].CastToNumber() ?? 0d), (float)(a[2].CastToNumber() ?? 0d)));
            return DynValue.Nil;
        });
        Add("SetWeather", a => { _ctx?.SetWeather(a[0].CastToBool()); return DynValue.Nil; });
        return game;
    }
}
