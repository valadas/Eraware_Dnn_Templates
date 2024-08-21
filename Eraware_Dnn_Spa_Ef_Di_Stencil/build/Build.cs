using BuildHelpers;
using Nuke.Common;
using Nuke.Common.CI.GitHubActions;
using Nuke.Common.Execution;
using Nuke.Common.Git;
using Nuke.Common.IO;
using Nuke.Common.ProjectModel;
using Nuke.Common.Tooling;
using Nuke.Common.Tools.Coverlet;
using Nuke.Common.Tools.DocFX;
using Nuke.Common.Tools.DotNet;
using Nuke.Common.Tools.GitHub;
using Nuke.Common.Tools.GitVersion;
using Nuke.Common.Tools.MSBuild;
using Nuke.Common.Tools.Npm;
using Nuke.Common.Tools.NSwag;
using Nuke.Common.Tools.ReportGenerator;
using Nuke.Common.Tools.VSTest;
using Nuke.Common.Tools.Xunit;
using Nuke.Common.Utilities;
using Nuke.Common.Utilities.Collections;
using Octokit;
using System;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using static Nuke.Common.EnvironmentInfo;
using static Nuke.Common.IO.CompressionTasks;
using static Nuke.Common.IO.FileSystemTasks;
using static Nuke.Common.IO.PathConstruction;
using static Nuke.Common.IO.TextTasks;
using static Nuke.Common.Tools.DotNet.DotNetTasks;
using static Nuke.Common.Tools.Git.GitTasks;
using static Nuke.Common.Tools.MSBuild.MSBuildTasks;
using static Nuke.Common.Tools.Npm.NpmTasks;
using static Nuke.Common.Tools.ReportGenerator.ReportGeneratorTasks;

[GitHubActions(
    "Build",
    GitHubActionsImage.UbuntuLatest,
    ImportSecrets = new[] { nameof(GitHubToken) },
    OnPullRequestBranches = new[] { "develop", "main", "master", "release/*" },
    OnPushBranches = new[] { "main", "master", "develop", "release/*" },
    InvokedTargets = new[] { nameof(Package), nameof(DeployGeneratedFiles), nameof(Release) },
    FetchDepth = 0
    )]
[UnsetVisualStudioEnvironmentVariables]
class Build : NukeBuild
{
    /// Support plugins are available for:
    ///   - JetBrains ReSharper        https://nuke.build/resharper
    ///   - JetBrains Rider            https://nuke.build/rider
    ///   - Microsoft VisualStudio     https://nuke.build/visualstudio
    ///   - Microsoft VSCode           https://nuke.build/vscode

    public static int Main() => Execute<Build>(x => x.Package);

    [Parameter("Configuration to build - Default is 'Debug' (local) or 'Release' (server)")]
    readonly Configuration Configuration = IsLocalBuild ? Configuration.Debug : Configuration.Release;

    [Parameter("Github token to authenticate in CI")]
    readonly string GitHubToken;

    [Solution] readonly Solution Solution;
    [GitRepository] readonly GitRepository GitRepository;
    [GitVersion(Framework = "net6.0", UpdateAssemblyInfo = false, NoFetch = true)] readonly GitVersion GitVersion;

    [NuGetPackage("WebApiToOpenApiReflector", "WebApiToOpenApiReflector.dll")]
    readonly Tool WebApiToOpenApiReflector;

    AbsolutePath ArtifactsDirectory => RootDirectory / "artifacts";
    AbsolutePath InstallDirectory => RootDirectory.Parent.Parent / "Install" / "Module";
    AbsolutePath WebProjectDirectory => RootDirectory / "module.web";
    AbsolutePath TestResultsDirectory => RootDirectory / "TestResults";
    AbsolutePath UnitTestsResultsDirectory => TestResultsDirectory / "UnitTests";
    AbsolutePath IntegrationTestsResultsDirectory => TestResultsDirectory / "IntegrationTests";
    AbsolutePath GithubDirectory => RootDirectory / ".github";
    AbsolutePath BadgesDirectory => GithubDirectory / "badges";
    AbsolutePath UnitTestBadgesDirectory => BadgesDirectory / "UnitTests";
    AbsolutePath IntegrationTestsBadgesDirectory => BadgesDirectory / "IntegrationTests";
    AbsolutePath ClientServicesDirectory => WebProjectDirectory / "src" / "services";
    AbsolutePath DocFxProjectDirectory => RootDirectory / "docfx_project";
    AbsolutePath DocsDirectory => RootDirectory / "docs";

    private const string devViewsPath = "http://localhost:3333/build/";
    private const string prodViewsPath = "/DesktopModules/$ext_modulefoldername$/resources/scripts/$ext_scopeprefixkebab$/";
    private const string moduleName = "$ext_rootnamespace$";
    private bool FirstBuild = false;

    string releaseNotes = "";
    GitHubClient gitHubClient;
    Release release;

