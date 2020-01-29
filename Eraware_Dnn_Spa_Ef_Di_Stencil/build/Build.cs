using System;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Xml;
using BuildHelpers;
using Newtonsoft.Json.Linq;
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
using static Nuke.Common.IO.SerializationTasks;
using static Nuke.Common.IO.TextTasks;
using static Nuke.Common.IO.XmlTasks;
using static Nuke.Common.Tools.DotNet.DotNetTasks;
using static Nuke.Common.Tools.Npm.NpmTasks;

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
    AbsolutePath WebProjectDirectory => RootDirectory / "Module.Web";

    private string devViewsPath = "http://localhost:3333/build/";
    private string prodViewsPath = "DesktopModules/MyModule/resources/scripts/era-mymodule/";

    Target CleanArtifactsDirectory => _ => _
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
                Logger.Info(Helpers.Dump(GitRepository));
                Logger.Info(Helpers.Dump(GitVersion));
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
                .SetProjectFile(Solution.GetProject("Module")));
        });

    Target CompileLibraries => _ => _
        .DependsOn(Restore)
        .DependsOn(SetupGit)
        .Executes(() =>
        {
            DotNetBuild(_ => _
                .SetProjectFile(Solution.GetProject("Module"))
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

            if (GitRepository != null && GitVersion != null)
            {
                var packageJson = RootDirectory / "module.web" / "package.json";
                var packageJsonContent = JsonDeserializeFromFile<JObject>(packageJson);
                packageJsonContent["version"] = GitVersion.FullSemVer;
                if (packageJsonContent["repository"] == null)
                {
                    packageJsonContent.Property("version").AddAfterSelf(new JProperty("repository", GitRepository.HttpsUrl));
                }

                JsonSerializeToFile(packageJsonContent, packageJson);
            }
        });

    Target InstallNpmPackages => _ => _
        .Executes(() =>
        {
            NpmInstall(s =>
                s.SetWorkingDirectory(WebProjectDirectory));
        });

    Target DeployBinaries => _ => _
        .OnlyWhenDynamic(() => RootDirectory.Parent.ToString().EndsWith("DesktopModules"))
        .DependsOn(CompileLibraries)
        .Executes(() =>
        {
            var files = GlobFiles(RootDirectory, "bin/Debug/*");
            foreach (var file in files)
            {
                Helpers.CopyFileToDirectoryIfChanged(file, RootDirectory.Parent.Parent / "bin");
            }
        });

    Target BuildFrontEnd => _ => _
        .DependsOn(InstallNpmPackages)
        .Executes(() =>
        {
            NpmRun(s => s
                .SetWorkingDirectory(WebProjectDirectory)
                .SetArgumentConfigurator(a => a.Add("build"))
            );
        });

    Target DeployFrontEnd => _ => _
        .DependsOn(BuildFrontEnd)
        .Executes(() =>
        {
            var scriptsDestination = RootDirectory / "resources" / "scripts" / "$ext_scopeprefixkebab$";
            EnsureCleanDirectory(scriptsDestination);
            CopyDirectoryRecursively(RootDirectory / "module.web" / "dist" / "$ext_scopeprefixkebab$", scriptsDestination, Nuke.Common.IO.DirectoryExistsPolicy.Merge);
        });

    Target SetRelativeScripts => _ => _
        .DependsOn(DeployFrontEnd)
        .Executes(() =>
        {
            var views = GlobFiles(RootDirectory, "resources/views/**/*.html");
            foreach (var view in views)
            {
                var content = ReadAllText(view);
                content = content.Replace("http://localhost:3333/build/", "DesktopModules/MyModule/resources/scripts/$ext_scopeprefixkebab$/");
                WriteAllText(view, content, System.Text.Encoding.UTF8);
                Logger.Info("Set scripts path to {0} in {1}", prodViewsPath, view);
            }
        });

    Target SetLiveServer => _ => _
        .DependsOn(DeployFrontEnd)
        .Executes(() =>
        {
            var views = GlobFiles(RootDirectory, "resources/views/**/*.html");
            foreach (var view in views)
            {
                var content = ReadAllText(view);
                content = content.Replace(prodViewsPath, devViewsPath);
                WriteAllText(view, content, System.Text.Encoding.UTF8);
                Logger.Info("Set scripts path to {0} in {1}", devViewsPath, view);
            }
        });

    Target GenerateAppConfig => _ => _
    .OnlyWhenDynamic(() => RootDirectory.Parent.ToString().EndsWith("DesktopModules"))
    .Executes(() =>
    {
        var webConfigPath = RootDirectory.Parent.Parent / "web.config";
        var webConfigDoc = new XmlDocument();
        webConfigDoc.Load(webConfigPath);
        var connectionString = webConfigDoc.SelectSingleNode("/configuration/connectionStrings/add[@name='SiteSqlServer']");

        var appConfigPath = RootDirectory / "_build" / "App.config";
        var appConfig = new XmlDocument();
        var configurationNode = appConfig.AppendChild(appConfig.CreateElement("configuration"));
        var connectionStringsNode = configurationNode.AppendChild(appConfig.CreateElement("connectionStrings"));
        var importedNode = connectionStringsNode.OwnerDocument.ImportNode(connectionString, true);
        connectionStringsNode.AppendChild(importedNode);
        appConfig.Save(appConfigPath);

        Logger.Info("Generated {0} from {1}", appConfigPath, webConfigPath);
        Logger.Info("This file is local as it could contain credentials, it should not be committed to the repository.");
    });

    /// <summary>
    /// Package the module
    /// </summary>
    Target Package => _ => _
        .DependsOn(CleanArtifactsDirectory)
        .DependsOn(SetManifestVersions)
        .DependsOn(CompileLibraries)
        .DependsOn(SetRelativeScripts)
        .DependsOn(GenerateAppConfig)
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
            if (IsWin)
            {
                Process.Start("explorer.exe", ArtifactsDirectory);
            }

            Logger.Success("Packaging succeeded!");
        });

    /// <summary>
    /// Deploy the module files
    /// </summary>
    Target Deploy => _ => _
        .DependsOn(DeployBinaries)
        .DependsOn(SetRelativeScripts)
        .DependsOn(GenerateAppConfig)
        .Executes(() =>
        {

        });

    /// <summary>
    /// Watch frontend for changes
    /// </summary>
    Target Watch => _ => _
    .DependsOn(DeployBinaries)
    .DependsOn(InstallNpmPackages)
    .DependsOn(SetLiveServer)
    .DependsOn(GenerateAppConfig)
    .Executes(() =>
    {
        NpmRun(s => s
            .SetWorkingDirectory(WebProjectDirectory)
            .SetArgumentConfigurator(a => a.Add("start"))
            );
    });
}