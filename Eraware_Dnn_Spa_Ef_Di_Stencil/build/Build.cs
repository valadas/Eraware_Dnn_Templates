using System;
using System.IO;
using System.IO.Compression;
using System.Linq;
using BuildHelpers;
using Nuke.Common;
using Nuke.Common.Execution;
using Nuke.Common.Git;
using Nuke.Common.ProjectModel;
using Nuke.Common.Tooling;
using Nuke.Common.Tools.DotNet;
using Nuke.Common.Tools.GitVersion;
using Nuke.Common.Utilities.Collections;
using static Nuke.Common.EnvironmentInfo;
using static Nuke.Common.IO.FileSystemTasks;
using static Nuke.Common.IO.PathConstruction;
using static Nuke.Common.Tools.DotNet.DotNetTasks;

[CheckBuildProjectConfigurations]
[UnsetVisualStudioEnvironmentVariables]
class Build : NukeBuild
{
    /// Support plugins are available for:
    ///   - JetBrains ReSharper        https://nuke.build/resharper
    ///   - JetBrains Rider            https://nuke.build/rider
    ///   - Microsoft VisualStudio     https://nuke.build/visualstudio
    ///   - Microsoft VSCode           https://nuke.build/vscode

    public static int Main() => Execute<Build>(x => x.Deploy);

    [Parameter("Configuration to build - Default is 'Debug' (local) or 'Release' (server)")]
    readonly Configuration Configuration = IsLocalBuild ? Configuration.Debug : Configuration.Release;

    [Solution] readonly Solution Solution;
    private GitRepository GitRepository;
    private GitVersion GitVersion;

    AbsolutePath ArtifactsDirectory => RootDirectory / "artifacts";
    Target Clean => _ => _
        .Before(Restore)
        .Executes(() =>
        {
            EnsureCleanDirectory(ArtifactsDirectory);
        });

    Target SetupGit => _ => _
        .Executes(() =>
        {
            AbsolutePath git = RootDirectory / ".git";
            if (DirectoryExists(git))
            {
                GitRepository = GitRepository.FromLocalDirectory(RootDirectory);
            }
            else
            {
                Logger.Warn("For proper versioning support, publish this project to a git repository.");
            }
        });

    Target Restore => _ => _
        .Executes(() =>
        {
            DotNetRestore(_ => _
                .SetProjectFile(Solution));
        });

    Target CompileLibraries => _ => _
        .DependsOn(Restore)
        .DependsOn(SetupGit)
        .Executes(() =>
        {
            DotNetBuild(_ => _
                .SetProjectFile(Solution)
                .SetConfiguration(Configuration)
                .SetAssemblyVersion(GitVersion?.AssemblySemVer)
                .SetFileVersion(GitVersion?.AssemblySemFileVer)
                .SetInformationalVersion(GitVersion?.InformationalVersion)
                .EnableNoRestore());
        });

    Target Package => _ => _
        .DependsOn(Clean)
        .DependsOn(CompileLibraries)
        .Executes(() =>
        {
            var stagingDirectory = ArtifactsDirectory / "staging";
            EnsureCleanDirectory(stagingDirectory);

            // Resources
            ZipFile.CreateFromDirectory(RootDirectory / "resources", stagingDirectory / "resources.zip");

            // Symbols
            var symbolFiles = GlobFiles(RootDirectory, "bin/Release/**/*.pdb");
            Helpers.AddFilesToZip(stagingDirectory / "symbols.zip", symbolFiles.ToArray());

            // Install files
            var installFiles = GlobFiles(RootDirectory, "LICENSE", "manifest.dnn", "ReleaseNotes.html");
            installFiles.ForEach(i => CopyFileToDirectory(i, stagingDirectory));

            // Libraries
            CopyDirectoryRecursively(RootDirectory / "bin" / "Release", stagingDirectory / "bin", excludeFile: (f) => !f.Name.EndsWith("dll"));

            // Install package
            ZipFile.CreateFromDirectory(stagingDirectory, ArtifactsDirectory / "install.zip");
            DeleteDirectory(stagingDirectory);
        });

    Target Deploy => _ => _
    .DependsOn(CompileLibraries)
    .Executes(() =>
    {

    });

    Target Watch => _ => _
    .DependsOn(CompileLibraries)
    .Executes(() =>
    {

    });
}