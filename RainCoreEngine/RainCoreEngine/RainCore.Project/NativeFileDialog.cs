using System.Runtime.InteropServices;

namespace RainCore;

             
                                                                                   
                                                                              
                                                                        
                                                                             
                                                                          
                                                                            
                                                                               
                                                                             
              
public static class NativeFileDialog
{
    private const int OFN_FILEMUSTEXIST = 0x00001000;
    private const int OFN_PATHMUSTEXIST = 0x00000800;
    private const int OFN_HIDEREADONLY = 0x00000004;
    private const int OFN_EXPLORER = 0x00080000;
    private const int OFN_NOCHANGEDIR = 0x00000008;

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
    private struct OPENFILENAME
    {
        public int lStructSize;
        public IntPtr hwndOwner;
        public IntPtr hInstance;
        public string? lpstrFilter;
        public string? lpstrCustomFilter;
        public int nMaxCustFilter;
        public int nFilterIndex;
        public IntPtr lpstrFile;
        public int nMaxFile;
        public IntPtr lpstrFileTitle;
        public int nMaxFileTitle;
        public string? lpstrInitialDir;
        public string? lpstrTitle;
        public int Flags;
        public short nFileOffset;
        public short nFileExtension;
        public string? lpstrDefExt;
        public IntPtr lCustData;
        public IntPtr lpfnHook;
        public string? lpTemplateName;
        public IntPtr pvReserved;
        public int dwReserved;
        public int FlagsEx;
    }

    [DllImport("comdlg32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
    private static extern bool GetOpenFileNameW(ref OPENFILENAME ofn);

                 
                                                                            
                                                                               
                                                                                
                              
                  
    public static bool TryOpenFile(string filterPattern, out string path, string title = "Открыть файл")
    {
        path = "";
        const int maxPath = 1024;
        var fileBuffer = Marshal.AllocHGlobal(maxPath * sizeof(char));
        try
        {
            Marshal.WriteInt16(fileBuffer, 0, 0);

                                                                                             
            var filter = $"Файлы {filterPattern}\0{filterPattern}\0Все файлы (*.*)\0*.*\0\0";

            var ofn = new OPENFILENAME
            {
                lpstrFilter = filter,
                lpstrFile = fileBuffer,
                nMaxFile = maxPath,
                lpstrTitle = title,
                Flags = OFN_FILEMUSTEXIST | OFN_PATHMUSTEXIST | OFN_HIDEREADONLY | OFN_EXPLORER | OFN_NOCHANGEDIR,
            };
            ofn.lStructSize = Marshal.SizeOf(ofn);

            if (!GetOpenFileNameW(ref ofn))
                return false;                                                

            path = Marshal.PtrToStringUni(fileBuffer) ?? "";
            return !string.IsNullOrEmpty(path);
        }
        finally
        {
            Marshal.FreeHGlobal(fileBuffer);
        }
    }

                 
                                                                                
                                                                              
                                                                                   
                               
                  
    public static string CopyIntoProjectFolder(string sourcePath, string targetDir)
    {
        Directory.CreateDirectory(targetDir);
        var fileName = Path.GetFileName(sourcePath);
        var fullSource = Path.GetFullPath(sourcePath);
        var destPath = Path.Combine(targetDir, fileName);
        var fullDest = Path.GetFullPath(destPath);

        if (string.Equals(fullSource, fullDest, StringComparison.OrdinalIgnoreCase))
            return fileName;                                 

        if (File.Exists(destPath) && !SameSize(fullSource, fullDest))
        {
            var name = Path.GetFileNameWithoutExtension(fileName);
            var ext = Path.GetExtension(fileName);
            int i = 1;
            do
            {
                fileName = $"{name}_{i}{ext}";
                destPath = Path.Combine(targetDir, fileName);
                i++;
            } while (File.Exists(destPath));
        }

        if (!File.Exists(destPath))
            File.Copy(fullSource, destPath, overwrite: false);

        return fileName;
    }

                                                                              
                                                                     
    private static bool SameSize(string a, string b)
    {
        try { return new FileInfo(a).Length == new FileInfo(b).Length; }
        catch { return false; }
    }
}
