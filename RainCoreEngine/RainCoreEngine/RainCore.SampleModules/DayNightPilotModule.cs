namespace RainCore.SampleModules;

             
                                                                        
                                                                            
                                                                                
                                                                            
                                                                         
                                                       
   
                                                                           
                                          
                                                         
                                                                        
                                              
              
public sealed class DayNightPilotModule : IGameModule
{
    private const float HoursToRealSeconds = 30f;                                                                                          
    private const float HoursPerDay = 24f;

    private float _gameHour = 8f;                                              
    private int _lastLoggedHour = -1;
    private string _levelName = string.Empty;

    public void Initialize(ILevel level, string projectPath)
    {
        _levelName = level.Name;
        _lastLoggedHour = -1;
        Console.WriteLine($"[DayNightPilotModule] Загружен для уровня \"{_levelName}\" ({projectPath}). Время: {FormatHour(_gameHour)}.");
    }

    public void Update(float deltaTime)
    {
        _gameHour += deltaTime / HoursToRealSeconds;
        if (_gameHour >= HoursPerDay) _gameHour -= HoursPerDay;

        int wholeHour = (int)_gameHour;
        if (wholeHour != _lastLoggedHour)
        {
            _lastLoggedHour = wholeHour;
            Console.WriteLine($"[DayNightPilotModule] {_levelName}: {FormatHour(_gameHour)} ({DescribePhase(_gameHour)}).");
        }
    }

                                                                               
                                                                           
                                                                         
                                                                                 
                                                                 
    public void Render()
    {
    }

    private static string FormatHour(float hour) => $"{(int)hour:00}:{(int)((hour % 1f) * 60f):00}";

    private static string DescribePhase(float hour) => hour switch
    {
        >= 6f and < 10f => "утро",
        >= 10f and < 17f => "день",
        >= 17f and < 21f => "вечер",
        _ => "ночь",
    };
}
