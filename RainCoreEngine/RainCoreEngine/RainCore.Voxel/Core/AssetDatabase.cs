namespace RainCore;

             
                                                                             
                                                                             
                                                                               
                                                                                 
   
                                                                              
                                                                              
                                                                                
                                                                               
                                                                                
                                                                     
              
public class AssetDatabase
{
                                                                                    
                                                                                       
                                                                                                 
    public static AssetDatabase? Instance { get; set; }

    private readonly List<AssetEntry> _all = new();
    private readonly Dictionary<(AssetType Type, string Id), AssetEntry> _byTypeAndId =
        new(new TypeIdComparer());

    public IReadOnlyList<AssetEntry> All => _all;

                                                                                     
                                                                                       
                                                                                          
                                                                                         
                                                     
    private static readonly Dictionary<AssetType, string> RootFolderNames = new()
    {
        [AssetType.Image] = "Images",
        [AssetType.Model] = "Models",
        [AssetType.Texture] = "Textures",
        [AssetType.Sound] = "Sounds",
        [AssetType.Music] = "Audio",
        [AssetType.Video] = "Videos",
        [AssetType.Scene] = "Scenes",
    };

                                                                                        
                                                                                          
                                                                                         
                                                                                       
                                                      
    private static readonly Dictionary<AssetType, string[]?> AllowedExtensions = new()
    {
        [AssetType.Image] = null,
        [AssetType.Model] = null,
        [AssetType.Texture] = new[] { ".png" },
        [AssetType.Sound] = null,
        [AssetType.Music] = null,
        [AssetType.Video] = null,
        [AssetType.Scene] = new[] { ".json" },
    };

    public static string RootFolderFor(AssetType type) => RootFolderNames[type];

                                                                                               
                                                                                           
                                                                                              
    public static AssetDatabase Build(string contentRoot)
    {
        var db = new AssetDatabase();
        foreach (var type in RootFolderNames.Keys)
            db.ScanType(contentRoot, type);
        return db;
    }

    private void ScanType(string contentRoot, AssetType type)
    {
        var rootFolder = RootFolderNames[type];
        var rootPath = Path.Combine(contentRoot, rootFolder);
        if (!Directory.Exists(rootPath)) return;

        var allowedExt = AllowedExtensions[type];
        foreach (var file in Directory.GetFiles(rootPath, "*", SearchOption.AllDirectories))
        {
            if (allowedExt != null && !allowedExt.Contains(Path.GetExtension(file), StringComparer.OrdinalIgnoreCase))
                continue;

            var relative = Path.GetRelativePath(rootPath, file).Replace('\\', '/');
            var id = relative[..^Path.GetExtension(relative).Length];                                     

            var entry = new AssetEntry(type, id, relative, file);
            var key = (type, id);
            if (_byTypeAndId.ContainsKey(key))
            {
                Console.WriteLine($"[Ассеты] Пропущен дубликат Id \"{id}\" ({type}) в {file} - Id уже занят другим файлом того же типа.");
                continue;
            }

            _byTypeAndId[key] = entry;
            _all.Add(entry);
        }
    }

    public bool TryResolve(AssetType type, string id, out AssetEntry entry) =>
        _byTypeAndId.TryGetValue((type, id), out entry!);

    public IEnumerable<AssetEntry> OfType(AssetType type) => _all.Where(e => e.Type == type);

                                                                                    
                                                                              
    public string Summary() => string.Join(", ", RootFolderNames.Keys.Select(t => $"{t}: {OfType(t).Count()}"));

    private sealed class TypeIdComparer : IEqualityComparer<(AssetType Type, string Id)>
    {
        public bool Equals((AssetType Type, string Id) x, (AssetType Type, string Id) y) =>
            x.Type == y.Type && string.Equals(x.Id, y.Id, StringComparison.OrdinalIgnoreCase);

        public int GetHashCode((AssetType Type, string Id) obj) =>
            HashCode.Combine(obj.Type, obj.Id.ToLowerInvariant());
    }
}
