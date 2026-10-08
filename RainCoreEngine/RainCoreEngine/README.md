# RainCoreEngine

RainCoreEngine is a modular game engine built with .NET 8. The solution contains
reusable engine modules, an editor, and a player runtime.

## Build

Install the .NET 8 SDK, then run:

```powershell
dotnet build RainCore.sln
```

The editor targets Windows. Other projects target .NET 8.

## Modules

- `RainCore.Core` — shared engine contracts and gameplay systems
- `RainCore.Graphics` and `RainCore.Scene` — rendering and scene systems
- `RainCore.Audio` — audio and video playback
- `RainCore.UI` — UI components
- `RainCore.Voxel` — voxel world systems
- `RainCore.Scripting` and `RainCore.TurnBased` — optional systems
- `RainCore.EditorApp` and `RainCore.Player` — editor and runtime applications

## Author

nonexist
