using System.Text.Json;

namespace RainCore;

                                                                                              
public static class Inventory
{
    private static readonly Dictionary<string, int> Items = new(StringComparer.OrdinalIgnoreCase);
    private static string SaveFilePath => Path.Combine(AppContext.BaseDirectory, "Saves", "inventory.json");

    public static IReadOnlyDictionary<string, int> All => Items;

    public static int Get(string name) => Items.TryGetValue(name, out var amount) ? amount : 0;

    public static void Add(string name, int amount)
    {
        if (string.IsNullOrWhiteSpace(name) || amount == 0) return;
        Items[name] = Math.Max(0, Get(name) + amount);
        if (Items[name] == 0) Items.Remove(name);
    }

    public static bool Remove(string name, int amount = 1)
    {
        if (amount <= 0 || Get(name) < amount) return false;
        Add(name, -amount);
        return true;
    }

    public static void Load()
    {
        if (!File.Exists(SaveFilePath)) return;
        try
        {
            var data = JsonSerializer.Deserialize<Dictionary<string, int>>(File.ReadAllText(SaveFilePath));
            Items.Clear();
            if (data == null) return;
            foreach (var item in data)
                if (item.Value > 0) Items[item.Key] = item.Value;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[Инвентарь] Не удалось загрузить: {ex.Message}");
        }
    }

    public static void Save()
    {
        try
        {
            Directory.CreateDirectory(Path.GetDirectoryName(SaveFilePath)!);
            AtomicFile.WriteAllText(SaveFilePath, JsonSerializer.Serialize(Items));
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[Инвентарь] Не удалось сохранить: {ex.Message}");
        }
    }
}
