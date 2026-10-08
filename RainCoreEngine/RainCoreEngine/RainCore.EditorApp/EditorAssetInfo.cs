using System.Text;
using System.Text.Json;

namespace RainCore.EditorApp;

             
                                                                                                                
                                                                                                           
                                                                                                        
              
public static class EditorAssetInfo
{
    public static List<string> Describe(string path)
    {
        var lines = new List<string>();
        try
        {
            var info = new FileInfo(path);
            if (!info.Exists) { lines.Add("File not found."); return lines; }

            lines.Add($"Size: {FormatSize(info.Length)}");
            switch (info.Extension.ToLowerInvariant())
            {
                case ".glb": DescribeGlb(path, lines); break;
                case ".png": DescribePng(path, lines); break;
                case ".wav": DescribeWav(path, lines); break;
                case ".lua": lines.Add($"Lines: {File.ReadLines(path).Count()}"); break;
                case ".json" when path.EndsWith(".roomscene.json", StringComparison.OrdinalIgnoreCase):
                    lines.Add("Scene file"); break;
            }
        }
        catch (Exception ex)
        {
            lines.Add($"Could not read details: {ex.Message}");
        }
        return lines;
    }

    public static string FormatSize(long bytes) =>
        bytes < 1024 ? $"{bytes} B" : bytes < 1024 * 1024 ? $"{bytes / 1024.0:0.#} KB" : $"{bytes / 1048576.0:0.##} MB";

    private static void DescribeGlb(string path, List<string> lines)
    {
        using var fs = File.OpenRead(path);
        using var br = new BinaryReader(fs);
        if (fs.Length < 20 || br.ReadUInt32() != 0x46546C67) { lines.Add("Not a valid GLB."); return; }          
        br.ReadUInt32();           
        br.ReadUInt32();                
        uint chunkLength = br.ReadUInt32();
        uint chunkType = br.ReadUInt32();
        if (chunkType != 0x4E4F534A || chunkLength > 64 * 1024 * 1024) return;          

        var json = Encoding.UTF8.GetString(br.ReadBytes((int)chunkLength));
        using var doc = JsonDocument.Parse(json);
        int Count(string name) => doc.RootElement.TryGetProperty(name, out var e) && e.ValueKind == JsonValueKind.Array ? e.GetArrayLength() : 0;

        lines.Add($"Meshes: {Count("meshes")}");
        lines.Add($"Materials: {Count("materials")}");
        lines.Add($"Textures: {Count("textures")}");
        lines.Add($"Animations: {Count("animations")}");
        lines.Add($"Nodes: {Count("nodes")}");
    }

    private static void DescribePng(string path, List<string> lines)
    {
        using var fs = File.OpenRead(path);
        var header = new byte[26];
        if (fs.Read(header, 0, header.Length) < 26) return;
        int Be(int o) => (header[o] << 24) | (header[o + 1] << 16) | (header[o + 2] << 8) | header[o + 3];
        lines.Add($"Resolution: {Be(16)} x {Be(20)}");
        int colorType = header[25];
        lines.Add($"Alpha: {(colorType == 4 || colorType == 6 ? "yes" : "no")}");
    }

    private static void DescribeWav(string path, List<string> lines)
    {
        using var fs = File.OpenRead(path);
        using var br = new BinaryReader(fs);
        if (fs.Length < 44 || br.ReadUInt32() != 0x46464952) return;          
        br.ReadUInt32();
        if (br.ReadUInt32() != 0x45564157) return;          

        int channels = 0, sampleRate = 0, byteRate = 0;
        long dataLength = 0;
        while (fs.Position + 8 <= fs.Length)
        {
            uint id = br.ReadUInt32();
            uint size = br.ReadUInt32();
            if (id == 0x20746D66)          
            {
                br.ReadUInt16();
                channels = br.ReadUInt16();
                sampleRate = br.ReadInt32();
                byteRate = br.ReadInt32();
                fs.Position += size - 12;
            }
            else if (id == 0x61746164)          
            {
                dataLength = size;
                break;
            }
            else fs.Position += size;
        }

        if (sampleRate > 0)
        {
            lines.Add($"Sample rate: {sampleRate} Hz");
            lines.Add($"Channels: {channels}");
            if (byteRate > 0) lines.Add($"Duration: {dataLength / (double)byteRate:0.00} s");
        }
    }
}
