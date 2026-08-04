using EnvDTE;
using EnvDTE100;
using EnvDTE80;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.TemplateWizard;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Forms;
using System.Xml;

namespace Eraware_Dnn_Templates
{
    /// <summary>
    /// This method is called before opening any item that has the OpenInEditor attribute
    /// </summary>
    internal class WizardImplementation : IWizard
    {
        private const string VsTemplateNamespace = "http://schemas.microsoft.com/developer/vstemplate/2005";

        private bool isValid = false;
        private DTE2 dte;

        // Directory of the root .vstemplate being instantiated (from RunStarted customParams).
        // Used to locate the extracted template source files that SDK-style (.esproj)
        // projects fail to copy on their own.
        private string templateDirectory;

        // Snapshot of the replacement tokens (including their $ext_...$ variants) used to
        // reproduce Visual Studio's ReplaceParameters behavior when the wizard copies files.
        private Dictionary<string, string> replacements;

        public void BeforeOpeningFile(ProjectItem projectItem)
        {
        }

        public void ProjectFinishedGenerating(Project project)
        {
        }

        public void ProjectItemFinishedGenerating(ProjectItem projectItem)
        {
        }

        public void RunFinished()
        {
            ThreadHelper.ThrowIfNotOnUIThread();

            var projects = dte.Solution.Projects;
            foreach (Project project in projects)
            {
                try
                {
                    if (project.FullName.Contains("build"))
                    {
                        var itemsToRemove = new List<ProjectItem>();
                        foreach (ProjectItem item in project.ProjectItems)
                        {
                            if (item.Name.Contains("docs"))
                            {
                                itemsToRemove.Add(item);
                            }
                        }
                        
                        foreach (var item in itemsToRemove)
                        {
                            try
                            {
                                item.Remove();
                            }
                            catch (Exception ex)
                            {
                                Debug.WriteLine($"Could not remove item {item.Name}: {ex.Message}");
                            }
                        }
                    }

                    if (project.FullName.Contains("module.web"))
                    {
                        // SDK-style (.esproj) projects do not honor the classic
                        // <ProjectItem> file copy during template instantiation, so
                        // Visual Studio only creates the empty project file. Populate the
                        // project by copying the template's items ourselves.
                        this.PopulateSdkProject(project);

                        // Fix InvalidVariant exception by safely handling ConfigurationManager
                        try
                        {
                            var configManager = project.ConfigurationManager;
                            if (configManager != null)
                            {
                                // Get configuration names safely
                                var configurationNames = new List<string>();
                                
                                try
                                {
                                    var configRowNames = configManager.ConfigurationRowNames;
                                    if (configRowNames != null && configRowNames is Array configArray)
                                    {
                                        foreach (var configName in configArray)
                                        {
                                            if (configName != null)
                                            {
                                                configurationNames.Add(configName.ToString());
                                            }
                                        }
                                    }
                                }
                                catch (COMException comEx)
                                {
                                    Debug.WriteLine($"COM exception when accessing configuration names: {comEx.Message}");
                                    // Skip configuration deletion for this project - just return from this block
                                }

                                // Delete configurations safely (only if no COM exception occurred)
                                if (configurationNames.Count > 0)
                                {
                                    foreach (var configName in configurationNames)
                                    {
                                        try
                                        {
                                            configManager.DeleteConfigurationRow(configName);
                                        }
                                        catch (COMException comEx)
                                        {
                                            Debug.WriteLine($"Could not delete configuration {configName}: {comEx.Message}");
                                        }
                                        catch (Exception ex)
                                        {
                                            Debug.WriteLine($"Could not delete configuration {configName}: {ex.Message}");
                                        }
                                    }
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            Debug.WriteLine($"Could not handle configurations for project {project.Name}: {ex.Message}");
                        }
                    }

                    if (project.FullName.Contains("Module\\Module"))
                    {
                        // The main Module project is now SDK-style, so Visual Studio only
                        // writes the project file during instantiation and never copies the
                        // classic <ProjectItem> entries (source folders, .nuke, .github, ...)
                        // to disk. Populate the project from the extracted template first so
                        // the move-up below has real files to relocate to the module root.
                        this.PopulateSdkProject(project);

                        string moduleProjectFilePath = project.FullName;
                        dte.Solution.Remove(project);
                        var moduleProjectFile = new FileInfo(moduleProjectFilePath);
                        var originalDirectory = moduleProjectFile.Directory;

                        this.CopyAll(originalDirectory, originalDirectory.Parent);

                        dte.Solution.AddFromFile(Path.Combine(originalDirectory.Parent.FullName, moduleProjectFile.Name));
                        originalDirectory.Delete(true);
                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"Error processing project {project?.Name ?? "unknown"}: {ex.Message}");
                }
            }
        }

        private void CopyAll(DirectoryInfo source, DirectoryInfo target)
        {
            foreach (var file in source.GetFiles())
            {
                file.CopyTo(Path.Combine(target.FullName, file.Name), true);
            }

            foreach (var dir in source.GetDirectories())
            {
                var subDir = target.CreateSubdirectory(dir.Name);
                CopyAll(dir, subDir);
            }
        }

        /// <summary>
        /// Copies the template's project items into an SDK-style project (e.g. an
        /// <c>.esproj</c>). Visual Studio only writes the project file for SDK-style
        /// projects during template instantiation and relies on file globbing for the
        /// rest, but it never copies the classic <c>&lt;ProjectItem&gt;</c> entries to
        /// disk. This method reproduces that copy (including <c>TargetFileName</c> renames
        /// and <c>ReplaceParameters</c> token substitution) from the extracted template
        /// source, so the generated project is populated for both local testing and
        /// CI-produced VSIX packages.
        /// </summary>
        private void PopulateSdkProject(Project project)
        {
            Microsoft.VisualStudio.Shell.ThreadHelper.ThrowIfNotOnUIThread();

            if (string.IsNullOrEmpty(this.templateDirectory))
            {
                return;
            }

            try
            {
                var projectFile = new FileInfo(project.FullName);
                var targetDirectory = projectFile.Directory;

                // The extracted template mirrors the destination folder name (e.g. "module.web").
                var sourceDirectory = new DirectoryInfo(Path.Combine(this.templateDirectory, targetDirectory.Name));
                if (!sourceDirectory.Exists)
                {
                    Debug.WriteLine($"Template source folder not found: {sourceDirectory.FullName}");
                    return;
                }

                var templateFile = sourceDirectory.GetFiles("*.vstemplate").FirstOrDefault();
                if (templateFile == null)
                {
                    Debug.WriteLine($"No .vstemplate found in {sourceDirectory.FullName}");
                    return;
                }

                var doc = new XmlDocument();
                doc.Load(templateFile.FullName);
                var nsManager = new XmlNamespaceManager(doc.NameTable);
                nsManager.AddNamespace("vst", VsTemplateNamespace);

                var projectNode = doc.SelectSingleNode("//vst:TemplateContent/vst:Project", nsManager);
                if (projectNode == null)
                {
                    Debug.WriteLine($"No <Project> node found in {templateFile.FullName}");
                    return;
                }

                this.CopyProjectItems(projectNode, sourceDirectory.FullName, targetDirectory.FullName);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Could not populate SDK project {project.Name}: {ex.Message}");
            }
        }

        /// <summary>
        /// Recursively walks the &lt;Project&gt;/&lt;Folder&gt;/&lt;ProjectItem&gt; tree of a
        /// .vstemplate and copies each item from the extracted template source to the
        /// generated project, honoring TargetFileName renames and ReplaceParameters.
        /// </summary>
        private void CopyProjectItems(XmlNode node, string sourceDirectory, string targetDirectory)
        {
            foreach (XmlNode child in node.ChildNodes)
            {
                if (string.Equals(child.LocalName, "Folder", StringComparison.Ordinal))
                {
                    var folderName = child.Attributes?["Name"]?.Value;
                    if (string.IsNullOrEmpty(folderName))
                    {
                        continue;
                    }

                    var targetFolderName = child.Attributes?["TargetFolderName"]?.Value;
                    targetFolderName = string.IsNullOrEmpty(targetFolderName)
                        ? folderName
                        : this.ReplaceTokens(targetFolderName);

                    this.CopyProjectItems(
                        child,
                        Path.Combine(sourceDirectory, folderName),
                        Path.Combine(targetDirectory, targetFolderName));
                }
                else if (string.Equals(child.LocalName, "ProjectItem", StringComparison.Ordinal))
                {
                    var fileName = child.InnerText.Trim();
                    if (string.IsNullOrEmpty(fileName))
                    {
                        continue;
                    }

                    var targetFileName = child.Attributes?["TargetFileName"]?.Value;
                    targetFileName = string.IsNullOrEmpty(targetFileName)
                        ? fileName
                        : this.ReplaceTokens(targetFileName);

                    var sourcePath = Path.Combine(sourceDirectory, fileName);
                    var targetPath = Path.Combine(targetDirectory, targetFileName);

                    if (!File.Exists(sourcePath))
                    {
                        Debug.WriteLine($"Template item not found on disk: {sourcePath}");
                        continue;
                    }

                    Directory.CreateDirectory(Path.GetDirectoryName(targetPath));

                    var replaceParameters = string.Equals(
                        child.Attributes?["ReplaceParameters"]?.Value,
                        "true",
                        StringComparison.OrdinalIgnoreCase);

                    if (replaceParameters)
                    {
                        // Preserve the source file's byte order mark exactly: keep a BOM
                        // when the source has one (StyleCop SA1412 requires UTF-8 with BOM
                        // for .cs files) and omit it when the source has none (e.g. package.json,
                        // which Volta/npm cannot parse if a BOM is present). Using a non-BOM
                        // UTF-8 fallback ensures BOM-less files stay BOM-less; when a BOM is
                        // detected the reader reports a BOM-emitting UTF-8 encoding instead.
                        Encoding encoding;
                        using (var reader = new StreamReader(sourcePath, new UTF8Encoding(false), detectEncodingFromByteOrderMarks: true))
                        {
                            reader.Peek();
                            encoding = reader.CurrentEncoding;
                        }

                        var content = File.ReadAllText(sourcePath);
                        File.WriteAllText(targetPath, this.ReplaceTokens(content), encoding);
                    }
                    else
                    {
                        File.Copy(sourcePath, targetPath, true);
                    }
                }
            }
        }

        /// <summary>
        /// Replaces $token$ (and $ext_token$) parameters the same way Visual Studio would
        /// when ReplaceParameters is enabled. Longer keys are applied first to avoid
        /// partial overlaps.
        /// </summary>
        private string ReplaceTokens(string input)
        {
            if (string.IsNullOrEmpty(input) || this.replacements == null)
            {
                return input;
            }

            foreach (var pair in this.replacements.OrderByDescending(p => p.Key.Length))
            {
                input = input.Replace(pair.Key, pair.Value);
            }

            return input;
        }

        public void RunStarted(object automationObject, Dictionary<string, string> replacementsDictionary, WizardRunKind runKind, object[] customParams)
        {
            Microsoft.VisualStudio.Shell.ThreadHelper.ThrowIfNotOnUIThread();
            this.dte = automationObject as DTE2;
            string destinationDirectory = replacementsDictionary["$destinationdirectory$"];
            var moduleFolderName = new DirectoryInfo(destinationDirectory).Name;

            // Capture the directory of the root .vstemplate so RunFinished can copy the
            // extracted template source into SDK-style projects that VS leaves empty.
            if (customParams != null && customParams.Length > 0 && customParams[0] is string rootTemplatePath)
            {
                this.templateDirectory = Path.GetDirectoryName(rootTemplatePath);
            }

            try
            {
                var inputForm = new SetupWizard();
                isValid = inputForm.ShowDialog() ?? false;

                if (!isValid)
                {
                    throw new WizardCancelledException();
                }

                replacementsDictionary.Add("$companyname$", inputForm.settings.CompanyName);
                replacementsDictionary.Add("$ownername$", inputForm.settings.OwnerName);
                replacementsDictionary.Add("$owneremail$", inputForm.settings.OwnerEmail);
                replacementsDictionary.Add("$ownerwebsite$", inputForm.settings.OwnerWebsite);
                replacementsDictionary.Add("$modulename$", inputForm.settings.ModuleName);
                replacementsDictionary.Add("$modulefriendlyname$", inputForm.settings.ModuleFriendlyName);
                replacementsDictionary.Add("$rootnamespace$", inputForm.settings.RootNamespace);
                replacementsDictionary.Add("$packagename$", inputForm.settings.PackageName);
                replacementsDictionary.Add("$scopeprefix$", inputForm.settings.ScopePrefix);
                replacementsDictionary.Add("$scopeprefixkebab$", inputForm.settings.ScopePrefix.ToLower().Replace('_', '-'));
                replacementsDictionary.Add("$modulefoldername$", moduleFolderName);

                // Snapshot the tokens (plus their $ext_...$ variants used by linked child
                // projects) so the wizard can reproduce VS parameter replacement when it
                // populates SDK-style (.esproj) projects that VS leaves empty.
                this.replacements = new Dictionary<string, string>();
                foreach (var pair in replacementsDictionary)
                {
                    this.replacements[pair.Key] = pair.Value;
                    if (pair.Key.StartsWith("$") && pair.Key.EndsWith("$"))
                    {
                        var extKey = "$ext_" + pair.Key.Substring(1);
                        if (!this.replacements.ContainsKey(extKey))
                        {
                            this.replacements[extKey] = pair.Value;
                        }
                    }
                }
            }
            catch (WizardCancelledException ex)
            {
                if (Directory.Exists(destinationDirectory))
                {
                    Directory.Delete(destinationDirectory, true);
                }
                Debug.WriteLine(ex);
                throw;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
                throw;
            }
        }

        public bool ShouldAddProjectItem(string filePath)
        {
            Debug.WriteLine("FILE :::::::::: " + filePath);
            return true;
        }
    }
}
