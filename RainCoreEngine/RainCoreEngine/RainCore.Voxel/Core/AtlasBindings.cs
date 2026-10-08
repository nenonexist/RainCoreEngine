using System.Text.Json;

namespace RainCore;

             
                                                                     
                                                       
                                                                          
                                                                        
                                                                          
                  
                                                                      
                                                                           
                                                                            
                                                                      
                                                    
   
                                                                           
                                                                       
                                                                  
              
public static class AtlasBindings
{
    private static string _path = "";
    private static readonly Dictionary<string, (int Col, int Row)> ByName = new(StringComparer.OrdinalIgnoreCase);

                 
                                                                         
                                                                           
                                                                    
                                                                         
                  
    public static void Load(string jsonPath)
    {
        _path = jsonPath;
        ByName.Clear();
        if (!File.Exists(jsonPath))
        {
            Console.WriteLine($"[Атлас] Файл привязок не найден ({jsonPath}) - используются только встроенные/кастомные привязки по умолчанию.");
            return;
        }

        try
        {
            var json = File.ReadAllText(jsonPath);
            var entries = JsonSerializer.Deserialize<List<BindingJson>>(json, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });
            if (entries == null) return;

            foreach (var e in entries)
            {
                if (string.IsNullOrWhiteSpace(e.Name)) continue;
                ByName[e.Name] = (e.Col, e.Row);
            }
            Console.WriteLine($"[Атлас] Привязок текстур загружено: {ByName.Count} (из {jsonPath}).");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[Атлас] Не удалось разобрать {jsonPath}: {ex.Message} - привязки из этого файла проигнорированы.");
            ByName.Clear();
        }
    }

    public static bool TryGet(string name, out (int Col, int Row) cell) => ByName.TryGetValue(name, out cell);

                                                                                                                                       
    public static bool TryGetOccupant(int col, int row, out string name)
    {
        foreach (var (n, cell) in ByName)
        {
            if (cell.Col == col && cell.Row == row)
            {
                name = n;
                return true;
            }
        }
        name = "";
        return false;
    }

                 
                                                                                  
                                                                        
                  
    public static void SetAndSave(string name, int col, int row)
    {
        ByName[name] = (col, row);
        Save();
    }

    private static void Save()
    {
        if (string.IsNullOrEmpty(_path))
        {
            Console.WriteLine("[Атлас] Не удалось сохранить привязку - AtlasBindings.Load ещё не вызывался (нет пути к файлу).");
            return;
        }

        try
        {
            var list = ByName
                .Select(kv => new BindingJson { Name = kv.Key, Col = kv.Value.Col, Row = kv.Value.Row })
                .OrderBy(b => b.Name)
                .ToList();
            var json = JsonSerializer.Serialize(list, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(_path, json);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[Атлас] Не удалось сохранить {_path}: {ex.Message}");
        }
    }

    private sealed class BindingJson
    {
        public string Name { get; set; } = "";
        public int Col { get; set; }
        public int Row { get; set; }
    }
}
