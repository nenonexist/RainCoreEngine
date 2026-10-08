using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;

namespace RainCore.Player;

             
                                                                      
                                                                          
                                                                           
                                                                           
                                                                            
                                                                 
   
                                                                           
                                                                          
                                                                         
                                                                         
                                                                           
              
internal static class Program
{
    private static void Main(string[] args)
    {
        string projectPath = args.Length > 0
            ? args[0]
            : Path.Combine(AppContext.BaseDirectory, "Project");

        if (!ProjectDescriptor.Exists(projectPath))
        {
            Console.WriteLine($"[Player] \"{projectPath}\" - не папка проекта (нет project.json).");
            Console.WriteLine("[Player] Использование: RainCore.Player.exe <путь к папке проекта>");
            return;
        }

        var project = ProjectDescriptor.TryLoad(projectPath);
        if (project == null)
        {
            Console.WriteLine($"[Player] Не удалось прочитать project.json в \"{projectPath}\".");
            return;
        }

        var nativeWindowSettings = new NativeWindowSettings
        {
            ClientSize = new Vector2i(1600, 900),
            Title = project.Name,
            Flags = ContextFlags.ForwardCompatible,
            WindowBorder = WindowBorder.Resizable,
        };

        using var window = new PlayerWindow(GameWindowSettings.Default, nativeWindowSettings, projectPath, project);
        window.Run();
    }
}
