using System.Globalization;
using System.Linq;

namespace RainCore;

public sealed record EventCommandParameter(string Name);

public enum EventCommandParameterType
{
    Text,
    Boolean,
    Integer,
    Float,
}

public sealed record TypedEventCommandParameter(
    string Name,
    EventCommandParameterType Type,
    string DefaultValue = "");

             
                                                                                
                                                                                    
                                                                                   
                                                                           
                                                                              
                                                                                   
                                                                                 
                                                                                
                                                                 
              
public static class EventNameHistory
{
    private static readonly HashSet<string> _globalSwitches = new(StringComparer.Ordinal);
    private static readonly HashSet<string> _selfSwitches = new(StringComparer.Ordinal);
    private static readonly HashSet<string> _variables = new(StringComparer.Ordinal);

    public static IReadOnlyCollection<string> GlobalSwitches => _globalSwitches;
    public static IReadOnlyCollection<string> SelfSwitches => _selfSwitches;
    public static IReadOnlyCollection<string> Variables => _variables;

    public static void RegisterGlobalSwitch(string name) => Register(_globalSwitches, name);
    public static void RegisterSelfSwitch(string name) => Register(_selfSwitches, name);
    public static void RegisterVariable(string name) => Register(_variables, name);

    private static void Register(HashSet<string> set, string name)
    {
        if (!string.IsNullOrWhiteSpace(name)) set.Add(name.Trim());
    }

                 
                                                                                   
                                                                         
                                                                
                  
    public static IReadOnlyList<string> Suggest(IReadOnlyCollection<string> pool, string prefix, int maxCount = 6)
    {
        return pool
            .Where(name => string.IsNullOrEmpty(prefix) || name.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
            .OrderBy(name => name, StringComparer.OrdinalIgnoreCase)
            .Take(maxCount)
            .ToList();
    }
}

public sealed class EventCommandDescriptor
{
    public string Kind { get; }
    public string DisplayName { get; }
    public IReadOnlyList<EventCommandParameter> Parameters { get; }
    public IReadOnlyList<TypedEventCommandParameter> TypedParameters { get; }
    public string Category { get; }
    public Func<IReadOnlyList<string>, EventCommand> Build { get; }
    public Type CommandType { get; }
    public Func<EventCommand, SavedCommand> Encode { get; }
    public Func<EventCommand, string> Preview { get; }

                 
                                                                           
                                                                           
                                                                           
                                                      
                  
    public Func<SavedCommand, EventCommand>? BuildFromSaved { get; }

                 
                                                                       
                                                                             
                                                                         
                                                    
                  
    public bool IsVariadic { get; }

                                                                                                 
    public string VariadicParameterName { get; }

                                                                                                              
    public int MinVariadicCount { get; }

    public EventCommandDescriptor(
        string kind,
        string displayName,
        Type commandType,
        IReadOnlyList<EventCommandParameter> parameters,
        Func<IReadOnlyList<string>, EventCommand> build,
        Func<EventCommand, SavedCommand> encode,
        string category = "Игра",
        IReadOnlyList<TypedEventCommandParameter>? typedParameters = null,
        Func<EventCommand, string>? preview = null,
        bool isVariadic = false,
        string variadicParameterName = "значение",
        int minVariadicCount = 0,
        Func<SavedCommand, EventCommand>? buildFromSaved = null)
    {
        Kind = kind;
        DisplayName = displayName;
        CommandType = commandType;
        Parameters = parameters;
        TypedParameters = typedParameters ?? parameters.Select(parameter =>
        {
            var type = parameter.Name.Contains("true/false", StringComparison.OrdinalIgnoreCase)
                ? EventCommandParameterType.Boolean
                : parameter.Name.Contains("секунд", StringComparison.OrdinalIgnoreCase)
                    || parameter.Name is "X" or "Y" or "Z"
                    ? EventCommandParameterType.Float
                    : parameter.Name.Contains("количество", StringComparison.OrdinalIgnoreCase)
                        || parameter.Name.Contains("изменение", StringComparison.OrdinalIgnoreCase)
                        ? EventCommandParameterType.Integer
                        : EventCommandParameterType.Text;
            var defaultValue = type switch
            {
                EventCommandParameterType.Boolean => "false",
                EventCommandParameterType.Integer => "0",
                EventCommandParameterType.Float => "0",
                _ => "",
            };
            return new TypedEventCommandParameter(parameter.Name, type, defaultValue);
        }).ToArray();
        Category = category;
        Build = build;
        Encode = encode;
        Preview = preview ?? (_ => DisplayName);
        IsVariadic = isVariadic;
        VariadicParameterName = variadicParameterName;
        MinVariadicCount = minVariadicCount;
        BuildFromSaved = buildFromSaved;
    }
}

             
                                                                           
                                                                 
              
public static class EventCommandRegistry
{
    private static readonly IReadOnlyList<EventCommandDescriptor> Descriptors = BuildDescriptors();
    private static readonly IReadOnlyDictionary<string, EventCommandDescriptor> ByKind =
        Descriptors.ToDictionary(item => item.Kind, StringComparer.Ordinal);

