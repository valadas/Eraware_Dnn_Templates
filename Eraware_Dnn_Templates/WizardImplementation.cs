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
using System.Windows.Forms;

namespace Eraware_Dnn_Templates
{
    /// <summary>
    /// This method is called before opening any item that has the OpenInEditor attribute
    /// </summary>
    internal class WizardImplementation : IWizard
    {
        private bool isValid = false;
        private DTE2 dte;

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
                if (project.FullName.Contains("build"))
                {
                    foreach (ProjectItem item in project.ProjectItems)
                    {
                        if (item.Name.Contains("docs"))
                        {
                            item.Remove();
                        }
                    }
                }

                if (project.FullName.Contains("module.web"))
                {
                    var configurations = (Array)project.ConfigurationManager.ConfigurationRowNames;
                    foreach(var configuration in configurations)
                    {
                        project.ConfigurationManager.DeleteConfigurationRow(configuration.ToString());
                    }
                }

                if (project.FullName.Contains("Module\\Module"))
                {
                    foreach (ProjectItem item in project.ProjectItems)
                    {
                        string[] ignoredFiles = { ".nuke", "build.ps1", "build.sh", ".github" };
                        if (ignoredFiles.Any(i => i == item.Name)){
                            item.Remove();
                        }
                    }

                    string moduleProjectFilePath = project.FullName;
                    dte.Solution.Remove(project);
                    var moduleProjectFile = new FileInfo(moduleProjectFilePath);
                    var originalDirectory = moduleProjectFile.Directory;

                    this.CopyAll(originalDirectory, originalDirectory.Parent);

                    dte.Solution.AddFromFile(Path.Combine(originalDirectory.Parent.FullName, moduleProjectFile.Name));
                    originalDirectory.Delete(true);
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

        public void RunStarted(object automationObject, Dictionary<string, string> replacementsDictionary, WizardRunKind runKind, object[] customParams)
        {
            Microsoft.VisualStudio.Shell.ThreadHelper.ThrowIfNotOnUIThread();
            this.dte = automationObject as DTE2;
            string destinationDirectory = replacementsDictionary["$destinationdirectory$"];

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
                replacementsDictionary.Add("$ownerwebsite$", inputForm.settings.OwnerEmail);
                replacementsDictionary.Add("$modulename$", inputForm.settings.ModuleName);
                replacementsDictionary.Add("$modulefriendlyname$", inputForm.settings.ModuleFriendlyName);
                replacementsDictionary.Add("$rootnamespace$", inputForm.settings.RootNamespace);
                replacementsDictionary.Add("$packagename$", inputForm.settings.PackageName);
                replacementsDictionary.Add("$scopeprefix$", inputForm.settings.ScopePrefix);
                replacementsDictionary.Add("$scopeprefixkebab$", inputForm.settings.ScopePrefix.ToLower().Replace('_', '-'));
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
