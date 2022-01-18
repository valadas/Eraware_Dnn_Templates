using Newtonsoft.Json;
using Nuke.Common;
using System;
using System.CodeDom;
using System.CodeDom.Compiler;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml;
using static Nuke.Common.IO.FileSystemTasks;
using static Nuke.Common.IO.PathConstruction;
using static Nuke.Common.IO.XmlTasks;
using static Nuke.Common.IO.TextTasks;
using Nuke.Common.IO;

namespace BuildHelpers
{
    public class Helpers : NukeBuild
    {
        public static void CopyFileToDirectoryIfChanged(string source, string target)
        {
            var sourceFile = new FileInfo(source);
            var destinationFile = new FileInfo(Path.Combine(target, sourceFile.Name));
            var destinationExists = destinationFile.Exists;
            var sameSize = destinationExists ? sourceFile.Length == destinationFile.Length : false;
            var sameContent = true;

            Serilog.Log.Debug("{0} is {1} Bytes", sourceFile.FullName, sourceFile.Length);
            if (destinationExists)
            {
                Serilog.Log.Debug("{0} exists and is {1} Bytes", destinationFile.FullName, destinationFile.Length);
            }

            if (destinationExists && sameSize)
            {
                sameContent = FilesAreEqual(sourceFile, destinationFile);
                Serilog.Log.Debug(sameContent ? "Both files have the same content" : "The files have different contents");
            }

            if (!destinationExists || !sameSize || !sameContent)
            {
                CopyFileToDirectory(source, target, Nuke.Common.IO.FileExistsPolicy.OverwriteIfNewer);
                Serilog.Log.Information("Copied {0} to {1}", sourceFile.FullName, destinationFile.FullName);
            }
            else
            {
                Serilog.Log.Information("Skipped {0} since it is unchanged.", sourceFile.FullName);
            }
        }

