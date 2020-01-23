using EnvDTE;
using Microsoft.VisualStudio.TemplateWizard;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Windows.Forms;

namespace Eraware_Dnn_Templates
{
    /// <summary>
    /// This method is called before opening any item that has the OpenInEditor attribute
    /// </summary>
    internal class WizardImplementation : IWizard
    {
        private bool isValid = false;

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
        }

        public void RunStarted(object automationObject, Dictionary<string, string> replacementsDictionary, WizardRunKind runKind, object[] customParams)
        {
            Microsoft.VisualStudio.Shell.ThreadHelper.ThrowIfNotOnUIThread();
            DTE dte = automationObject as DTE;
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
            return true;
        }
    }
}