    Target UpdateTokens => _ => _
        .OnlyWhenDynamic(() => GitRepository != null && GitRepository.IsGitHubRepository())
        .Executes(() =>
        {
            if (GitRepository != null)
            {
                Serilog.Log.Information($"We are on branch {GitRepository.Branch}");
                var repositoryFiles = RootDirectory.GlobFiles("README.md", "build/**/git.html", "**/articles/git.md");
                repositoryFiles.ForEach(file =>
                {
                    var fileContent = file.ReadAllText(Encoding.UTF8);
                    fileContent = fileContent.Replace("{owner}", GitRepository.GetGitHubOwner());
                    fileContent = fileContent.Replace("{repository}", GitRepository.GetGitHubName());
                    file.WriteAllText(fileContent, Encoding.UTF8);
                });
            }
        });

    Target LogInfo => _ => _
        .Before(Release)
        .DependsOn(UpdateTokens)
        .Executes(() =>
        {
            Serilog.Log.Information($"Branch name is {GitRepository.Branch}");
            Serilog.Log.Information(GitVersion.ToJson());
        });

    Target Clean => _ => _
        .Before(Restore)
        .Before(Package)
        .Executes(() =>
        {
            ArtifactsDirectory.CreateOrCleanDirectory();
            TestResultsDirectory.CreateOrCleanDirectory();
            UnitTestsResultsDirectory.CreateOrCleanDirectory();
            IntegrationTestsResultsDirectory.CreateOrCleanDirectory();
        });

    Target Restore => _ => _
        .Executes(() =>
        {
            DotNetRestore(s => s
                .SetProjectFile(Solution.GetProject("Module")));

            DotNetRestore(s => s
                .SetProjectFile(Solution.GetProject("UnitTests")));

            DotNetRestore(s => s
                .SetProjectFile(Solution.GetProject("IntegrationTests")));
        });

    // TODO: This is a workaround for https://github.com/dnnsoftware/Dnn.Platform/issues/6024 and can be removed once a new release with that fix comes out.
    Target AdjustCasing => _ => _
        .After(Compile)
        .Executes(() =>
        {
            if (!IsWin)
            {
                var log4netFiles = RootDirectory.GlobFiles("**/DotNetNuke.Log4Net.dll");
                log4netFiles.ForEach(f => f.Rename("DotNetNuke.log4net.dll"));
            }
        });

    Target UnitTests => _ => _
        .DependsOn(Compile)
        .DependsOn(AdjustCasing)
        .Executes(() =>
        {
            MSBuild(_ => _
                .SetConfiguration(Configuration)
                .SetProjectFile(Solution.GetProject("UnitTests"))
                .SetTargets("Build")
                .ResetVerbosity());

            DotNetTest(_ => _
                .SetConfiguration(Configuration)
                .ResetVerbosity()
                .SetResultsDirectory(UnitTestsResultsDirectory)
                .EnableCollectCoverage()
                .SetCoverletOutputFormat(CoverletOutputFormat.cobertura)
                .SetLoggers("trx;LogFileName=UnitTests.trx")
                .SetCoverletOutput(UnitTestsResultsDirectory / "coverage.xml")
                .SetExcludeByFile("**/App_LocalResources/**/*")
                .SetProjectFile(Solution.GetProject("UnitTests"))
                .SetNoBuild(true));

            ReportGenerator(_ => _
                .SetReports(UnitTestsResultsDirectory / "*.xml")
                .SetReportTypes(ReportTypes.Badges, ReportTypes.HtmlInline, ReportTypes.HtmlChart)
                .SetTargetDirectory(UnitTestsResultsDirectory)
                .SetHistoryDirectory(RootDirectory / "UnitTests" / "history")
                .SetProcessArgumentConfigurator(a => a
                    .Add("-title:UnitTests"))
                .SetFramework("net5.0"));

            Helpers.CleanCodeCoverageHistoryFiles(RootDirectory / "UnitTests" / "history");

            var testBadges = UnitTestsResultsDirectory.GlobFiles("badge_branchcoverage.svg", "badge_linecoverage.svg");
            testBadges.ForEach(f => CopyFileToDirectory(f, UnitTestBadgesDirectory, FileExistsPolicy.Overwrite, true));

            if (IsWin && (InvokedTargets.Contains(UnitTests) || InvokedTargets.Contains(Test)))
            {
                Process.Start(@"cmd.exe ", @"/c " + (UnitTestsResultsDirectory / "index.html"));
            }
        });