        // Fast but accurate way to check if two files are difference (safer than write time for when rebuilding without changes).
        private static bool FilesAreEqual(FileInfo first, FileInfo second)
        {
            const int BYTES_TO_READ = sizeof(Int64);
            if (first.Length != second.Length)
            {
                return false;
            }

            if (string.Equals(first.FullName, second.FullName, StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }

            int iterations = (int)Math.Ceiling((double)first.Length / BYTES_TO_READ);

            using (FileStream fs1 = first.OpenRead())
            {
                using (FileStream fs2 = second.OpenRead())
                {
                    byte[] one = new byte[BYTES_TO_READ];
                    byte[] two = new byte[BYTES_TO_READ];

                    for (int i = 0; i < iterations; i++)
                    {
                        fs1.Read(one, 0, BYTES_TO_READ);
                        fs2.Read(two, 0, BYTES_TO_READ);

                        if (BitConverter.ToInt64(one, 0) != BitConverter.ToInt64(two, 0))
                        {
                            return false;
                        }
                    }
                }
            }
            return true;
        }

        public static void AddFilesToZip(string zipPath, string[] files)
        {
            if (files == null || files.Length == 0)
            {
                return;
            }

            using (var zipArchive = ZipFile.Open(zipPath, ZipArchiveMode.Update))
            {
                foreach (var file in files)
                {
                    var fileInfo = new FileInfo(file);
                    zipArchive.CreateEntryFromFile(fileInfo.FullName, fileInfo.Name);
                }
            }
        }

        public static string Dump(object obj)
        {
            return obj.ToString() + System.Environment.NewLine + JsonConvert.SerializeObject(obj, Newtonsoft.Json.Formatting.Indented);
        }

        public static IEnumerable<string> GetAssembliesFromManifest(string manifestFilePath)
        {
            var doc = new XmlDocument();
            doc.Load(manifestFilePath);
            var nodes = doc.SelectNodes("dotnetnuke/packages/package/components/component[@type='Assembly']/assemblies/assembly/name");
            List<string> assemblies = new List<string>();
            foreach (XmlNode node in nodes)
            {
                assemblies.Add(node.InnerText);
            }

            return assemblies;
        }

        public static void CleanCodeCoverageHistoryFiles(string directory)
        {
            var files = GlobFiles(directory, "*.xml");
            if (files == null || files.Count() < 2)
            {
                return;
            }

            var fileInfos = new List<(
                DateTime FileDate,
                string FileName,
                int CoveredLines,
                int CoverableLines,
                int TotalLines,
                int CoveredBranches,
                int TotalBranches)>();
            foreach (var file in files)
            {
                var d = XmlPeekSingle(file, "/coverage/@date");
                var date = new DateTime(
                    int.Parse(d.Substring(0, 4)),
                    int.Parse(d.Substring(5, 2)),
                    int.Parse(d.Substring(8, 2)),
                    int.Parse(d.Substring(11, 2)),
                    int.Parse(d.Substring(14, 2)),
                    int.Parse(d.Substring(17, 2)));
                var coveredLines = XmlPeek(file, "/coverage/assembly/class/@coveredlines").Sum(c => int.Parse(c));
                var coverableLines = XmlPeek(file, "coverage/assembly/class/@coverablelines").Sum(c => int.Parse(c));
                var totalLines = XmlPeek(file, "coverage/assembly/class/@totallines").Sum(c => int.Parse(c));
                var coveredBranches = XmlPeek(file, "coverage/assembly/class/@coveredbranches").Sum(c => int.Parse(c));
                var totalBranches = XmlPeek(file, "coverage/assembly/class/@totalbranches").Sum(c => int.Parse(c));
                fileInfos.Add((date, file, coveredLines, coverableLines, totalLines, coveredBranches, totalBranches));
            }

            var orderedFiles = fileInfos.OrderBy(f => f.FileDate).ToArray();
            for (int i = 0; i < orderedFiles.Count() - 1; i++)
            {
                var fileA = orderedFiles[i];
                var fileB = orderedFiles[i + 1];
                if (
                    fileA.CoveredLines == fileB.CoveredLines &&
                    fileA.CoverableLines == fileB.CoverableLines &&
                    fileA.TotalLines == fileB.TotalLines &&
                    fileA.CoveredBranches == fileB.CoveredBranches &&
                    fileA.TotalBranches == fileB.TotalBranches)
                {
                    DeleteFile(fileB.FileName);
                }
            }
        }

        public static void GenerateLocalizationFiles(string rootNamespace)
        {
            var localizationFiles = GlobFiles(RootDirectory / "resources" / "App_LocalResources", "*.resx")
                .Where(l => Regex.Matches(l, @"\.").Count == 1).ToList();
            var generatedComment = GetGeneratedComment();
            var vm = new StringBuilder();
            vm.AppendLine(GetGeneratedComment());
            vm.Append(GenerateLocalizationViewModel(rootNamespace, localizationFiles));

            var svc = new StringBuilder();
            svc.AppendLine(GetGeneratedComment());
            svc.AppendLine(GenerateLocalizationService(rootNamespace, localizationFiles));

            File.WriteAllText(RootDirectory / "ViewModels" / "LocalizationViewModel.cs", vm.ToString());
            File.WriteAllText(RootDirectory / "Services" / "LocalizationService.cs", svc.ToString());
        }

        public static string GetManifestOwnerName(string manifestPath)
        {
            var doc = new XmlDocument();
            doc.Load(manifestPath);
            var node = doc.SelectSingleNode("dotnetnuke/packages/package/owner/name");
            return node.InnerText;
        }

        public static string GetManifestOwnerEmail(string manifestPath)
        {
            var doc = new XmlDocument();
            doc.Load(manifestPath);
            var node = doc.SelectSingleNode("dotnetnuke/packages/package/owner/email");
            return node.InnerText;
        }

        private static string GenerateLocalizationService(string rootNamespace, List<string> localizationFiles)
        {
            var moduleFolderName = new DirectoryInfo(RootDirectory).Name;
            var sb = new StringBuilder();
            sb
                .AppendLine($"namespace {rootNamespace}.Services")
                 .AppendLine("{")
                 .AppendLine("    using DotNetNuke.Common.Utilities;")
                .AppendLine($"    using DotNetNuke.Services.Localization;")
                .AppendLine($"    using {rootNamespace}.ViewModels;")
                .AppendLine($"    using System.Diagnostics.CodeAnalysis;")
                .AppendLine($"    using System.Web.Hosting;")
                .AppendLine($"    using System.Threading;")
                .AppendLine($"    using static {rootNamespace}.ViewModels.LocalizationViewModel;")
                .AppendLine()
                .AppendLine($"    /// <summary>")
                .AppendLine($"    /// Provides strongly typed localization services for this module.")
                .AppendLine($"    /// </summary>")
                .AppendLine($"    [ExcludeFromCodeCoverage]")
                .AppendLine($"    public class LocalizationService : ILocalizationService")
                 .AppendLine("    {")
                .AppendLine($"        private readonly ILocalizationProvider localizationProvider;")
                .AppendLine($"        private readonly LocalizationViewModel viewModel;")
                .AppendLine($"        private readonly string cacheKey;")
                .AppendLine($"        private string resourceFileRoot;")
                .AppendLine()
                .AppendLine($"        private string ResourceFileRoot")
                 .AppendLine("        {")
                .AppendLine($"            get")
                 .AppendLine("            {")
                .AppendLine($"                if (string.IsNullOrWhiteSpace(this.resourceFileRoot))")
                 .AppendLine("                {")
                .AppendLine($"                    this.resourceFileRoot = HostingEnvironment.MapPath(")
                .AppendLine($"                        \"~/DesktopModules/{moduleFolderName}/resources/App_LocalResources/\");")
                 .AppendLine("                }")
                 .AppendLine()
                 .AppendLine("                return this.resourceFileRoot;")
                 .AppendLine("            }")
                 .AppendLine("        }")
                 .AppendLine()
                .AppendLine($"        /// <summary>")
                .AppendLine($"        /// Initializes a new instance of the <see cref=\"LocalizationService\"/> class.")
                .AppendLine($"        /// </summary>")
                .AppendLine($"        public LocalizationService()")
                 .AppendLine("        {")
                .AppendLine($"            this.localizationProvider = new LocalizationProvider();")
                .AppendLine($"            this.cacheKey = \"{rootNamespace}\" + \"_Localization_\" + Thread.CurrentThread.CurrentCulture;")
                .AppendLine($"            this.viewModel = new LocalizationViewModel();")
                .AppendLine($"            var viewModel = DataCache.GetCache<LocalizationViewModel>(this.cacheKey);")
                 .AppendLine("            if (viewModel is null)")
                 .AppendLine("            {")
                 .AppendLine("                viewModel = this.HydrateViewModel();")
                 .AppendLine("                DataCache.SetCache(this.cacheKey, viewModel);")
                 .AppendLine("            }")
                 .AppendLine("            this.viewModel = viewModel;")
                 .AppendLine("        }")
                 .AppendLine()
                 .AppendLine("        /// <summary>")
                 .AppendLine("        /// A viewmodel that strongly types all resource keys.")
                 .AppendLine("        /// </summary>")
                 .AppendLine("        public LocalizationViewModel ViewModel")
                 .AppendLine("        {")
                 .AppendLine("            get")
                 .AppendLine("            {")
                 .AppendLine("                return this.viewModel;")
                 .AppendLine("            }")
                 .AppendLine("        }")
                 .AppendLine()
                 .AppendLine("        private string GetString(string key, string file)")
                 .AppendLine("        {")
                 .AppendLine("            return this.localizationProvider.GetString(key, this.ResourceFileRoot + file);")
                 .AppendLine("        }")
                 .AppendLine()
                 .AppendLine("        private LocalizationViewModel HydrateViewModel()")
                 .AppendLine("        {")
                 .AppendLine(GetLocalizationFilesForViewModel(localizationFiles))
                 .AppendLine("        }")
                 .AppendLine("    }")
                 .AppendLine("}");
            return sb.ToString();
        }

        private static string GetLocalizationFilesForViewModel(List<string> localizationFiles)
        {
            var sb = new StringBuilder();
            foreach (var file in localizationFiles)
            {
                var fileInfo = new FileInfo(file);
                var fileName = fileInfo.Name.Split('.')[0];
                sb
                   .AppendLine($"            var {fileName.ToLower()} = new {fileName}Info")
                    .AppendLine("            {")
                    .AppendLine(GetLocalizationKeysForViewModel(file))
                    .AppendLine("            };")
                   .AppendLine($"            viewModel.{fileName} = {fileName.ToLower()};");
            }
            sb.AppendLine("            return viewModel;");
            return sb.ToString();
        }

        private static string GetLocalizationKeysForViewModel(string file)
        {
            var fileInfo = new FileInfo(file);
            var fileName = fileInfo.Name;
            var xmlDoc = new XmlDocument();
            xmlDoc.Load(file);
            var items = xmlDoc.SelectNodes("/root/data");
            var sb = new StringBuilder();
            foreach (XmlNode item in items)
            {
                var name = item.Attributes["name"].InnerText.Replace(".Text", "");
                var valueNode = item.SelectSingleNode("value");
                var value = valueNode.InnerText;
                sb
                    .AppendLine($"                {name} = this.GetString(\"{name}\", \"{fileName}\"),");
            }
            return sb.ToString();
        }

        private static string GenerateLocalizationViewModel(string rootNamespace, List<string> localizationFiles)
        {
            var sb = new StringBuilder();
            sb
                .AppendLine($"namespace {rootNamespace}.ViewModels")
                .AppendLine("{")
                .AppendLine("    using System.Diagnostics.CodeAnalysis;")
                .AppendLine()
                .AppendLine("    /// <summary>")
                .AppendLine("    /// A viewmodel that exposes all resource keys in strong types.")
                .AppendLine("    /// </summary>")
                .AppendLine("    [ExcludeFromCodeCoverage]")
                .AppendLine("    public class LocalizationViewModel")
                .AppendLine("    {")
                .AppendLine(GetLocalizationViewModelClass(localizationFiles))
                .AppendLine("    }")
                .AppendLine("}");
            return sb.ToString();
        }

        private static string GetLocalizationViewModelClass(List<string> localizationFiles)
        {
            var sb = new StringBuilder();
            for (int i = 0; i < localizationFiles.Count(); i++)
            {
                var fileName = new FileInfo(localizationFiles[i]).Name.Split('.')[0];
                sb
                    .AppendLine("        /// <summary>")
                   .AppendLine($"        /// Localized strings present the {fileName} resources.")
                    .AppendLine("        /// </summary>")
                   .AppendLine($"        public {fileName}Info {fileName}" + " { get; set; }");
                if (i < localizationFiles.Count() - 1) sb.AppendLine();
            }
            sb.AppendLine(GetLocalizationFilePropertiesClasses(localizationFiles));
            return sb.ToString();
        }

        private static string GetLocalizationFilePropertiesClasses(List<string> localizationFiles)
        {
            var sb = new StringBuilder();
            for (int i = 0; i < localizationFiles.Count(); i++)
            {
                var file = localizationFiles[i];
                var fileInfo = new FileInfo(file);
                var fileName = fileInfo.Name.Split('.')[0];

                sb
                    .AppendLine("        /// <summary>")
                   .AppendLine($"        /// Localized strings for the {fileName} resources.")
                    .AppendLine("        /// </summary>")
                   .AppendLine($"        public class {fileName}Info")
                    .AppendLine("        {")
                    .AppendLine(GetLocalizationFileProperties(file))
                    .AppendLine("        }");
                if (i < localizationFiles.Count() - 1) sb.AppendLine();

            }
            return sb.ToString();
        }

        private static string GetLocalizationFileProperties(string file)
        {
            var xmlDoc = new XmlDocument();
            xmlDoc.Load(file);
            var items = xmlDoc.SelectNodes("/root/data");
            var sb = new StringBuilder();
            foreach (XmlNode item in items)
            {
                var name = item.Attributes["name"].InnerText.Replace(".Text", "");
                var valueNode = item.SelectSingleNode("value");
                var value = valueNode.InnerText;
                sb
                    .AppendLine($"            /// <summary>Gets or sets the {name} localized text.</summary>")
                    .AppendLine($"            /// <example>{value}</example>")
                    .AppendLine($"            public string {name}" + " { get; set; }")
                    .AppendLine();
            }
            return sb.ToString();
        }

        private static string GetGeneratedComment()
        {
            var sb = new StringBuilder();
            sb
                .AppendLine("//------------------------------------------------------------------------------")
                .AppendLine("// <auto-generated>")
                .AppendLine("//     This code was generated by a tool.")
                .AppendLine()
                .AppendLine("//     Changes to this file may cause incorrect behavior and will be lost if")
                .AppendLine("//     the code is regenerated.")
                .AppendLine("// </auto-generated>")
                .AppendLine("//------------------------------------------------------------------------------");
            return sb.ToString();
        }
    }
}
