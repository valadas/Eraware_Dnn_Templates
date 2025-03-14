using BuildHelpers;
using Nuke.Common;
using Nuke.Common.CI.GitHubActions;
using Nuke.Common.Execution;
using Nuke.Common.Git;
using Nuke.Common.IO;
using Nuke.Common.ProjectModel;
using Nuke.Common.Tooling;
using Nuke.Common.Tools.Coverlet;
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
using System.Runtime.Versioning;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using static Nuke.Common.EnvironmentInfo;
using static Nuke.Common.IO.PathConstruction;
using static Nuke.Common.IO.TextTasks;
using static Nuke.Common.Tools.DotNet.DotNetTasks;
using static Nuke.Common.Tools.Git.GitTasks;
using static Nuke.Common.Tools.MSBuild.MSBuildTasks;
using static Nuke.Common.Tools.Npm.NpmTasks;
using static Nuke.Common.Tools.ReportGenerator.ReportGeneratorTasks;

[GitHubActions(
    "Build",
    GitHubActionsImage.Ubuntu2204,
    ImportSecrets = new[] { nameof(GitHubToken) },
    OnPullRequestBranches = new[] { "develop", "main", "master", "release/*" },
    OnPushBranches = new[] { "main", "master", "develop", "release/*" },
    InvokedTargets = new[] { nameof(Package), nameof(DeployGeneratedFiles), nameof(Release) },
    FetchDepth = 0,
    CacheKeyFiles = new string[] { }
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
    AbsolutePath ResourcesDirectory => RootDirectory / "resources";
    AbsolutePath ScriptsDirectory => ResourcesDirectory / "scripts";
    AbsolutePath ScriptsModulesDirectory => ScriptsDirectory / "$ext_scopeprefixkebab$";
    AbsolutePath DeployDirectory => RootDirectory.Parent / "Admin" / "Dnn.PersonaBar" / "Modules" / "$ext_modulefoldername$";

    private const string devViewsPath = "http://localhost:3333/build/";
    private const string prodViewsPath = "/DesktopModules/Admin/Dnn.PersonaBar/Modules/$ext_modulefoldername$/scripts/$ext_scopeprefixkebab$/";
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
                .AddProcessAdditionalArguments("-title:UnitTests"));

            Helpers.CleanCodeCoverageHistoryFiles(RootDirectory / "UnitTests" / "history");

            var testBadges = UnitTestsResultsDirectory.GlobFiles("badge_branchcoverage.svg", "badge_linecoverage.svg");
            testBadges.ForEach(f => f.CopyToDirectory(UnitTestBadgesDirectory, ExistsPolicy.FileOverwrite, createDirectories: true));

            if (IsWin && (InvokedTargets.Contains(UnitTests) || InvokedTargets.Contains(Test)))
            {
                Process.Start(@"cmd.exe ", @"/c " + (UnitTestsResultsDirectory / "index.html"));
            }
        });

    Target Test => _ => _
        .DependsOn(UnitTests)
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
        .Executes(() =>
        {
            var views = RootDirectory.GlobFiles("resources/**/*.html");
            foreach (var view in views)
            {
                var content = view.ReadAllText();
                content = content.Replace(devViewsPath, prodViewsPath, StringComparison.OrdinalIgnoreCase);
                view.WriteAllText(content, System.Text.Encoding.UTF8);
                Serilog.Log.Information("Set scripts path to {0} in {1}", prodViewsPath, view);
            }
        });

    Target SetLiveServer => _ => _
        .After(Deploy)
        .Executes(() =>
        {
            var views = RootDirectory.GlobFiles("resources/*.html");
            foreach (var view in views)
            {
                var content = view.ReadAllText();
                content = content.Replace(prodViewsPath, devViewsPath, StringComparison.OrdinalIgnoreCase);
                view.WriteAllText(content, Encoding.UTF8);
                Serilog.Log.Information("Set scripts path to {0} in {1}", devViewsPath, view);
            }
        });

    Target DeployFrontEnd => _ => _
        .DependsOn(BuildFrontEnd)
        .DependsOn(CopyScripts)
        .Executes(() =>
        {
            var resourcesDirectory = RootDirectory / "resources";
            DeployDirectory.CreateOrCleanDirectory();
            resourcesDirectory.CopyToDirectory(DeployDirectory, ExistsPolicy.MergeAndOverwrite);
        });

    Target CopyScripts => _ => _
        .DependsOn(BuildFrontEnd)
        .Executes(() =>
        {
            ScriptsModulesDirectory.CreateOrCleanDirectory();
            (RootDirectory / "module.web" / "dist" / "nvi-davidmodule").CopyToDirectory(ScriptsModulesDirectory, ExistsPolicy.MergeAndOverwrite);
            var collectionDirectory = RootDirectory / "module.web" / "dist" / "dnn";
            collectionDirectory
                .GlobFiles("*.*")
                .ForEach(f => f.CopyToDirectory(ScriptsDirectory, ExistsPolicy.FileOverwrite, createDirectories: true));
        });

    Target InstallNpmPackages => _ => _
        .Executes(() =>
        {
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
        .DependsOn(DeployFrontEnd)
        .Executes(() =>
        {
            ResetDocs();
        });

    /// <summary>
    /// Watch frontend for changes
    /// </summary>
    Target Watch => _ => _
    .DependsOn(SetLiveServer)
    .DependsOn(Deploy)
    .Executes(() =>
    {
        ResetDocs();

        var sourceDirectory = ResourcesDirectory;
        var destinationDirectory = DeployDirectory;

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
        .DependsOn(CopyScripts)
        .Produces(ArtifactsDirectory / "*.zip")
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
            installFiles.ForEach(i => i.CopyToDirectory(stagingDirectory, ExistsPolicy.FileOverwrite));

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
                    assembly.CopyToDirectory(stagingDirectory / "bin", ExistsPolicy.FileOverwrite, createDirectories: true);
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
                (ArtifactsDirectory / fileName).CopyToDirectory(InstallDirectory, ExistsPolicy.FileOverwrite);

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
                .AddProcessAdditionalArguments(
                    "/Template:Fetch",
                    "/GenerateClientClasses:True",
                    "/ClientBaseClass:ClientBase",
                    "/ConfigurationClass:ConfigureRequest",
                    "/UseTransformOptionsMethod:True",
                    "/MarkOptionalProperties:True",
                    $"/ExtensionCode:{ClientServicesDirectory / "client-base.ts"}",
                    "/UseGetBaseUrlMethod:True",
                    "/UseAbortSignal:True"));
        });

    Target DeployGeneratedFiles => _ => _
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

    private static void OnChanged(FileSystemEventArgs e, string sourceDir, string destDir)
    {
        try
        {
            string sourcePath = e.FullPath;
            string relativePath = Path.GetRelativePath(sourceDir, sourcePath);
            string destinationPath = Path.Combine(destDir, relativePath);

            if (Directory.Exists(sourcePath))
            {
                Directory.CreateDirectory(destinationPath);
            }
            else
            {
                Directory.CreateDirectory(Path.GetDirectoryName(destinationPath));
                File.Copy(sourcePath, destinationPath, true);
                Serilog.Log.Information($"Copied {sourcePath} to {destinationPath}");
            }
        }
        catch (Exception ex)
        {
            Serilog.Log.Error($"Error copying file: {ex.Message}");
        }
    }
}
