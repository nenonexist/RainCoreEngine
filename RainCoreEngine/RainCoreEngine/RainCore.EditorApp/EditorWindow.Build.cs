using System.Diagnostics;
using ImGuiNET;

namespace RainCore.EditorApp;

             
                                                                          
                                                                            
                                                                      
                                                                         
                                                                          
                                                                             
                                                                            
                                                                               
                                                                        
                                
   
                                                                        
                                                                             
                                                                           
                                                                          
                                                                          
              
public sealed partial class EditorWindow
{
    private static readonly (string Label, string Rid)[] BuildTargets =
    {
        ("Windows x64", "win-x64"),
        ("Linux x64", "linux-x64"),
        ("macOS x64 (Intel)", "osx-x64"),
        ("macOS ARM64 (Apple Silicon)", "osx-arm64"),
    };

    private int _buildTargetIndex;
    private string _playerCsprojPathBuffer = string.Empty;
    private string _buildOutputFolderBuffer = string.Empty;
    private readonly List<string> _buildLog = new();
    private bool _buildInProgress;

                                                                          
                                                                           
                                                                                 
                                                                             
                                                                                
                                                                               
                                                                              
                                                 
    private static string? TryGuessPlayerCsprojPath()
    {
        var dir = new DirectoryInfo(AppContext.BaseDirectory);
        for (int i = 0; i < 6 && dir != null; i++, dir = dir.Parent)
        {
            var candidate = Path.Combine(dir.FullName, "RainCore.Player", "RainCore.Player.csproj");
            if (File.Exists(candidate)) return candidate;
        }
        return null;
    }

    private void DrawBuildWindow()
    {
        if (!_showBuildWindow) return;

        if (string.IsNullOrEmpty(_playerCsprojPathBuffer))
            _playerCsprojPathBuffer = _settings.PlayerCsprojPath.Length > 0
                ? _settings.PlayerCsprojPath
                : TryGuessPlayerCsprojPath() ?? string.Empty;

        if (string.IsNullOrEmpty(_buildOutputFolderBuffer) && !string.IsNullOrEmpty(_projectPath))
            _buildOutputFolderBuffer = Path.Combine(_projectPath, "Build", BuildTargets[_buildTargetIndex].Rid);

        ImGui.Begin("Build", ref _showBuildWindow);

        ImGui.TextWrapped("Publishes a standalone copy of this project - self-contained, " +
                           "no .NET SDK needed on the player's machine.");
        ImGui.Separator();

        ImGui.InputText("RainCore.Player.csproj", ref _playerCsprojPathBuffer, 512);
        ImGui.SameLine();
        if (ImGui.Button("Browse...##PlayerCsproj") && NativeFileDialog.TryOpenFile("*.csproj", out var picked, "Select RainCore.Player.csproj"))
            _playerCsprojPathBuffer = picked;
        if (!File.Exists(_playerCsprojPathBuffer))
            ImGui.TextColored(new System.Numerics.Vector4(1f, 0.5f, 0.4f, 1f), "File not found.");

        var targetLabels = BuildTargets.Select(t => t.Label).ToArray();
        if (ImGui.Combo("Target platform", ref _buildTargetIndex, targetLabels, targetLabels.Length))
            _buildOutputFolderBuffer = Path.Combine(_projectPath, "Build", BuildTargets[_buildTargetIndex].Rid);

        ImGui.InputText("Output folder", ref _buildOutputFolderBuffer, 512);
        ImGui.SameLine();
        if (ImGui.Button("Browse...##BuildOutput"))
        {
            using var dialog = new System.Windows.Forms.FolderBrowserDialog { SelectedPath = _buildOutputFolderBuffer };
            if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                _buildOutputFolderBuffer = dialog.SelectedPath;
        }

        ImGui.Separator();

        ImGui.BeginDisabled(_buildInProgress || !File.Exists(_playerCsprojPathBuffer) || string.IsNullOrEmpty(_projectPath));
        if (ImGui.Button("Build"))
        {
            _settings.PlayerCsprojPath = _playerCsprojPathBuffer;
            _settings.Save();
            BuildProject(runAfter: false);
        }
        ImGui.SameLine();
        if (ImGui.Button("Build and Run"))
        {
            _settings.PlayerCsprojPath = _playerCsprojPathBuffer;
            _settings.Save();
            BuildProject(runAfter: true);
        }
        ImGui.EndDisabled();

        if (_buildInProgress)
        {
            ImGui.SameLine();
            ImGui.TextDisabled("Building - this blocks the editor UI until dotnet publish finishes (see build log below).");
        }

        ImGui.Separator();
        ImGui.BeginChild("##BuildLog", System.Numerics.Vector2.Zero, ImGuiChildFlags.Border);
        foreach (var line in _buildLog) ImGui.TextWrapped(line);
        ImGui.EndChild();

        ImGui.End();
    }

                                                                            
                                                                           
                                                                      
                                                                              
                                                                              
                                                                                      