    public static IReadOnlyList<EventCommandDescriptor> All => Descriptors;

    public static EventCommandDescriptor? FindByKind(string kind) =>
        ByKind.TryGetValue(kind, out var descriptor) ? descriptor : null;

    public static EventCommandDescriptor? FindFor(EventCommand command) =>
        Descriptors.FirstOrDefault(item => item.CommandType.IsInstanceOfType(command));

    public static string Preview(EventCommand command)
    {
        var descriptor = FindFor(command);
        if (descriptor == null) return command.GetType().Name;
        return command switch
        {
            ShowMessage item => $"{descriptor.DisplayName}: \"{item.Speaker}\" — \"{item.Text}\"",
            PlaySoundCommand item => $"{descriptor.DisplayName}: {item.Path}",
            WaitCommand item => $"{descriptor.DisplayName}: {item.Seconds:0.##} сек",
            SetFlagCommand item => $"{descriptor.DisplayName}: {item.Name} = {item.Value}",
            SetSelfSwitchCommand item => $"{descriptor.DisplayName}: {item.Name} = {item.Value}",
            AddVariableCommand item => $"{descriptor.DisplayName}: {item.Name} += {item.Delta}",
            SetVariableCommand item => $"{descriptor.DisplayName}: {item.Name} = {item.Value}",
            RandomVariableCommand item => $"{descriptor.DisplayName}: {item.Name} [{item.Min}..{item.Max}]",
            ShowChoicesCommand item => $"{descriptor.DisplayName}: {item.Prompt}",
            IfVariableCommand item => $"{descriptor.DisplayName}: {item.Name} {item.Comparison.Symbol()} {item.Expected}",
            CallCommonEventCommand item => $"{descriptor.DisplayName}: {item.Name}",
            CommentCommand item => $"{descriptor.DisplayName}: {item.Text}",
            LoopCommand item => $"{descriptor.DisplayName}: {item.Count} раз",
            MoveRouteCommand item => $"{descriptor.DisplayName}: {item.NpcName}",
            FadeScreenCommand item => $"{descriptor.DisplayName}: {item.FadeOut}",
            FlashScreenCommand item => $"{descriptor.DisplayName}: {item.Seconds:0.##} сек",
            ShakeScreenCommand item => $"{descriptor.DisplayName}: {item.Strength:0.##}",
            GiveItemCommand item => $"{descriptor.DisplayName}: {item.ItemName} x{item.Amount}",
            RemoveItemCommand item => $"{descriptor.DisplayName}: {item.ItemName} x{item.Amount}",
            ShowPictureCommand item => $"{descriptor.DisplayName}: #{item.Id} {item.FileName} ({item.X:0.##},{item.Y:0.##}) x{item.Scale:0.##} α{item.Opacity:0.##}",
            HidePictureCommand item => $"{descriptor.DisplayName}: #{item.Id}",
            StartBattleCommand item => $"{descriptor.DisplayName}: {item.TroopId}",
            StartQuestCommand item => $"{descriptor.DisplayName}: {item.QuestId}",
            AdvanceQuestCommand item => $"{descriptor.DisplayName}: {item.QuestId}",
            CompleteQuestCommand item => $"{descriptor.DisplayName}: {item.QuestId}",
            FailQuestCommand item => $"{descriptor.DisplayName}: {item.QuestId}",
            TeleportCommand item => $"{descriptor.DisplayName}: ({item.Position.X:0.##}, {item.Position.Y:0.##}, {item.Position.Z:0.##})",
            SetWeatherCommand item => $"{descriptor.DisplayName}: {item.Active}",
            RunScriptCommand item => $"{descriptor.DisplayName}: {item.ScriptFile} -> {item.FunctionName}({string.Join(", ", item.Args)})",
            _ => descriptor.DisplayName,
        };
    }

