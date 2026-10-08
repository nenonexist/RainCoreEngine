using ImGuiNET;
using System.Windows.Forms;

namespace RainCore.EditorApp;

             
                                                                             
                                                                                 
                                                                          
                                                                        
                                              
   
                                                                   
                                                                           
                                                                      
                                                                
              
public sealed partial class EditorWindow
{
    private string _launcherNewProjectName = "MyGame";
    private string _launcherNewProjectParentDir = string.Empty;
    private string _launcherOpenProjectPath = string.Empty;
    private string _launcherError = string.Empty;
    private List<string> _launcherRecentProjects = RecentProjectsStore.Load();

    private void DrawLauncher()
    {
        var viewport = ImGui.GetMainViewport();
        ImGui.SetNextWindowPos(viewport.WorkPos);
        ImGui.SetNextWindowSize(viewport.WorkSize);
        ImGui.SetNextWindowViewport(viewport.ID);

        var flags = ImGuiWindowFlags.NoDocking | ImGuiWindowFlags.NoTitleBar | ImGuiWindowFlags.NoCollapse
            | ImGuiWindowFlags.NoResize | ImGuiWindowFlags.NoMove | ImGuiWindowFlags.NoBringToFrontOnFocus
            | ImGuiWindowFlags.NoNavFocus;
        ImGui.Begin("LauncherHost", flags);

        ImGui.Dummy(new System.Numerics.Vector2(0, 16));
        ImGui.SetWindowFontScale(1.4f);
        ImGui.TextUnformatted("RainCore Editor");
        ImGui.SetWindowFontScale(1.0f);
        ImGui.Dummy(new System.Numerics.Vector2(0, 12));

        DrawRecentProjectsSection();
        ImGui.Dummy(new System.Numerics.Vector2(0, 16));
        ImGui.Separator();
        ImGui.Dummy(new System.Numerics.Vector2(0, 16));
        DrawNewProjectSection();
        ImGui.Dummy(new System.Numerics.Vector2(0, 16));
        DrawOpenProjectSection();

        if (!string.IsNullOrEmpty(_launcherError))
        {
            ImGui.Dummy(new System.Numerics.Vector2(0, 12));
            ImGui.TextColored(new System.Numerics.Vector4(1f, 0.4f, 0.4f, 1f), _launcherError);
        }

        ImGui.End();
    }

    private void DrawRecentProjectsSection()
    {
        ImGui.TextUnformatted("Recent Projects");
        if (_launcherRecentProjects.Count == 0)
        {
            ImGui.TextDisabled("(no recent projects yet)");
            return;
        }

        foreach (var path in _launcherRecentProjects)
        {
            ImGui.PushID(path);
            if (ImGui.Button("Open", new System.Numerics.Vector2(60, 0)))
            {
                _launcherError = string.Empty;
                OpenProject(path, isNew: false);
            }
            ImGui.SameLine();
            ImGui.TextUnformatted(path);
            ImGui.PopID();
        }
    }

    private void DrawNewProjectSection()
    {
                                                                            
                                                                              
                                             
        if (string.IsNullOrEmpty(_launcherNewProjectParentDir) && !string.IsNullOrEmpty(_settings.DefaultNewProjectParentFolder))
            _launcherNewProjectParentDir = _settings.DefaultNewProjectParentFolder;

        ImGui.TextUnformatted("New Project");

        ImGui.SetNextItemWidth(300);
        ImGui.InputText("Game name", ref _launcherNewProjectName, 128);

        ImGui.SetNextItemWidth(400);
        ImGui.InputText("Parent folder", ref _launcherNewProjectParentDir, 512);
        ImGui.SameLine();
        if (ImGui.Button("Browse...##new"))
        {
            using var dialog = new FolderBrowserDialog { Description = "Choose a folder to create the project in" };
            if (dialog.ShowDialog() == DialogResult.OK)
                _launcherNewProjectParentDir = dialog.SelectedPath;
        }

        if (ImGui.Button("Create Project"))
        {
            _launcherError = string.Empty;
            TryCreateNewProject();
        }
    }

    private void DrawOpenProjectSection()
    {
        ImGui.TextUnformatted("Open Project");

        ImGui.SetNextItemWidth(400);
        ImGui.InputText("Project folder", ref _launcherOpenProjectPath, 512);
        ImGui.SameLine();
        if (ImGui.Button("Browse...##open"))
        {
            using var dialog = new FolderBrowserDialog { Description = "Choose an existing project folder" };
            if (dialog.ShowDialog() == DialogResult.OK)
                _launcherOpenProjectPath = dialog.SelectedPath;
        }

        if (ImGui.Button("Open Project"))
        {
            _launcherError = string.Empty;
            TryOpenProject();
        }
    }

                                                                                 
                                                                             
                                                                             
                                                  
    private void TryCreateNewProject()
    {
        if (string.IsNullOrWhiteSpace(_launcherNewProjectName))
        {
            _launcherError = "Enter a game name.";
            return;
        }
        if (string.IsNullOrWhiteSpace(_launcherNewProjectParentDir) || !Directory.Exists(_launcherNewProjectParentDir))
        {
            _launcherError = "Choose an existing parent folder.";
            return;
        }

        var projectPath = Path.Combine(_launcherNewProjectParentDir, _launcherNewProjectName);
        try
        {
            ProjectDescriptor.Create(projectPath, _launcherNewProjectName);
        }
        catch (Exception ex)
        {
            _launcherError = $"Could not create project: {ex.Message}";
            return;
        }

        OpenProject(projectPath, isNew: true);
        _launcherRecentProjects = RecentProjectsStore.Load();
    }

    private void TryOpenProject()
    {
        if (string.IsNullOrWhiteSpace(_launcherOpenProjectPath) || !Directory.Exists(_launcherOpenProjectPath))
        {
            _launcherError = "Choose an existing folder.";
            return;
        }
        if (!ProjectDescriptor.Exists(_launcherOpenProjectPath))
        {
            _launcherError = "That folder has no project.json - not a RainCore project.";
            return;
        }

        OpenProject(_launcherOpenProjectPath, isNew: false);
        _launcherRecentProjects = RecentProjectsStore.Load();
    }
}
