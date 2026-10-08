namespace RainCore;

             
                                                                          
                                                                            
                                                                         
                                                             
                                                                  
                                                                           
                                                                          
                                                      
   
                                                                       
                                                                          
                                                                           
                                                                          
                                        
              
public sealed record ActorData(
    string Id, string Name,
    int MaxHp, int MaxMp, int Attack, int Defense, int MagicAttack, int MagicDefense, int Agility, int Luck,
    List<string> SkillIds);

public enum SkillTargetKind { OneEnemy, AllEnemies, OneAlly, AllAllies, Self }

public sealed record SkillData(
    string Id, string Name, int MpCost, int Power, SkillTargetKind Target, string Description);

public sealed record ItemData(
    string Id, string Name, int Price, int HealHp, int HealMp, string Description);

public sealed record EnemyData(
    string Id, string Name,
    int MaxHp, int MaxMp, int Attack, int Defense, int MagicAttack, int MagicDefense, int Agility, int Luck,
    List<string> SkillIds, int ExpReward, int GoldReward);

public sealed record TroopData(string Id, string Name, List<string> EnemyIds);
