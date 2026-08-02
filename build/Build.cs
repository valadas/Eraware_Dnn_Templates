using Nuke.Common;
using Nuke.Common.CI.GitHubActions;
using Nuke.Common.Git;
using Nuke.Common.IO;
using Nuke.Common.ProjectModel;
using Nuke.Common.Tools.DotNet;
using Nuke.Common.Tools.GitHub;
using Nuke.Common.Tools.GitVersion;
using Nuke.Common.Tools.MSBuild;
using Octokit;
using System;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Xml;
using static Nuke.Common.Tools.DotNet.DotNetTasks;
using static Nuke.Common.Tools.Git.GitTasks;
using static Nuke.Common.Tools.GitHub.GitHubTasks;
using static Nuke.Common.Tools.MSBuild.MSBuildTasks;

[CustomGitHubActions(
    "Build",
    GitHubActionsImage.WindowsLatest,
    ImportSecrets = new[] { nameof(GithubToken) },
    OnPullRequestBranches = new[] { "master", "main", "develop", "development", "release/*" },
    OnPushBranches = new[] { "master", "develop", "release/*" },
    InvokedTargets = new[] { nameof(CI) },
    FetchDepth = 0,
    CacheKeyFiles = new string[0],
    PublishArtifacts = true
    )]
class Build : NukeBuild
{
    /// Support plugins are available for:
    ///   - JetBrains ReSharper        https://nuke.build/resharper
    ///   - JetBrains Rider            https://nuke.build/rider
    ///   - Microsoft VisualStudio     https://nuke.build/visualstudio
    ///   - Microsoft VSCode           https://nuke.build/vscode

    public static int Main () => Execute<Build>(x => x.Compile);

    [Parameter("Configuration to build - Default is 'Debug' (local) or 'Release' (server)")]
    readonly Configuration Configuration = IsLocalBuild ? Configuration.Debug : Configuration.Release;

    [Parameter("Github Token")]
    [Secret]
    readonly string GithubToken;

    [Solution] readonly Solution Solution;
    [GitRepository] readonly GitRepository GitRepository;
    [GitVersion(UpdateAssemblyInfo = false)] readonly GitVersion GitVersion;

    AbsolutePath ArtifactsDirectory => RootDirectory / "artifacts";
    AbsolutePath TemplateProjectDirectory => RootDirectory / "Eraware_Dnn_Templates";

    Target Clean => _ => _
        .Before(Restore)
        .Executes(() =>
        {
            ArtifactsDirectory.CreateOrCleanDirectory();
            var projects = Solution.GetAllProjects("*");
            foreach (var project in projects.Where(p => p.Name != "_build"))
            {
                (project.Path / "bin").DeleteDirectory();
                (project.Path / "obj").DeleteDirectory();
            }
        });

    Target Restore => _ => _
        .DependsOn(Clean)
        .Executes(() =>
        {
            var projects = Solution.GetAllProjects("*");

            foreach (var project in projects.Where(p => p.Name != "_build"))
            {
                DotNetRestore(s => s
                    .SetProjectFile(project));
            }
        });

    Target SetVersion => _ => _
        .DependsOn(Restore)
        .Executes(() =>
        {
            var version = $"{GitVersion.MajorMinorPatch}.0";
            Serilog.Log.Information($"Setting version to: {version}");

            // Update VSIX manifest version
            var manifestFile = TemplateProjectDirectory / "source.extension.vsixmanifest";
            var manifest = new XmlDocument();
            manifest.Load(manifestFile);
            var metadataNode = manifest.DocumentElement.ChildNodes.Cast<XmlNode>().First(n => n.Name == "Metadata") as XmlElement;
            var identityNode = metadataNode.ChildNodes.Cast<XmlNode>().First(n => n.Name == "Identity") as XmlElement;
            var versionAttribute = identityNode.Attributes["Version"];
            versionAttribute.Value = GitVersion.MajorMinorPatch;
            manifest.Save(manifestFile);
            Serilog.Log.Information($"Updated VSIX manifest to version: {GitVersion.MajorMinorPatch}");

            // Update assembly version references in .vstemplate files
            var templateFiles = RootDirectory.GlobFiles("**/*.vstemplate")
                .Where(f => f.ReadAllText().Contains("WizardExtension"));

            foreach (var templateFile in templateFiles)
            {
                var templateDoc = new XmlDocument();
                templateDoc.Load(templateFile);
                
                // Create namespace manager for the VSTemplate namespace
                var nsManager = new XmlNamespaceManager(templateDoc.NameTable);
                nsManager.AddNamespace("vst", "http://schemas.microsoft.com/developer/vstemplate/2005");
                
                // Use namespace-aware XPath to find WizardExtension
                var wizardExtensionNode = templateDoc.SelectSingleNode("//vst:WizardExtension", nsManager);
                if (wizardExtensionNode != null)
                {
                    // Find Assembly child node using namespace-aware XPath
                    var assemblyNode = wizardExtensionNode.SelectSingleNode("vst:Assembly", nsManager);
                    if (assemblyNode != null)
                    {
                        var assemblyText = assemblyNode.InnerText;
                        // Update the version in the assembly reference
                        // Format: "Eraware_Dnn_Templates, Version=X.X.X.X, Culture=Neutral, PublicKeyToken=..."
                        var updatedAssembly = Regex.Replace(
                            assemblyText,
                            @"Version=\d+\.\d+\.\d+\.\d+",
                            $"Version={version}"
                        );
                        assemblyNode.InnerText = updatedAssembly;
                        templateDoc.Save(templateFile);
                        Serilog.Log.Information($"Updated assembly version in {templateFile}: {assemblyText} -> {updatedAssembly}");
                    }
                    else
                    {
                        Serilog.Log.Warning($"Assembly node not found in WizardExtension for {templateFile}");
                    }
                }
                else
                {
                    Serilog.Log.Warning($"WizardExtension node not found in {templateFile}");
                }
            }

            // Update AssemblyInfo.cs version
            var assemblyInfoFile = TemplateProjectDirectory / "Properties" / "AssemblyInfo.cs";
            if (assemblyInfoFile.FileExists())
            {
                var content = assemblyInfoFile.ReadAllText();
                content = Regex.Replace(
                    content,
                    @"AssemblyVersion\(""[^""]*""\)",
                    $@"AssemblyVersion(""{version}"")"
                );
                content = Regex.Replace(
                    content,
                    @"AssemblyFileVersion\(""[^""]*""\)",
                    $@"AssemblyFileVersion(""{version}"")"
                );
                content = Regex.Replace(
                    content,
                    @"AssemblyInformationalVersion\(""[^""]*""\)",
                    $@"AssemblyInformationalVersion(""{GitVersion.InformationalVersion}"")"
                );
                assemblyInfoFile.WriteAllText(content);
                Serilog.Log.Information($"Updated AssemblyInfo.cs version to {version} and informational version to {GitVersion.InformationalVersion}");
            }
        });