    public static SavedCommand Encode(EventCommand command)
    {
        var descriptor = FindFor(command)
            ?? throw new NotSupportedException($"Команда {command.GetType().Name} не зарегистрирована.");
        return descriptor.Encode(command);
    }

    public static EventCommand Decode(SavedCommand data)
    {
        var descriptor = FindByKind(data.Kind)
            ?? throw new NotSupportedException($"Неизвестный тип команды: {data.Kind}");
        var command = descriptor.BuildFromSaved != null ? descriptor.BuildFromSaved(data) : descriptor.Build(data.Args);
        RegisterNameHistory(command);
        return command;
    }

                 
                                                                                    
                                                                                
                                                                               
                                                                           
                  
    public static void RegisterNameHistory(EventCommand command)
    {
        switch (command)
        {
            case SetFlagCommand item: EventNameHistory.RegisterGlobalSwitch(item.Name); break;
            case SetSelfSwitchCommand item: EventNameHistory.RegisterSelfSwitch(item.Name); break;
            case AddVariableCommand item: EventNameHistory.RegisterVariable(item.Name); break;
            case SetVariableCommand item: EventNameHistory.RegisterVariable(item.Name); break;
            case RandomVariableCommand item: EventNameHistory.RegisterVariable(item.Name); break;
        }
    }

    private static IReadOnlyList<EventCommandDescriptor> BuildDescriptors()
    {
        static EventCommandParameter Parameter(string name) => new(name);
        static float Float(string value) => float.TryParse(value, NumberStyles.Float, CultureInfo.InvariantCulture, out var result) ? result : 0f;
        static int Int(string value) => int.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, out var result) ? result : 0;
        static bool Bool(string value) => value.Equals("true", StringComparison.OrdinalIgnoreCase)
            || value.Equals("1", StringComparison.OrdinalIgnoreCase)
            || value.Equals("да", StringComparison.OrdinalIgnoreCase);
        static string FloatText(float value) => value.ToString(CultureInfo.InvariantCulture);
        static string IntText(int value) => value.ToString(CultureInfo.InvariantCulture);

