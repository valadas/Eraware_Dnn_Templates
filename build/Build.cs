using System;
using System.Linq;
using Nuke.Common;
using Nuke.Common.CI;
using Nuke.Common.CI.GitHubActions;
using Nuke.Common.Execution;
using Nuke.Common.IO;
using Nuke.Common.ProjectModel;
using Nuke.Common.Tooling;
using Nuke.Common.Tools.DotNet;
using Nuke.Common.Tools.MSBuild;
using Nuke.Common.Utilities.Collections;
using Octokit;
using static Nuke.Common.EnvironmentInfo;
using static Nuke.Common.IO.FileSystemTasks;
using static Nuke.Common.IO.PathConstruction;
using static Nuke.Common.Tools.DotNet.DotNetTasks;
using static Nuke.Common.Tools.MSBuild.MSBuildTasks;

[GitHubActions(
    "Build",
    GitHubActionsImage.WindowsLatest,
    OnPullRequestBranches = new[] { "master", "main", "develop", "development", "release/*" },
    OnPushBranches = new[] { "master", "develop", "release/*" },
    InvokedTargets = new[] { nameof(Compile) },
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

    AbsolutePath ArtifactsDirectory => RootDirectory / "artifacts";

    Target Clean => _ => _
        .Before(Restore)
        .Executes(() =>
        {
            ArtifactsDirectory.CreateOrCleanDirectory();
        });

    Target Restore => _ => _
        .Executes(() =>
        {
            var projects = Solution.GetAllProjects("*");

            foreach (var project in projects.Where(p => p.Name != "_build"))
            {
                DotNetRestore(s => s
                    .SetProjectFile(project));
            }
        });

    Target Compile => _ => _
        .DependsOn(Restore)
        .Produces(ArtifactsDirectory / "*.vsix")
        .Executes(() =>
        {
            MSBuild(s => s
                .SetTargetPath(Solution)
                .SetConfiguration(Configuration));
        });

}
