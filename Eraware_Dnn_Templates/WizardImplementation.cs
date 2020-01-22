using EnvDTE;
using Microsoft.VisualStudio.TemplateWizard;
using System;
using System.Collections.Generic;
using System.Windows.Forms;

namespace Eraware_Dnn_Templates
{
    /// <summary>
    /// This method is called before opening any item that has the OpenInEditor attribute
    /// </summary>
    internal class WizardImplementation : IWizard
    {
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
            try
            {
                var inputForm = new UserInputForm();
                inputForm.ShowDialog();

                var rootNamespace = UserInputForm.RootNamespace;

                replacementsDictionary.Add("$rootnamespace$", rootNamespace);
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