    Target Compile => _ => _
        .DependsOn(Restore)
        .DependsOn(SetVersion)
        .Executes(() =>
        {
            MSBuild(s => s
                .SetTargetPath(Solution)
                .SetConfiguration(Configuration));

            var vsix = TemplateProjectDirectory / "bin" / Configuration / "Eraware_Dnn_Templates.vsix";
            vsix.CopyToDirectory(ArtifactsDirectory, ExistsPolicy.FileOverwrite);
        });

    Target CI => _ => _
        .Description("Handles everything needed for CI")
        .DependsOn(Compile)
        .Produces(ArtifactsDirectory / "*.vsix")
        .Triggers(Release)
        .Executes(() =>
        {
        });

    Target Release => _ => _
        .OnlyWhenDynamic(() => GitRepository.IsOnMainOrMasterBranch() || GitRepository.IsOnReleaseBranch())
        .OnlyWhenDynamic(() => !string.IsNullOrEmpty(GithubToken))
        .Executes(async () =>
        {
            Serilog.Log.Information($"Running release for branch {GitRepository.Branch}");
            Serilog.Log.Information($"IsDevelopBranch: {GitRepository.IsOnDevelopBranch()}");
            Serilog.Log.Information($"IsMainOrMasterBranch: {GitRepository.IsOnMainOrMasterBranch()}");
            Serilog.Log.Information($"IsReleaseBranch: {GitRepository.IsOnReleaseBranch()}");
            if (GitRepository.IsOnDevelopBranch())
            {
                Serilog.Log.Information("Skipping release on develop branch");
                return;
            }
            var version = GitRepository.IsOnMainOrMasterBranch() 
                ? GitVersion.MajorMinorPatch 
                : GitVersion.SemVer;
            var releaseTag = $"v{version}";

            var actor = Environment.GetEnvironmentVariable("GITHUB_ACTOR");
            Git($"config --global user.name '{actor}'");
            Git($"config --global user.email '{actor}@github.com'");
            if (IsServerBuild)
            {
                Git($"remote set-url origin https://{actor}:{GithubToken}@github.com/{GitRepository.GetGitHubOwner()}/{GitRepository.GetGitHubName()}.git");
            }

            // Create the Git tag first using GitTasks
            Git($"tag -a {releaseTag} -m \"Release {releaseTag}\"", RootDirectory);
            
            // Push the tag to origin using GitTasks
            Git($"push origin {releaseTag}", RootDirectory);
            
            Serilog.Log.Information($"Git tag {releaseTag} created and pushed");
            
            var credentials = new Credentials(GithubToken);
            GitHubTasks.GitHubClient = new GitHubClient(new ProductHeaderValue("Eraware.Dnn.Templates"))
            {
                Credentials = credentials,
            };
            var (owner, name) = (GitRepository.GetGitHubOwner(), GitRepository.GetGitHubName());
            
            Serilog.Log.Information($"Creating GitHub release with tag: {releaseTag}");
            
            var newRelease = new NewRelease(releaseTag)
            {
                Draft = true,
                Name = releaseTag,
                GenerateReleaseNotes = true,
                TargetCommitish = GitVersion.Sha,
                Prerelease = GitRepository.IsOnReleaseBranch(),
            };

            var createdRelease = await GitHubTasks
                .GitHubClient
                .Repository
                .Release
                .Create(owner, name, newRelease);

            // Upload assets
            var artifactFiles = ArtifactsDirectory.GlobFiles("*");
            
            foreach (var file in artifactFiles)
            {
                await using var artifactStream = File.OpenRead(file);
                var fileName = Path.GetFileName(file);
                var assetUpload = new ReleaseAssetUpload
                {
                    FileName = fileName,
                    ContentType = "application/octet-stream",
                    RawData = artifactStream
                };
                
                await GitHubTasks
                    .GitHubClient
                    .Repository
                    .Release
                    .UploadAsset(createdRelease, assetUpload);
            }
            
            Serilog.Log.Information($"Release process completed: {releaseTag}");
        });
}
