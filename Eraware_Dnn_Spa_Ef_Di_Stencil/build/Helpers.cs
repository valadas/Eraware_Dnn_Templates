using Newtonsoft.Json;
using Nuke.Common;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Text;
using System.Xml;
using static Nuke.Common.IO.FileSystemTasks;
using static Nuke.Common.IO.PathConstruction;
using static Nuke.Common.IO.XmlTasks;

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

            Logger.Trace("{0} is {1} Bytes", sourceFile.FullName, sourceFile.Length);
            if (destinationExists)
            {
                Logger.Trace("{0} exists and is {1} Bytes", destinationFile.FullName, destinationFile.Length);
            }

            if (destinationExists && sameSize)
            {
                sameContent = FilesAreEqual(sourceFile, destinationFile);
                Logger.Trace(sameContent ? "Both files have the same content" : "The files have different contents");
            }

            if (!destinationExists || !sameSize || !sameContent)
            {
                CopyFileToDirectory(source, target, Nuke.Common.IO.FileExistsPolicy.OverwriteIfNewer);
                Logger.Success("Copied {0} to {1}", sourceFile.FullName, destinationFile.FullName);
                Logger.Trace("\n");
            }
            else
            {
                Logger.Info("Skipped {0} since it is unchanged.", sourceFile.FullName);
                Logger.Trace("\n");
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
    }
}
