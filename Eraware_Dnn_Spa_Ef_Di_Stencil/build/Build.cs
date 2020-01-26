using System;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Xml;
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
                GitVersion = GitVersionTasks.GitVersion(s => s
                    .SetFramework("netcoreapp3.0")
                    .DisableLogOutput()
                    .SetUpdateAssemblyInfo(true)).Result;
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

    Target SetManifestVersions => _ => _
        .DependsOn(SetupGit)
        .Executes(() =>
        {
            if (GitVersion != null)
            {
                var manifests = GlobFiles(RootDirectory, "**/*.dnn");
                foreach (var manifest in manifests)
                {
                    var doc = new XmlDocument();
                    doc.Load(manifest);
                    var packages = doc.SelectNodes("dotnetnuke/packages/package");
                    foreach (XmlNode package in packages)
                    {
                        var version = package.Attributes["version"];
                        if (version != null)
                        {
                            version.Value = $"{GitVersion.Major.ToString("00")}.{GitVersion.Minor.ToString("00")}.{GitVersion.Patch.ToString("00")}";
                        }
                    }
                    doc.Save(manifest);
                }
            }
        });

    Target Package => _ => _
        .DependsOn(Clean)
        .DependsOn(SetManifestVersions)
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
            string fileName = new DirectoryInfo(RootDirectory).Name;
            if (GitVersion != null)
            {
                fileName += $"_{GitVersion.FullSemVer}";
            }
            fileName += "_install.zip";
            ZipFile.CreateFromDirectory(stagingDirectory, ArtifactsDirectory / fileName);
            DeleteDirectory(stagingDirectory);

            // Open folder
            if (EnvironmentInfo.IsWin)
            {
                Process.Start("explorer.exe", ArtifactsDirectory);
            }
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