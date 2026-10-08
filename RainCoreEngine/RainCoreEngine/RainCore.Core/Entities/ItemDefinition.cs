namespace RainCore;

             
                                                                                 
                                                                                
                                                                    
                                                                             
   
                                                                            
                                                                                   
                                                                                
                                                                             
                                                          
   
                                                                           
                                                                           
                                                         
                                                                                                   
              
public sealed class ItemDefinition
{
    public string Name { get; set; } = "";

                                                                                     
                                                                        
    public string IconPath { get; set; } = "";

    public ItemDefinition() { }

    public ItemDefinition(string name, string iconPath = "")
    {
        Name = name;
        IconPath = iconPath;
    }
}
