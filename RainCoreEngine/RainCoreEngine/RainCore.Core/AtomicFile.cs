namespace RainCore;

             
                                                                       
                                                                               
                                                                  
   
                                                                               
                                                                       
                                                                           
                                                                        
                                                                     
                                                                       
                                                                        
                                                                           
                                                                       
                                                           
   
                                                                        
                                                                         
                                                                             
                                                                 
              
public static class AtomicFile
{
    public static void WriteAllText(string path, string contents)
    {
        var tempPath = path + ".tmp";
        File.WriteAllText(tempPath, contents);
        File.Move(tempPath, path, overwrite: true);
    }
}
