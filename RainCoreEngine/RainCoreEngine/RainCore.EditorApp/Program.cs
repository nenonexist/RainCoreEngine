namespace RainCore.EditorApp;

             
                                                                           
                                                                        
                                                                             
                                                              
                                                                            
                                                          
              
internal static class Program
{
                                                                       
                                                                           
                                                                            
                                                                          
                                                                          
                                                                             
                                                                     
                                    
    [STAThread]
    private static void Main(string[] args)
    {
        string? initialProjectPath = args.Length > 0 ? args[0] : null;

        using var window = new EditorWindow(initialProjectPath);
        window.Run();
    }
}
