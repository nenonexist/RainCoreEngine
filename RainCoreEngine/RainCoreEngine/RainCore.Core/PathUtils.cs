namespace RainCore;

                                                                                      
                                                                                       
                                                                  
public static class PathUtils
{
                                                                                           
    public static string SanitizeForPath(string name)
    {
        var invalid = Path.GetInvalidFileNameChars();
        var chars = name.Select(c => invalid.Contains(c) ? '_' : c).ToArray();
        return new string(chars);
    }
}
