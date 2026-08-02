using System.Collections.Generic;
using System.Linq;
using Nuke.Common.CI.GitHubActions;
using Nuke.Common.CI.GitHubActions.Configuration;
using Nuke.Common.Execution;
using Nuke.Common.Utilities;

/// <summary>
/// Custom GitHub Actions attribute that injects a "microsoft/setup-msbuild" step
/// so that NUKE's <c>MSBuild</c> task can locate a Visual Studio MSBuild instance
/// on the hosted runner. This is required because the VSIX (VSSDK) projects can
/// only be built with the full Visual Studio MSBuild, not the .NET SDK MSBuild.
/// </summary>
class CustomGitHubActionsAttribute : GitHubActionsAttribute
{
    public CustomGitHubActionsAttribute(
        string name,
        GitHubActionsImage image,
        params GitHubActionsImage[] images)
        : base(name, image, images)
    {
    }

    protected override GitHubActionsJob GetJobs(GitHubActionsImage image, IReadOnlyCollection<ExecutableTarget> relevantTargets)
    {
        var job = base.GetJobs(image, relevantTargets);

        var steps = job.Steps.ToList();

        // Insert right after the checkout step so MSBuild is on PATH before the build runs.
        steps.Insert(1, new GitHubActionsSetupMSBuildStep());

        job.Steps = steps.ToArray();
        return job;
    }
}

/// <summary>
/// Emits a step that adds MSBuild.exe from the runner's Visual Studio installation to PATH.
/// </summary>
class GitHubActionsSetupMSBuildStep : GitHubActionsStep
{
    public override void Write(CustomFileWriter writer)
    {
        writer.WriteLine("- name: Setup MSBuild");
        writer.WriteLine("  uses: microsoft/setup-msbuild@v2");
    }
}