    Target IntegrationTests => _ => _
        .DependsOn(Compile)
        .DependsOn(AdjustCasing)
        .Executes(() =>
        {
            MSBuild(_ => _
                .SetConfiguration(Configuration)
                .SetProjectFile(Solution.GetProject("IntegrationTests"))
                .SetTargets("Build")
                .ResetVerbosity());

            DotNetTest(_ => _
                .SetConfiguration(Configuration)
                .ResetVerbosity()
                .SetResultsDirectory(IntegrationTestsResultsDirectory)
                .EnableCollectCoverage()
                .SetCoverletOutputFormat(CoverletOutputFormat.cobertura)
                .SetLoggers("trx;LogFileName=IntegrationTests.trx")
                .SetCoverletOutput(IntegrationTestsResultsDirectory / "coverage.xml")
                .SetProjectFile(Solution.GetProject("IntegrationTests"))
                .SetNoBuild(true));

            ReportGenerator(_ => _
                .SetReports(IntegrationTestsResultsDirectory / "*.xml")
                .SetReportTypes(ReportTypes.Badges, ReportTypes.HtmlInline, ReportTypes.HtmlChart)
                .SetHistoryDirectory(RootDirectory / "IntegrationTests" / "history")
                .SetTargetDirectory(IntegrationTestsResultsDirectory)
                .AddClassFilters("-*Data.ModuleDbContext")
                .SetProcessArgumentConfigurator(a => a
                    .Add("-title:IntegrationTests"))
                .SetFramework("net5.0"));

            Helpers.CleanCodeCoverageHistoryFiles(RootDirectory / "IntegrationTests" / "history");

            var testBadges = IntegrationTestsResultsDirectory.GlobFiles("badge_branchcoverage.svg", "badge_linecoverage.svg");
            testBadges.ForEach(f => CopyFileToDirectory(f, IntegrationTestsBadgesDirectory, FileExistsPolicy.Overwrite, true));

            if (IsWin && (InvokedTargets.Contains(IntegrationTests) || InvokedTargets.Contains(Test)))
            {
                Process.Start(@"cmd.exe ", @"/c " + (IntegrationTestsResultsDirectory / "index.html"));
            }
        });

    Target Test => _ => _
        .DependsOn(UnitTests)
        .DependsOn(IntegrationTests)
        .Executes(() =>
        {
        });

    Target Compile => _ => _
        .DependsOn(Clean)
        .DependsOn(Restore)
        .DependsOn(SetManifestVersions)
        .DependsOn(UpdateTokens)
        .DependsOn(EnsureBootstrapingScriptsAreExecutable)
        .Executes(() =>
        {
            var moduleAssemblyName = Solution.GetProject("Module").GetProperty("AssemblyName");
            Helpers.GenerateLocalizationFiles(moduleAssemblyName);
            var assemblyVersion = "0.1.0";
            var fileVersion = "0.1.0";
            if (GitVersion != null && GitRepository != null && GitRepository.IsOnMainOrMasterBranch())
            {
                assemblyVersion = GitVersion.AssemblySemVer;
                fileVersion = GitVersion.InformationalVersion;
            }

            MSBuildTasks.MSBuild(s => s
                .SetProjectFile(Solution.GetProject("Module"))
                .SetConfiguration(Configuration)
                .SetAssemblyVersion(assemblyVersion)
                .SetFileVersion(fileVersion));

            MSBuildTasks.MSBuild(s => s
                .SetProjectFile(Solution.GetProject("UnitTests"))
                .SetConfiguration(Configuration)
                .SetAssemblyVersion(assemblyVersion)
                .SetFileVersion(fileVersion));

            MSBuildTasks.MSBuild(s => s
                .SetProjectFile(Solution.GetProject("IntegrationTests"))
                .SetConfiguration(Configuration)
                .SetAssemblyVersion(assemblyVersion)
                .SetFileVersion(fileVersion));
        });

