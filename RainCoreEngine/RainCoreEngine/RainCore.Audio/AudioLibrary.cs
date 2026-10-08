namespace RainCore;

             
                                                                        
                                                                                      
   
                                                                                 
                                                                             
                                                                               
                                                                                 
                                                                                  
                                                                             
                                                                               
                              
   
                                                                               
                                                                                
                                                                            
              
public static class AudioLibrary
{
                                                                                    
                                                                                      
                                                                                             
    public static string MusicDirFor(string sanitizedDreamId) =>
        Path.Combine(AppContext.BaseDirectory, "Audio", sanitizedDreamId);

                                                                                    
                                                                                    
                                                                              
    public static string AmbientDir() =>
        Path.Combine(AppContext.BaseDirectory, "Audio", "Ambient");

                                                                                        
                                                                                 
                                                 
    public static List<string> ScanFileNames(string dir)
    {
        if (!Directory.Exists(dir)) return new List<string>();

        return Directory.EnumerateFiles(dir)
            .Where(path => path.EndsWith(".wav", StringComparison.OrdinalIgnoreCase)
                        || path.EndsWith(".ogg", StringComparison.OrdinalIgnoreCase))
            .Select(Path.GetFileName)
            .Where(name => !string.IsNullOrEmpty(name))
            .Select(name => name!)
            .OrderBy(name => name, StringComparer.OrdinalIgnoreCase)
            .ToList();
    }

                                                                                      
                                                                                   
                                                                                   
                                                                               
                                                                                 
                                                          
    public static List<string> OrderedFileNames(string dir, IReadOnlyList<string> customOrder)
    {
        var scanned = ScanFileNames(dir);
        var byName = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        foreach (var name in scanned) byName[name] = name;

        var used = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        var ordered = new List<string>(scanned.Count);

        foreach (var name in customOrder)
            if (byName.TryGetValue(name, out var actual) && used.Add(actual))
                ordered.Add(actual);

        foreach (var name in scanned)
            if (used.Add(name))
                ordered.Add(name);

        return ordered;
    }
}