        return new List<EventCommandDescriptor>
        {
            new("message", "Показать сообщение", typeof(ShowMessage),
                new[] { Parameter("говорящий"), Parameter("текст"), Parameter("секунды показа") },
                p => new ShowMessage(p[0], p[1], Float(p[2])),
                command => { var item = (ShowMessage)command; return new("message", new() { item.Speaker, item.Text, FloatText(item.HoldSeconds) }); }),
            new("note", "Показать записку", typeof(ShowNoteCommand),
                new[] { Parameter("заголовок"), Parameter("текст") },
                p => new ShowNoteCommand(p[0], p[1]),
                command => { var item = (ShowNoteCommand)command; return new("note", new() { item.Title, item.Text }); },
                category: "Сообщения"),
            new("sound", "Проиграть звук", typeof(PlaySoundCommand),
                new[] { Parameter("файл звука") },
                p => new PlaySoundCommand(p[0]),
                command => new("sound", new() { ((PlaySoundCommand)command).Path })),
            new("wait", "Ожидание", typeof(WaitCommand),
                new[] { Parameter("секунды") },
                p => new WaitCommand(Float(p[0])),
                command => new("wait", new() { FloatText(((WaitCommand)command).Seconds) })),
                                                                                    
                                                                                        
                                                                                      
                                                                                       
                                                                                   
                                           
            new("flag", "Установить switch", typeof(SetFlagCommand),
                new[] { Parameter("имя"), Parameter("true/false") },
                p => new SetFlagCommand(p[0], Bool(p[1])),
                command => { var item = (SetFlagCommand)command; return new("flag", new() { item.Name, item.Value.ToString() }); },
                category: "Состояние"),
            new("self", "Установить self-switch", typeof(SetSelfSwitchCommand),
                new[] { Parameter("имя A/B"), Parameter("true/false") },
                p => new SetSelfSwitchCommand(p[0], Bool(p[1])),
                command => { var item = (SetSelfSwitchCommand)command; return new("self", new() { item.Name, item.Value.ToString() }); },
                category: "Состояние"),
            new("variable", "Изменить переменную", typeof(AddVariableCommand),
                new[] { Parameter("имя"), Parameter("изменение") },
                p => new AddVariableCommand(p[0], Int(p[1])),
                command => { var item = (AddVariableCommand)command; return new("variable", new() { item.Name, IntText(item.Delta) }); },
                category: "Состояние"),
            new("set_variable", "Задать переменную", typeof(SetVariableCommand),
                new[] { Parameter("имя"), Parameter("значение") },
                p => new SetVariableCommand(p[0], Int(p[1])),
                command => { var item = (SetVariableCommand)command; return new("set_variable", new() { item.Name, IntText(item.Value) }); },
                category: "Состояние"),
            new("random_variable", "Случайное значение", typeof(RandomVariableCommand),
                new[] { Parameter("имя"), Parameter("минимум"), Parameter("максимум") },
                p => new RandomVariableCommand(p[0], Int(p[1]), Int(p[2])),
                command => { var item = (RandomVariableCommand)command; return new("random_variable", new() { item.Name, IntText(item.Min), IntText(item.Max) }); },
                category: "Состояние"),
            new("choices", "Выбор вариантов", typeof(ShowChoicesCommand),
                new[] { Parameter("вопрос") },
                p => new ShowChoicesCommand(p[0], p.Skip(1).ToArray()),
                command => { var item = (ShowChoicesCommand)command; return new("choices", item.Choices.Prepend(item.Prompt).ToList()); },
                category: "Сообщения",
                isVariadic: true,
                variadicParameterName: "вариант ответа"),
            new("if_variable", "Если переменная", typeof(IfVariableCommand),
                new[] { Parameter("имя переменной"), Parameter("оператор ==/!=/>/</>=/<="), Parameter("значение"), Parameter("текст если да"), Parameter("текст если нет") },
                p => new IfVariableCommand(p[0], ParseComparison(p[1]), Int(p[2]),
                    new[] { new ShowMessage("", p[3]) }, new[] { new ShowMessage("", p[4]) }),
                command =>
                {
                                                                                       
                                                                                        
                                                                                        
                    var item = (IfVariableCommand)command;
                    return new SavedCommand(
                        "if_variable",
                        new() { item.Name, item.Comparison.Symbol(), IntText(item.Expected) },
                        ThenCommands: item.ThenCommands.Select(EventCommandRegistry.Encode).ToList(),
                        ElseCommands: item.ElseCommands.Select(EventCommandRegistry.Encode).ToList());
                },
                category: "Поток управления",
                buildFromSaved: data =>
                {
                                                                                   
                                                                                      
                                                                                          
                    var thenCommands = data.ThenCommands is { Count: > 0 }
                        ? data.ThenCommands.Select(EventCommandRegistry.Decode)
                        : new EventCommand[] { new ShowMessage("", data.Args.Count > 3 ? data.Args[3] : "") };
                    var elseCommands = data.ElseCommands is { Count: > 0 }
                        ? data.ElseCommands.Select(EventCommandRegistry.Decode)
                        : new EventCommand[] { new ShowMessage("", data.Args.Count > 4 ? data.Args[4] : "") };
                    return new IfVariableCommand(data.Args[0], ParseComparison(data.Args[1]), Int(data.Args[2]), thenCommands, elseCommands);
                }),
            new("common_event", "Вызвать общее событие", typeof(CallCommonEventCommand),
                new[] { Parameter("имя общего события") },
                p => new CallCommonEventCommand(p[0]),
                command => new("common_event", new() { ((CallCommonEventCommand)command).Name }),
                category: "Поток управления"),
            new("comment", "Комментарий", typeof(CommentCommand),
                new[] { Parameter("текст") },
                p => new CommentCommand(p[0]),
                command => new("comment", new() { ((CommentCommand)command).Text }),
                category: "Поток управления"),
            new("loop", "Цикл сообщений", typeof(LoopCommand),
                new[] { Parameter("число повторов"), Parameter("текст") },
                p => new LoopCommand(Int(p[0]), new[] { new ShowMessage("", p[1]) }),
                command =>
                {
                                                                                   
                                                                                  
                                         
                    var item = (LoopCommand)command;
                    return new SavedCommand(
                        "loop",
                        new() { item.Count.ToString() },
                        Body: item.Commands.Select(EventCommandRegistry.Encode).ToList());
                },
                category: "Поток управления",
                buildFromSaved: data =>
                {
                                                                                       
                                                                                       
                    var body = data.Body is { Count: > 0 }
                        ? data.Body.Select(EventCommandRegistry.Decode)
                        : new EventCommand[] { new ShowMessage("", data.Args.Count > 1 ? data.Args[1] : "") };
                    return new LoopCommand(Int(data.Args[0]), body);
                }),
            new("move_route", "Move Route NPC", typeof(MoveRouteCommand),
                new[] { Parameter("имя NPC"), Parameter("смещение X"), Parameter("смещение Y"), Parameter("смещение Z") },
                p => new MoveRouteCommand(p[0], new OpenTK.Mathematics.Vector3(Float(p[1]), Float(p[2]), Float(p[3]))),
                command => { var item = (MoveRouteCommand)command; return new("move_route", new() { item.NpcName, FloatText(item.Offset.X), FloatText(item.Offset.Y), FloatText(item.Offset.Z) }); },
                category: "Движение"),
            new("fade", "Fade Screen", typeof(FadeScreenCommand),
                new[] { Parameter("true = fade out/false = fade in"), Parameter("секунды") },
                p => new FadeScreenCommand(Bool(p[0]), Float(p[1])),
                command => { var item = (FadeScreenCommand)command; return new("fade", new() { item.FadeOut.ToString(), FloatText(item.Seconds) }); },
                category: "Экран"),
            new("flash", "Flash Screen", typeof(FlashScreenCommand),
                new[] { Parameter("секунды") },
                p => new FlashScreenCommand(Float(p[0])),
                command => new("flash", new() { FloatText(((FlashScreenCommand)command).Seconds) }),
                category: "Экран"),
            new("shake", "Shake Screen", typeof(ShakeScreenCommand),
                new[] { Parameter("секунды"), Parameter("сила") },
                p => new ShakeScreenCommand(Float(p[0]), Float(p[1])),
                command => { var item = (ShakeScreenCommand)command; return new("shake", new() { FloatText(item.Seconds), FloatText(item.Strength) }); },
                category: "Экран"),
            new("picture", "Show Picture", typeof(ShowPictureCommand),
                new[] { Parameter("файл PNG"), Parameter("id"), Parameter("X"), Parameter("Y"), Parameter("масштаб"), Parameter("прозрачность 0-1") },
                p => new ShowPictureCommand(p[0], Int(p[1]), Float(p[2]), Float(p[3]), Float(p[4]), Float(p[5])),
                command => { var item = (ShowPictureCommand)command; return new("picture", new() { item.FileName, IntText(item.Id), FloatText(item.X), FloatText(item.Y), FloatText(item.Scale), FloatText(item.Opacity) }); },
                category: "Экран",
                typedParameters: new[]
                {
                    new TypedEventCommandParameter("файл PNG", EventCommandParameterType.Text),
                    new TypedEventCommandParameter("id", EventCommandParameterType.Integer, "0"),
                    new TypedEventCommandParameter("X", EventCommandParameterType.Float, "0"),
                    new TypedEventCommandParameter("Y", EventCommandParameterType.Float, "0"),
                    new TypedEventCommandParameter("масштаб", EventCommandParameterType.Float, "1"),
                    new TypedEventCommandParameter("прозрачность 0-1", EventCommandParameterType.Float, "1"),
                }),
            new("hide_picture", "Hide Picture", typeof(HidePictureCommand),
                new[] { Parameter("id") },
                p => new HidePictureCommand(Int(p[0])),
                command => new("hide_picture", new() { IntText(((HidePictureCommand)command).Id) }),
                category: "Экран",
                typedParameters: new[] { new TypedEventCommandParameter("id", EventCommandParameterType.Integer, "0") }),
            new("start_battle", "Начать бой", typeof(StartBattleCommand),
                new[] { Parameter("ID отряда") },
                p => new StartBattleCommand(p[0]),
                command => new("start_battle", new() { ((StartBattleCommand)command).TroopId }),
                category: "Бой"),
            new("item", "Выдать предмет", typeof(GiveItemCommand),
                new[] { Parameter("имя предмета"), Parameter("количество") },
                p => new GiveItemCommand(p[0], Int(p[1])),
                command => { var item = (GiveItemCommand)command; return new("item", new() { item.ItemName, IntText(item.Amount) }); }),
            new("remove_item", "Забрать предмет", typeof(RemoveItemCommand),
                new[] { Parameter("имя предмета"), Parameter("количество") },
                p => new RemoveItemCommand(p[0], Int(p[1])),
                command => { var item = (RemoveItemCommand)command; return new("remove_item", new() { item.ItemName, IntText(item.Amount) }); }),
            new("start_quest", "Начать квест", typeof(StartQuestCommand),
                new[] { Parameter("id квеста") },
                p => new StartQuestCommand(p[0]),
                command => new("start_quest", new() { ((StartQuestCommand)command).QuestId }),
                category: "Квесты"),
            new("advance_quest", "Продвинуть квест", typeof(AdvanceQuestCommand),
                new[] { Parameter("id квеста") },
                p => new AdvanceQuestCommand(p[0]),
                command => new("advance_quest", new() { ((AdvanceQuestCommand)command).QuestId }),
                category: "Квесты"),
            new("complete_quest", "Завершить квест", typeof(CompleteQuestCommand),
                new[] { Parameter("id квеста") },
                p => new CompleteQuestCommand(p[0]),
                command => new("complete_quest", new() { ((CompleteQuestCommand)command).QuestId }),
                category: "Квесты"),
            new("fail_quest", "Провалить квест", typeof(FailQuestCommand),
                new[] { Parameter("id квеста") },
                p => new FailQuestCommand(p[0]),
                command => new("fail_quest", new() { ((FailQuestCommand)command).QuestId }),
                category: "Квесты"),
            new("teleport", "Телепорт", typeof(TeleportCommand),
                new[] { Parameter("X"), Parameter("Y"), Parameter("Z") },
                p => new TeleportCommand(Float(p[0]), Float(p[1]), Float(p[2])),
                command => { var item = (TeleportCommand)command; return new("teleport", new() { FloatText(item.Position.X), FloatText(item.Position.Y), FloatText(item.Position.Z) }); }),
            new("weather", "Погода", typeof(SetWeatherCommand),
                new[] { Parameter("true/false"), Parameter("секунды ожидания") },
                p => new SetWeatherCommand(Bool(p[0]), Float(p[1])),
                command => { var item = (SetWeatherCommand)command; return new("weather", new() { item.Active.ToString(), FloatText(item.WaitSeconds) }); }),
            new("run_script", "Выполнить скрипт (.lua)", typeof(RunScriptCommand),
                new[] { Parameter("файл скрипта (Scripts/...)"), Parameter("имя функции") },
                p => new RunScriptCommand(p[0], p[1], p.Skip(2).ToArray()),
                command =>
                {
                    var item = (RunScriptCommand)command;
                    return new("run_script", new List<string> { item.ScriptFile, item.FunctionName }.Concat(item.Args).ToList());
                },
                category: "Скрипты",
                isVariadic: true,
                variadicParameterName: "аргумент"),
        };

        static VariableComparison ParseComparison(string value) => value switch
        {
            "!=" => VariableComparison.NotEqual,
            ">" => VariableComparison.Greater,
            "<" => VariableComparison.Less,
            ">=" => VariableComparison.GreaterOrEqual,
            "<=" => VariableComparison.LessOrEqual,
            _ => VariableComparison.Equal,
        };
    }
}
