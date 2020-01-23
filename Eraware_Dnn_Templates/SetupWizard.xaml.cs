using Eraware_Dnn_Templates.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Eraware_Dnn_Templates
{
    /// <summary>
    /// Interaction logic for SetupWizard.xaml
    /// </summary>
    public partial class SetupWizard : Window
    {
        public ModuleSettingsVM settings;

        public string CompanyName
        {
            get { return txtCompanyName.Text; }
        }

        public SetupWizard()
        {
            InitializeComponent();
            this.settings = new ModuleSettingsVM();
            this.DataContext = settings;
        }

        private void BtnCancel_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
            this.Close();
        }

        private void TxtRootNamespace_GotFocus(object sender, RoutedEventArgs e)
        {
            this.settings.RootNamespaceLocked = false;
        }

        private void TxtPackageName_GotFocus(object sender, RoutedEventArgs e)
        {
            this.settings.PackageNameLocked = false;
        }

        private void TxtScopePrefix_GotFocus(object sender, RoutedEventArgs e)
        {
            this.settings.ScopePrefixLocked = false;
        }

        public bool CanCreate
        {
            get
            {
                int errorsCount = 0;
                var childs = FormData.Children;
                foreach (var control in childs)
                {
                    if (control is TextBox)
                    {
                        errorsCount += Validation.GetErrors(control as TextBox).Count;
                    }
                }
                return errorsCount == 0;
            }
        }

        private void BtnCreate_Click(object sender, RoutedEventArgs e)
        {
            if (CanCreate)
            {
                this.DialogResult = true;
                this.Close();
            }
            else
            {
                MessageBox.Show("The information provided is not valid, please fix the form or cancel.");
            }
        }
    }
}
