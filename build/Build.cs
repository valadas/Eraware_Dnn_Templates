using Nuke.Common;
using Nuke.Common.CI.GitHubActions;
using Nuke.Common.Git;
using Nuke.Common.IO;
using Nuke.Common.ProjectModel;
using Nuke.Common.Tooling;
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

    // NUKE's built-in MSBuild resolver cannot locate MSBuild for VS 2026 (v18).
    // The "microsoft/setup-msbuild" CI step adds MSBuild.exe to PATH, so we resolve it from there.
    string MSBuildToolPath => System.Environment
        .GetEnvironmentVariable("PATH")
        .Split(Path.PathSeparator)
        .Select(dir => Path.Combine(dir, "MSBuild.exe"))
        .FirstOrDefault(File.Exists);

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

            // NOTE: The wizard assembly is strong-named, so the .vstemplate files must
            // reference it by its FULL display name. They pin Version=1.0.0.0, which
            // matches the permanently-fixed AssemblyVersion in AssemblyInfo.cs. Because
            // that binding version never changes, there is nothing to rewrite here and
            // the "template attempted to load component assembly ... Version=X.X.X.X"
            // failures can no longer occur. The release version lives only in the VSIX
            // manifest above (plus file/informational versions below).

            // Update AssemblyInfo.cs version.
            // IMPORTANT: AssemblyVersion is intentionally left UNTOUCHED and kept fixed so
            // the strong-name binding of the wizard assembly is stable across releases.
            // Only the file version (informational only) and informational version change.
            var assemblyInfoFile = TemplateProjectDirectory / "Properties" / "AssemblyInfo.cs";
            if (assemblyInfoFile.FileExists())
            {
                var content = assemblyInfoFile.ReadAllText();
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
                Serilog.Log.Information($"Updated AssemblyInfo.cs file version to {version} and informational version to {GitVersion.InformationalVersion} (AssemblyVersion kept fixed for stable strong-name binding)");
            }
        });

    Target Compile => _ => _
        .DependsOn(Restore)
        .DependsOn(SetVersion)
        .Executes(() =>
        {
            var msBuildToolPath = MSBuildToolPath;
            MSBuild(s => s
                .SetProcessToolPath(msBuildToolPath)
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
