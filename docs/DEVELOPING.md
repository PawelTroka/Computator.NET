# One solution for the whole repository

Open **Computator.NET.slnx** in a current Rider or Visual Studio supporting the pinned .NET SDK. This is the only checked-in solution; all supported C# projects are included, including installer authoring, CLI, the portable engine/tests and native interoperability probe. The build/CI files and documentation are linked in solution folders for normal editing and search.

On Windows, Build Solution builds every supported project. Running the installer authoring executable is not the same as building it: package compilation, installation, signing and release actions still require their explicit Cake targets/authorization. The native probe likewise needs the candidate-library arguments described by its validation target.

```powershell
./build.ps1 -Target Solution-Validate -Configuration Debug
./build.ps1 -Target Solution-Validate -Configuration Release
# Ordinary explicit CLI commands use the same file:
dotnet build Computator.NET.slnx
```

Choose Computator.NET, Computator.NET.WebApi, Computator.NET.WebClient, or Computator.NET.Cli as the startup project/run configuration. All are visible without switching solutions. IDE launch profiles remain project-owned. The WebClient startup correction is reviewed separately from solution organization.

Mac/Linux users can open this same solution to browse the complete codebase. Windows Forms/WPF and the Framework-bound installer authoring host still require Windows to build/run. `./build.sh --target=Portable-Validate` builds/tests the portable project graph directly; `Web-Validate` validates the independent web host. Neither creates a second solution or pretends Windows projects are cross-platform.

The old XP launcher project and VS2017 Coded UI records are linked under **Historical sources (not build targets)**. Their source remains accessible, but incompatible retired projects do not break every normal IDE restore. The XP launcher source is also present in the supported desktop project. Historical documents mentioning the former `.sln`/`.Modern.slnx`/`.Portable.slnx` names describe old checkpoints; use this guide and the single canonical solution for current development.

`build/Assert-Solution.ps1` rejects a second checked-in solution, missing projects, duplicate projects and broken linked paths. Adding a supported project therefore requires adding it to the canonical solution. No per-project NuGet lockfiles or platform-specific duplicate solutions are introduced.