    private void BuildProject(bool runAfter)
    {
        _buildInProgress = true;
        _buildLog.Clear();
        AppendBuildLog($"Publishing {_playerCsprojPathBuffer} for {BuildTargets[_buildTargetIndex].Rid}...");

        try
        {
            Directory.CreateDirectory(_buildOutputFolderBuffer);

            var rid = BuildTargets[_buildTargetIndex].Rid;
            var psi = new ProcessStartInfo("dotnet")
            {
                Arguments = $"publish \"{_playerCsprojPathBuffer}\" -c Release -r {rid} " +
                            "--self-contained true -p:PublishSingleFile=true " +
                            $"-p:IncludeNativeLibrariesForSelfExtract=true -o \"{_buildOutputFolderBuffer}\"",
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true,
            };

            using var process = Process.Start(psi);
            if (process == null)
            {
                AppendBuildLog("Could not start dotnet - is the .NET SDK installed and on PATH?");
                return;
            }

            string stdout = process.StandardOutput.ReadToEnd();
            string stderr = process.StandardError.ReadToEnd();
            process.WaitForExit();

            if (stdout.Length > 0) AppendBuildLog(stdout);
            if (stderr.Length > 0) AppendBuildLog(stderr);

            if (process.ExitCode != 0)
            {
                AppendBuildLog($"dotnet publish failed (exit code {process.ExitCode}) - see output above.");
                return;
            }

            AppendBuildLog("Publish succeeded. Copying project content...");
            CopyProjectContentInto(Path.Combine(_buildOutputFolderBuffer, "Project"));
            AppendBuildLog($"Done. Output: {_buildOutputFolderBuffer}");

            if (runAfter)
            {
                var exePath = FindBuiltExecutable(_buildOutputFolderBuffer);
                if (exePath != null)
                {
                    AppendBuildLog($"Launching {exePath}...");
                    Process.Start(new ProcessStartInfo(exePath) { UseShellExecute = true, WorkingDirectory = _buildOutputFolderBuffer });
                }
                else
                {
                    AppendBuildLog("Build succeeded, but could not find the published executable to launch automatically.");
                }
            }
        }
        catch (Exception ex)
        {
            AppendBuildLog($"Build failed: {ex.Message}");
        }
        finally
        {
            _buildInProgress = false;
        }
    }

                                                                       
                                                                            
                                                                              
                                                                           
                                                                                 
    private void CopyProjectContentInto(string destination)
    {
        Directory.CreateDirectory(destination);

        foreach (var folder in ProjectFolders)
        {
            var source = Path.Combine(_projectPath, folder);
            if (Directory.Exists(source))
                CopyDirectoryRecursive(source, Path.Combine(destination, folder));
        }

        const string databaseFolder = "Database";
        var databaseSource = Path.Combine(_projectPath, databaseFolder);
        if (Directory.Exists(databaseSource))
            CopyDirectoryRecursive(databaseSource, Path.Combine(destination, databaseFolder));

        var projectJson = Path.Combine(_projectPath, "project.json");
        if (File.Exists(projectJson))
            File.Copy(projectJson, Path.Combine(destination, "project.json"), overwrite: true);

        if (_currentProjectDescriptor?.GameModuleAssembly is { Length: > 0 } moduleRelPath)
        {
            var moduleSource = Path.Combine(_projectPath, moduleRelPath);
            if (File.Exists(moduleSource))
            {
                var moduleDest = Path.Combine(destination, moduleRelPath);
                Directory.CreateDirectory(Path.GetDirectoryName(moduleDest)!);
                File.Copy(moduleSource, moduleDest, overwrite: true);
            }
        }
    }

    private static void CopyDirectoryRecursive(string sourceDir, string destDir)
    {
        Directory.CreateDirectory(destDir);
        foreach (var file in Directory.GetFiles(sourceDir))
            File.Copy(file, Path.Combine(destDir, Path.GetFileName(file)), overwrite: true);
        foreach (var subDir in Directory.GetDirectories(sourceDir))
            CopyDirectoryRecursive(subDir, Path.Combine(destDir, Path.GetFileName(subDir)));
    }

                                                                    
                                                                           
                                                                         
                                                                        
                                                                                
    private static string? FindBuiltExecutable(string outputFolder)
    {
        var withExe = Path.Combine(outputFolder, "RainCore.Player.exe");
        if (File.Exists(withExe)) return withExe;

        var withoutExtension = Path.Combine(outputFolder, "RainCore.Player");
        return File.Exists(withoutExtension) ? withoutExtension : null;
    }

    private void AppendBuildLog(string text)
    {
        _buildLog.Add(text);
        Console.WriteLine($"[Build] {text}");
    }
}
