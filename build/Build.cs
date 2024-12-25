using System;
using System.IO;
using System.Linq;
using System.Xml;
using Nuke.Common;
using Nuke.Common.CI;
using Nuke.Common.CI.GitHubActions;
using Nuke.Common.Execution;
using Nuke.Common.Git;
using Nuke.Common.IO;
using Nuke.Common.ProjectModel;
using Nuke.Common.Tooling;
using Nuke.Common.Tools.DotNet;
using Nuke.Common.Tools.GitHub;
using Nuke.Common.Tools.GitVersion;
using Nuke.Common.Tools.MSBuild;
using Nuke.Common.Utilities.Collections;
using Octokit;
using static Nuke.Common.EnvironmentInfo;
using static Nuke.Common.IO.FileSystemTasks;
using static Nuke.Common.IO.PathConstruction;
using static Nuke.Common.Tools.DotNet.DotNetTasks;
using static Nuke.Common.Tools.GitHub.GitHubTasks;
using static Nuke.Common.Tools.MSBuild.MSBuildTasks;

[GitHubActions(
    "Build",
    GitHubActionsImage.WindowsLatest,
    EnableGitHubToken = true,
    OnPullRequestBranches = new[] { "master", "main", "develop", "development", "release/*" },
    OnPushBranches = new[] { "master", "develop", "release/*" },
    InvokedTargets = new[] { nameof(CI) },
    FetchDepth = 0,
    CacheKeyFiles = new string[0]
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
    
    [Solution] readonly Solution Solution;
    [GitRepository] readonly GitRepository GitRepository;
    [GitVersion(UpdateAssemblyInfo = false)] readonly GitVersion GitVersion;

    static GitHubActions GitHubActions => GitHubActions.Instance;
    static readonly string PackageContentType = "application/octet-stream";

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

    Target SetVsixVersion => _ => _
        .DependsOn(Restore)
        .Executes(() =>
        {
            var manifestFile = TemplateProjectDirectory / "source.extension.vsixmanifest";
            var manifest = new XmlDocument();
            manifest.Load(manifestFile);
            var metadataNode = manifest.DocumentElement.ChildNodes.Cast<XmlNode>().First(n => n.Name == "Metadata") as XmlElement;
            var identityNode = metadataNode.ChildNodes.Cast<XmlNode>().First(n => n.Name == "Identity") as XmlElement;
            var versionAttribute = identityNode.Attributes["Version"];
            versionAttribute.Value = GitVersion.MajorMinorPatch;
            manifest.Save(manifestFile);
        });

    Target Compile => _ => _
        .DependsOn(Restore)
        .DependsOn(SetVsixVersion)
        .Executes(() =>
        {
            MSBuild(s => s
                .SetTargetPath(Solution)
                .SetConfiguration(Configuration)
                .SetAssemblyVersion(GitVersion.AssemblySemVer)
                .SetFileVersion(GitVersion.MajorMinorPatch)
                .SetPackageVersion(GitVersion.MajorMinorPatch));

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
        .OnlyWhenStatic(() => GitRepository.IsOnReleaseBranch() || GitRepository.IsOnMainOrMasterBranch())
        .Requires(() => Configuration.Equals(Configuration.Release))
        .Executes(async () =>
        {
            var credentials = new Credentials(GitHubActions.Token);
            GitHubTasks.GitHubClient = new GitHubClient(new ProductHeaderValue("Eraware.Dnn.Templates"))
            {
                Credentials = credentials,
            };
            var (owner, name) = (GitRepository.GetGitHubOwner(), GitRepository.GetGitHubName());
            var version = GitRepository.IsOnMainOrMasterBranch() ? GitVersion.MajorMinorPatch : GitVersion.FullSemVer;
            var newRelease = new NewRelease(GitVersion.FullSemVer)
            {
                Draft = true,
                Name = $"v{version}",
                GenerateReleaseNotes = true,
                TargetCommitish = GitVersion.Sha,
                Prerelease = GitRepository.IsOnReleaseBranch(),
            };

            var createdRelease = await GitHubTasks
                .GitHubClient
                .Repository
                .Release
                .Create(owner, name, newRelease);

            ArtifactsDirectory.GlobFiles("*")
                .ForEach(async file =>
                {
                    await using var artifactStream = File.OpenRead(file);
                    var fileName = Path.GetFileName(file);
                    var assetUpload = new ReleaseAssetUpload
                    {
                        FileName = fileName,
                    };
                });
            
        });
        
}