    Target SetManifestVersions => _ => _
        .Executes(() =>
        {
            var manifests = RootDirectory.GlobFiles("*.dnn");
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
                        Serilog.Log.Information($"Found package {package.Attributes["name"].Value} with version {version.Value}");
                        version.Value =
                            GitVersion != null
                            ? $"{GitVersion.Major.ToString("00", CultureInfo.InvariantCulture)}.{GitVersion.Minor.ToString("00", CultureInfo.InvariantCulture)}.{GitVersion.Patch.ToString("00", CultureInfo.InvariantCulture)}"
                            : "00.01.00";
                        Serilog.Log.Information($"Updated package {package.Attributes["name"].Value} to version {version.Value}");

                        var components = package.SelectNodes("components/component");
                        foreach (XmlNode component in components)
                        {
                            if (component.Attributes["type"].Value == "Cleanup")
                            {
                                var cleanupVersion = component.Attributes["version"];
                                cleanupVersion.Value =
                                    GitVersion != null
                                    ? $"{GitVersion.Major.ToString("00", CultureInfo.InvariantCulture)}.{GitVersion.Minor.ToString("00", CultureInfo.InvariantCulture)}.{GitVersion.Patch.ToString("00", CultureInfo.InvariantCulture)}"
                                    : "00.01.00";
                            }
                        }
                    }
                }
                doc.Save(manifest);
                Serilog.Log.Information($"Saved {manifest}");
            }
        });

    Target DeployBinaries => _ => _
        .OnlyWhenDynamic(() => RootDirectory.Parent.ToString().EndsWith("DesktopModules", StringComparison.OrdinalIgnoreCase))
        .DependsOn(Compile)
        .Executes(() =>
        {
            var manifest = RootDirectory.GlobFiles("*.dnn").FirstOrDefault();
            var assemblyFiles = Helpers.GetAssembliesFromManifest(manifest);
            var files = RootDirectory.GlobFiles("bin/Debug/*.dll", "bin/Debug/*.pdb", "bin/Debug/*.xml");
            foreach (var file in files)
            {
                var fileInfo = new FileInfo(file);
                if (assemblyFiles.Contains(fileInfo.Name))
                {
                    Helpers.CopyFileToDirectoryIfChanged(file, RootDirectory.Parent.Parent / "bin");
                }
            }
        });

    Target SetRelativeScripts => _ => _
        .DependsOn(DeployFrontEnd)
        .Executes(() =>
        {
            var views = RootDirectory.GlobFiles("resources/views/**/*.html");
            foreach (var view in views)
            {
                var content = view.ReadAllText();
                content = content.Replace(devViewsPath, prodViewsPath, StringComparison.OrdinalIgnoreCase);
                view.WriteAllText(content, System.Text.Encoding.UTF8);
                Serilog.Log.Information("Set scripts path to {0} in {1}", prodViewsPath, view);
            }
        });

    Target SetLiveServer => _ => _
        .DependsOn(DeployFrontEnd)
        .Executes(() =>
        {
            var views = RootDirectory.GlobFiles("resources/views/**/*.html");
            foreach (var view in views)
            {
                var content = view.ReadAllText();
                content = content.Replace(prodViewsPath, devViewsPath, StringComparison.OrdinalIgnoreCase);
                view.WriteAllText(content, System.Text.Encoding.UTF8);
                Serilog.Log.Information("Set scripts path to {0} in {1}", devViewsPath, view);
            }
        });

    Target DeployFrontEnd => _ => _
        .DependsOn(BuildFrontEnd)
        .Executes(() =>
        {
            var scriptsDestination = RootDirectory / "resources" / "scripts" / "$ext_scopeprefixkebab$";
            scriptsDestination.CreateOrCleanDirectory();
            CopyDirectoryRecursively(RootDirectory / "module.web" / "dist" / "$ext_scopeprefixkebab$", scriptsDestination, DirectoryExistsPolicy.Merge);
        });

    Target InstallNpmPackages => _ => _
        .Executes(() =>
        {
            NpmLogger = (type, output) =>
            {
                if (type == OutputType.Std)
                {
                    Serilog.Log.Information(output);
                }
                if (type == OutputType.Err)
                {
                    if (output.StartsWith("npm WARN", StringComparison.OrdinalIgnoreCase))
                    {
                        Serilog.Log.Warning(output);
                    }
                    else
                    {
                        Serilog.Log.Error(output);
                    }
                }
            };
            NpmInstall(s =>
                s.SetProcessWorkingDirectory(WebProjectDirectory));
        });

    Target BuildFrontEnd => _ => _
        .DependsOn(InstallNpmPackages)
        .DependsOn(SetManifestVersions)
        .DependsOn(SetPackagesVersions)
        .DependsOn(Swagger)
        .Executes(() =>
        {
            NpmRun(s => s
                .SetProcessWorkingDirectory(WebProjectDirectory)
                .AddArguments("build")
            );
        });

    Target SetPackagesVersions => _ => _
        .DependsOn(UpdateTokens)
        .Executes(() =>
        {
            if (GitVersion != null)
            {
                Npm($"version --no-git-tag-version --allow-same-version {GitVersion.MajorMinorPatch}", WebProjectDirectory);
            }
        });

    Target SetupGitHubClient => _ => _
        .OnlyWhenDynamic(() => !string.IsNullOrWhiteSpace(GitHubToken))
        .OnlyWhenDynamic(() => GitRepository != null)
        .DependsOn(UpdateTokens)
        .Executes(() =>
        {
            Serilog.Log.Information($"We are on branch {GitRepository.Branch}");
            if (GitRepository.IsOnMainOrMasterBranch() || GitRepository.IsOnReleaseBranch())
            {
                gitHubClient = new GitHubClient(new ProductHeaderValue("Nuke"));
                var tokenAuth = new Credentials(GitHubToken);
                gitHubClient.Credentials = tokenAuth;
            }
        });

    Target GenerateReleaseNotes => _ => _
        .OnlyWhenDynamic(() => GitRepository.IsOnMainOrMasterBranch() || GitRepository.IsOnReleaseBranch())
        .OnlyWhenDynamic(() => !string.IsNullOrWhiteSpace(GitHubToken))
        .DependsOn(SetupGitHubClient)
        .DependsOn(UpdateTokens)
        .Executes(() =>
        {
            // Get the milestone
            var milestone = gitHubClient.Issue.Milestone.GetAllForRepository(
                GitRepository.GetGitHubOwner(),
                GitRepository.GetGitHubName()).Result
                .Where(m => m.Title == GitVersion.MajorMinorPatch).FirstOrDefault();
            Serilog.Log.Information(milestone.ToJson());
            if (milestone == null)
            {
                Serilog.Log.Warning("Milestone not found for this version");
                releaseNotes = "No release notes for this version.";
                return;
            }

            try
            {
                // Get the PRs
                var prRequest = new PullRequestRequest()
                {
                    State = ItemStateFilter.All
                };
                var allPrs = Task.Run(() =>
                    gitHubClient.Repository.PullRequest.GetAllForRepository(
                            GitRepository.GetGitHubOwner(),
                        GitRepository.GetGitHubName(), prRequest)
                ).Result;

                var pullRequests = allPrs.Where(p =>
                    p.Milestone?.Title == milestone.Title &&
                    p.Merged == true &&
                    p.Milestone?.Title == GitVersion.MajorMinorPatch);
                Serilog.Log.Information(pullRequests.ToJson());

                // Build release notes
                var releaseNotesBuilder = new StringBuilder();
                releaseNotesBuilder
                    .AppendLine($"# {GitRepository.GetGitHubName()} {milestone.Title}")
                    .AppendLine()
                    .AppendLine($"A total of {pullRequests.Count()} pull requests where merged in this release.")
                    .AppendLine();

                foreach (var group in pullRequests.GroupBy(p => p.Labels[0]?.Name, (label, prs) => new { label, prs }))
                {
                    Serilog.Log.Information(group.ToJson());
                    releaseNotesBuilder.AppendLine($"## {group.label}");
                    foreach (var pr in group.prs)
                    {
                        Serilog.Log.Information(pr.ToJson());
                        releaseNotesBuilder.AppendLine($"- #{pr.Number} {pr.Title}. Thanks @{pr.User.Login}");
                    }
                }

                // Checksums
                releaseNotesBuilder
                    .AppendLine()
                    .Append(File.ReadAllText(ArtifactsDirectory / "checksums.md"));

                releaseNotes = releaseNotesBuilder.ToString();
                Serilog.Log.Information(releaseNotes);
            }
            catch (Exception ex)
            {
                Serilog.Log.Error(ex, "Something went wrong with the github api call.");
                throw;
            }
        });

    Target TagRelease => _ => _
        .OnlyWhenDynamic(() => GitRepository != null && (GitRepository.IsOnMainOrMasterBranch() || GitRepository.IsOnReleaseBranch()))
        .OnlyWhenDynamic(() => !string.IsNullOrWhiteSpace(GitHubToken))
        .DependsOn(SetupGitHubClient)
        .DependsOn(UpdateTokens)
        .Before(Compile)
        .Executes(() =>
        {
            Git($"remote set-url origin https://{GitRepository.GetGitHubOwner()}:{GitHubToken}@github.com/{GitRepository.GetGitHubOwner()}/{GitRepository.GetGitHubName()}.git");
            var version = GitRepository.IsOnMainOrMasterBranch() ? GitVersion.MajorMinorPatch : GitVersion.SemVer;
            GitLogger = (type, output) => Serilog.Log.Information(output);
            Git($"tag v{version}");
            Git($"push --tags");
        });

    Target Release => _ => _
        .OnlyWhenDynamic(() => GitRepository != null && (GitRepository.IsOnMainOrMasterBranch() || GitRepository.IsOnReleaseBranch()))
        .OnlyWhenDynamic(() => !string.IsNullOrWhiteSpace(GitHubToken))
        .DependsOn(UpdateTokens)
        .DependsOn(SetupGitHubClient)
        .DependsOn(GenerateReleaseNotes)
        .DependsOn(TagRelease)
        .DependsOn(Package)
        .OnlyWhenDynamic(() => GitRepository.IsOnMainOrMasterBranch() || GitRepository.IsOnReleaseBranch())
        .Executes(() =>
        {
            var newRelease = new NewRelease(GitRepository.IsOnMainOrMasterBranch() ? $"v{GitVersion.MajorMinorPatch}" : $"v{GitVersion.SemVer}")
            {
                Body = releaseNotes,
                Draft = true,
                Name = GitRepository.IsOnMainOrMasterBranch() ? $"v{GitVersion.MajorMinorPatch}" : $"v{GitVersion.SemVer}",
                TargetCommitish = GitVersion.Sha,
                Prerelease = GitRepository.IsOnReleaseBranch(),
            };
            release = gitHubClient.Repository.Release.Create(
                GitRepository.GetGitHubOwner(),
                GitRepository.GetGitHubName(),
                newRelease).Result;
            Serilog.Log.Information($"{release.Name} released !");

            var artifactFile = RootDirectory.GlobFiles("artifacts/**/*.zip").FirstOrDefault();
            var artifact = File.OpenRead(artifactFile);
            var artifactInfo = new FileInfo(artifactFile);
            var assetUpload = new ReleaseAssetUpload()
            {
                FileName = artifactInfo.Name,
                ContentType = "application/zip",
                RawData = artifact
            };
            var asset = gitHubClient.Repository.Release.UploadAsset(release, assetUpload).Result;
            Serilog.Log.Information($"Asset {asset.Name} published at {asset.BrowserDownloadUrl}");
        });

    /// <summary>
    /// Lauch in deploy mode, updates the module on the current local site.
    /// </summary>
    Target Deploy => _ => _
        .DependsOn(DeployBinaries)
        .DependsOn(SetRelativeScripts)
        .Executes(() =>
        {
            ResetDocs();
        });

    /// <summary>
    /// Watch frontend for changes
    /// </summary>
    Target Watch => _ => _
    .DependsOn(SetLiveServer)
    .Executes(() =>
    {
        ResetDocs();
        NpmRun(s => s
            .SetProcessWorkingDirectory(WebProjectDirectory)
            .AddArguments("start")
            );
    });


    Target GenerateAppConfig => _ => _
    .OnlyWhenDynamic(() => RootDirectory.Parent.ToString().EndsWith("DesktopModules", StringComparison.OrdinalIgnoreCase))
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

        Serilog.Log.Information("Generated {0} from {1}", appConfigPath, webConfigPath);
        Serilog.Log.Information("This file is local as it could contain credentials, it should not be committed to the repository.");
    });

    /// <summary>
    /// Package the module
    /// </summary>
    Target Package => _ => _
        .DependsOn(Clean)
        .DependsOn(SetManifestVersions)
        .DependsOn(Compile)
        .DependsOn(SetRelativeScripts)
        .DependsOn(GenerateAppConfig)
        .DependsOn(Test)
        .DependsOn(UpdateTokens)
        .DependsOn(Docs)
        .Executes(() =>
        {
            var stagingDirectory = ArtifactsDirectory / "staging";
            stagingDirectory.CreateOrCleanDirectory();

            // Resources7
            var resourcesDirectory = RootDirectory / "resources";
            resourcesDirectory.CompressTo(stagingDirectory / "resources.zip", f => (f.Name != "resources.zip.manifest"));

            // Symbols
            var moduleAssemblyName = Solution.GetProject("Module").GetProperty("AssemblyName");
            var symbolFiles = RootDirectory.GlobFiles($"bin/Release/**/{moduleAssemblyName}.pdb");
            Helpers.AddFilesToZip(stagingDirectory / "symbols.zip", symbolFiles.ToList());

            // Install files
            var installFiles = RootDirectory.GlobFiles("LICENSE", "manifest.dnn", "ReleaseNotes.html");
            installFiles.ForEach(i => CopyFileToDirectory(i, stagingDirectory));

            // Libraries
            var manifest = RootDirectory.GlobFiles("*.dnn").FirstOrDefault();
            var binDirectory = RootDirectory / "bin" / Configuration;
            var assemblies = binDirectory.GlobFiles("*.dll");
            var manifestAssemblies = Helpers.GetAssembliesFromManifest(manifest);
            assemblies.ForEach(assembly =>
            {
                var assemblyFile = new FileInfo(assembly);
                var assemblyIncludedInManifest = manifestAssemblies.Any(a => a == assemblyFile.Name);

                if (assemblyIncludedInManifest)
                {
                    CopyFileToDirectory(assembly, stagingDirectory / "bin", FileExistsPolicy.Overwrite);
                }
            });

            // Install package
            string fileName = new DirectoryInfo(RootDirectory).Name + "_";
            fileName += GitRepository != null && GitRepository.IsOnMainOrMasterBranch()
                ? GitVersion != null ? GitVersion.MajorMinorPatch : "0.1.0"
                : GitVersion != null ? GitVersion.SemVer : "0.1.0";
            fileName += "_install.zip";
            ZipFile.CreateFromDirectory(stagingDirectory, ArtifactsDirectory / fileName);
            stagingDirectory.DeleteDirectory();

            var artifact = ArtifactsDirectory / fileName;
            string hash;
            using (var md5 = MD5.Create())
            {
                using (var stream = File.OpenRead(artifact))
                {
                    var hashBytes = md5.ComputeHash(stream);
                    hash = BitConverter.ToString(hashBytes).Replace("-", "").ToLowerInvariant();
                }
            }

            var hashMd = new StringBuilder();
            hashMd.AppendLine($"## MD5 Checksums");
            hashMd.AppendLine($"| File       | Checksum |");
            hashMd.AppendLine($"|------------|----------|");
            hashMd.AppendLine($"| {fileName} | {hash}   |");
            hashMd.AppendLine();
            File.WriteAllText(ArtifactsDirectory / "checksums.md", hashMd.ToString());

            // Open folder
            if (IsWin)
            {
                CopyFileToDirectory(ArtifactsDirectory / fileName, InstallDirectory, FileExistsPolicy.Overwrite);

                // Uncomment next line if you would like a package task to auto-open the package in explorer.
                // Process.Start("explorer.exe", ArtifactsDirectory);
            }

            ResetDocs();

            if (FirstBuild)
            {
                Git($"add .", RootDirectory, logger: (outputType, message) => Serilog.Log.Information($"{message}"));
                Git($"commit -m \"Commit generated files from first build.\"");
            }

            Serilog.Log.Information("Packaging succeeded!");
        });

    Target Swagger => _ => _
        .Before(DocFx)
        .DependsOn(Compile)
        .Executes(() =>
        {
            var swaggerDir = DocsDirectory / "rest";
            swaggerDir.CreateOrCleanDirectory();
            var swaggerFile = DocsDirectory / "rest" / "rest.json";
            var assembly = RootDirectory / "bin" / Configuration / $"{moduleName}.dll";
            var version = GitVersion != null ? GitVersion.AssemblySemVer : "0.1.0";
            var title = "$ext_companyname$ $ext_modulefriendlyname$";
            WebApiToOpenApiReflector($@"{assembly} --title {title} --info-version {version} --default-url-template {{controller}}/{{action}} --output {swaggerFile}");

            NSwagTasks.NSwagOpenApiToTypeScriptClient(c => c
                .SetInput(swaggerFile)
                .SetOutput(ClientServicesDirectory / "services.ts")
                .SetNSwagRuntime("Net80")
                .SetProcessArgumentConfigurator(c => c
                    .Add("/Template:Fetch")
                    .Add("/GenerateClientClasses:True")
                    .Add("/ClientBaseClass:ClientBase")
                    .Add("/ConfigurationClass:ConfigureRequest")
                    .Add("/UseTransformOptionsMethod:True")
                    .Add("/MarkOptionalProperties:True")
                    .Add($"/ExtensionCode:{ClientServicesDirectory / "client-base.ts"}")
                    .Add("/UseGetBaseUrlMethod:True")
                    .Add("/UseAbortSignal:True")));
        });

    Target CleanDocsFolder => _ => _
        .Before(Swagger)
        .Before(DocFx)
        .Executes(() =>
        {
            DocsDirectory.CreateOrCleanDirectory();
        });

    Target DocFx => _ => _
        .DependsOn(Compile)
        .DependsOn(TsDoc)
        .DependsOn(ComponentsDocs)
        .DependsOn(Swagger)
        .Executes(() =>
        {
            DocFXTasks.DocFXMetadata(s => s
                .SetProcessWorkingDirectory(DocFxProjectDirectory));

            var sb = new StringBuilder();
            sb.AppendLine("# Backend API documentation")
                .AppendLine()
                .AppendLine("This section documents the APIs available in the backend (c#) code.")
                .AppendLine()
                .AppendLine("Please expand the namespaces to navigate through the APIs.");
            (DocFxProjectDirectory / "api" / "index.md").WriteAllText(sb.ToString());

            NpmTasks.NpmInstall(s => s
                .SetProcessWorkingDirectory(DocFxProjectDirectory));

            NpmTasks.NpmRun(s => s
                .SetProcessWorkingDirectory(DocFxProjectDirectory)
                .SetArguments("adjust_toc"));

            DocFXTasks.DocFXBuild(s => s
                .SetOutputFolder(RootDirectory)
                .SetProcessWorkingDirectory(DocFxProjectDirectory));
        });

    Target Docs => _ => _
        .DependsOn(CleanDocsFolder)
        .DependsOn(Swagger)
        .DependsOn(ComponentsDocs)
        .DependsOn(TestsDocs)
        .DependsOn(TsDoc)
        .DependsOn(DocFx)
        .Executes(() =>
        {
            if (InvokedTargets.Contains(Docs))
            {
                NpmTasks.NpmInstall(s => s
                    .SetProcessWorkingDirectory(DocFxProjectDirectory));

                NpmTasks.NpmRun(s => s
                    .SetProcessWorkingDirectory(DocFxProjectDirectory)
                    .SetArguments("watch_docfx"));
            }
        });

    Target DeployGeneratedFiles => _ => _
        .DependsOn(Docs)
        .DependsOn(Test)
        .OnlyWhenDynamic(() => GitRepository.IsOnMainOrMasterBranch() || GitRepository.IsOnDevelopBranch() || GitRepository.IsOnReleaseBranch())
        .Executes(() =>
        {
            var gitHubClient = new GitHubClient(new ProductHeaderValue("Nuke"));
            var authToken = new Credentials(GitHubToken);
            gitHubClient.Credentials = authToken;

            var repo = gitHubClient.Repository.Get(GitRepository.GetGitHubOwner(), GitRepository.GetGitHubName()).Result;
            if (!repo.Fork)
            {
                Git($"config --global user.name '{GitRepository.GetGitHubOwner()}'");
                Git($"config --global user.email '{Helpers.GetManifestOwnerEmail(RootDirectory.GlobFiles("*.dnn").FirstOrDefault())}'");
                Git($"remote set-url origin https://{GitRepository.GetGitHubOwner()}:{GitHubToken}@github.com/{GitRepository.GetGitHubOwner()}/{GitRepository.GetGitHubName()}.git");
                Git("status");
                Git("add docs -f");
                Git("add IntegrationTests/history -f");
                Git("add UnitTests/history -f");
                Git("add .github/badges -f");
                Git("status");
                Git($"commit --allow-empty -m \"Commit latest generated files\""); // We allow an empty commit in case the last change did not affect the site.
                Git("status");
                Git("fetch origin");
                Git($"pull origin {GitRepository.Branch}");
                Git($"push --set-upstream origin {GitRepository.Branch}");
            }
        });

    Target TestsDocs => _ => _
        .DependsOn(Test)
        .DependsOn(CleanDocsFolder)
        .Executes(() => {
            var integrationTestsDocsDirectory = DocsDirectory / "integrationTests";
            integrationTestsDocsDirectory.CreateOrCleanDirectory();
            CopyDirectoryRecursively(
                IntegrationTestsResultsDirectory,
                integrationTestsDocsDirectory,
                DirectoryExistsPolicy.Merge,
                FileExistsPolicy.Overwrite);

            var unitTestsDocsDirectory = DocsDirectory / "unitTests";
            unitTestsDocsDirectory.CreateOrCleanDirectory();
            CopyDirectoryRecursively(
                UnitTestsResultsDirectory,
                unitTestsDocsDirectory,
                DirectoryExistsPolicy.Merge,
                FileExistsPolicy.Overwrite);
        });

    Target TsDoc => _ => _
        .Executes(() =>
        {
            var tempDirectory = WebProjectDirectory / "temp";
            var tempMdDirectory = WebProjectDirectory / "tempmd";
            var clientDocDirectory = DocFxProjectDirectory / "client";

            tempDirectory.CreateOrCleanDirectory();
            tempMdDirectory.CreateOrCleanDirectory();
            clientDocDirectory.CreateOrCleanDirectory();

            NpmRun(s => s
                .SetProcessWorkingDirectory(WebProjectDirectory)
                .SetArguments("tsdoc"));

            CopyDirectoryRecursively(
                tempMdDirectory,
                clientDocDirectory,
                DirectoryExistsPolicy.Merge,
                FileExistsPolicy.Overwrite);

            // Create a table of content
            var toc = new StringBuilder();

            var files = clientDocDirectory.GlobFiles("**/*.md");
            files = files
                .OrderBy(f => f.Name.Split('.').Count())
                .ThenBy(f => f.Name)
                .ToList();

            files.ForEach(file =>
            {
                var fileInfo = new FileInfo(file);
                if (fileInfo.Name == "index.md" || fileInfo.Name.Split('.').Count() > 3)
                {
                    return;
                }

                var fileLines = file.ReadAllLines();
                var cleanName = fileLines[4];
                cleanName = string.Join(' ', cleanName.Split(' ').Skip(1).ToArray());
                toc.AppendLine($"{new String('#', fileInfo.Name.Split('.').Count() - 1)} [{cleanName}](./{fileInfo.Name})");
            });
            (clientDocDirectory / "toc.md").WriteAllText(toc.ToString());

            tempDirectory.DeleteDirectory();
            tempMdDirectory.DeleteDirectory();
        });

    Target ComponentsDocs => _ => _
        .DependsOn(BuildFrontEnd)
        .Executes(() =>
        {
            var componentsDocsDirectory = DocFxProjectDirectory / "components";
            componentsDocsDirectory.CreateOrCleanDirectory();
            var componentsDirectory = WebProjectDirectory / "src" / "components";
            var docFiles = componentsDirectory.GlobFiles("**/*.md");
            var toc = new StringBuilder();
            docFiles.ForEach(f => {
                var fileInfo = new FileInfo(f);
                if (fileInfo.Directory.Name == "usage")
                {
                    return;
                }
                var newFileName = fileInfo.Directory.Name + ".md";
                CopyFile(f, componentsDocsDirectory / newFileName, FileExistsPolicy.Overwrite, true);
                toc.AppendLine($"# [{fileInfo.Directory.Name}]({newFileName})");
            });
            toc.AppendLine();
            (componentsDocsDirectory / "toc.md").WriteAllText(toc.ToString());

            var index = WebProjectDirectory.GlobFiles("readme.md").FirstOrDefault();
            CopyFileToDirectory(index, componentsDocsDirectory, FileExistsPolicy.Overwrite, true);
            RenameFile(componentsDocsDirectory / "readme.md", "index.md", FileExistsPolicy.Overwrite);
        });

    Target EnsureBootstrapingScriptsAreExecutable => _ => _
    .OnlyWhenDynamic(() => !IsServerBuild)
    .Executes(() =>
    {
        if (GitRepository is null)
        {
            FirstBuild = true;
            Git($"init -b develop", RootDirectory);
            Git($"add .", RootDirectory, logger: (outputType, message) => Serilog.Log.Information($"{message}"));
            Git($"commit -m \"Initial Commit\"", RootDirectory);

            var files = RootDirectory.GlobFiles("build.sh", "build.cmd");
            foreach (var file in files)
            {
                var fileContent = file.ReadAllText();
                fileContent = fileContent.Replace("\r\n", "\n");
                file.WriteAllText(fileContent);
                Git($"update-index --chmod=+x {file.Name}", RootDirectory);
            }

            Git($"add .", RootDirectory, logger: (outputType, message) => Serilog.Log.Information($"{message}"));
            Git($"commit -m \"Made build bootstrapping scripts executable", RootDirectory);
        }
    });

    private void ResetDocs()
    {
        if (GitRepository != null && IsLocalBuild)
        {
            try
            {
                Git("checkout -q HEAD -- docs", workingDirectory: RootDirectory);
            }
            catch (Exception)
            {
                // Ignored on purpose, if this fails, we just don't have a docs folder so everything is ok.
            }
        }
    }
}
