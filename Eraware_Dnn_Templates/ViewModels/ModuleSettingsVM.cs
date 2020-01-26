using Microsoft.CSharp;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;

namespace Eraware_Dnn_Templates.ViewModels
{
    public class ModuleSettingsVM : ObservableObject
    {
        #region private members
        private string companyName = "Eraware";
        private string ownerName = "Daniel Valadas";
        private string ownerEmail = "info@danielvaladas.com";
        private string ownerWebsite = "https://concepteurweb.ca";
        private string moduleName = "MyModule";
        private string moduleFriendlyName = "My Module";
        private string rootNamespace = "Eraware.Modules.MyModule";
        private string packageName = "Eraware_MyModule";
        private string scopePrefix = "Era_MyModule";
        private bool rootNamespaceLocked = true;
        private bool packageNameLocked = true;
        private bool scopePrefixLocked = true;
        #endregion

        #region static members
        public static ValidationResult IsValidPrefix(string prefix)
        {
            if (string.IsNullOrEmpty(prefix))
            {
                return new ValidationResult("The value is required");
            }
            var test = new Regex("^[A-Za-z_][A-Za-z0-9_]*$");
            if (test.IsMatch(prefix))
            {
                return ValidationResult.Success;
            }
            return new ValidationResult("Can only contain letters, numbers and _ and cannot start with a number.");
        }

        public static ValidationResult IsValidNamespace(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                return new ValidationResult("The namepace is required.");
            }
            if (Regex.IsMatch(name, "\\.\\."))
            {
                return new ValidationResult("'..' is not allowed in namespace.");
            }
            foreach (var part in name.Split('.'))
            {
                if (!CSharpCodeProvider.CreateProvider("CSharp").IsValidIdentifier(part))
                {
                    return new ValidationResult("This value is not a valid namespace identifier.");
                }
            }
            return ValidationResult.Success;
        }
        #endregion

        #region public properties
        [Required(ErrorMessage = "The company name is required.")]
        public string CompanyName
        {
            get
            {
                return this.companyName;
            }
            set
            {
                ValidateProperty(value, "CompanyName");
                OnPropertyChanged(ref companyName, value);
                UpdateRootNamespace();
                UpdatePackageName();
                UpdateScopePrefix();
            }
        }

        [Required(ErrorMessage = "The Owner name is required.")]
        public string OwnerName
        {
            get { return ownerName; }
            set
            {
                ValidateProperty(value, "OwnerName");
                OnPropertyChanged(ref ownerName, value);
            }
        }

        [Required(ErrorMessage = "The owner email is required.")]
        [EmailAddress(ErrorMessage = "The owner email is not a valid email.")]
        public string OwnerEmail
        {
            get { return ownerEmail; }
            set
            {
                ValidateProperty(value, "OwnerEmail");
                OnPropertyChanged(ref ownerEmail, value);
            }
        }

        [Required(ErrorMessage = "The owner website is required.")]
        [Url(ErrorMessage = "This value is not a valid url.")]
        public string OwnerWebsite
        {
            get { return ownerWebsite; }
            set
            {
                ValidateProperty(value, "OwnerWebsite");
                OnPropertyChanged(ref ownerWebsite, value);
            }
        }

        [Required(ErrorMessage = "The module friendly name is required.")]
        public string ModuleFriendlyName
        {
            get { return moduleFriendlyName; }
            set
            {
                ValidateProperty(value, "ModuleFriendlyName");
                OnPropertyChanged(ref moduleFriendlyName, value);
            }
        }

        [Required]
        [CustomValidation(typeof(ModuleSettingsVM), "IsValidPrefix")]
        public string ScopePrefix
        {
            get { return scopePrefix; }
            set
            {
                ValidateProperty(value, "ScopePrefix");
                OnPropertyChanged(ref scopePrefix, value);
            }
        }

        [Required]
        [RegularExpression("^[A-Za-z][A-Za-z0-9_]*", ErrorMessage = "The module name must start with a letter and contain only letters, numbers and _")]
        public string ModuleName
        {
            get { return moduleName; }
            set
            {
                ValidateProperty(value, "ModuleName");
                OnPropertyChanged(ref moduleName, value);
                UpdatePackageName();
                UpdateRootNamespace();
                UpdateScopePrefix();
            }
        }

        [Required]
        [CustomValidation(typeof(ModuleSettingsVM), "IsValidPrefix")]
        public string PackageName
        {
            get { return packageName; }
            set
            {
                ValidateProperty(value, "PackageName");
                OnPropertyChanged(ref packageName, value);
            }
        }

        [Required(ErrorMessage = "The root namepace is required.")]
        [CustomValidation(typeof(ModuleSettingsVM), "IsValidNamespace")]
        public string RootNamespace
        {
            get { return rootNamespace; }
            set
            {
                ValidateProperty(value, "RootNamespace");
                OnPropertyChanged(ref rootNamespace, value);
            }
        }
        #endregion

        public bool RootNamespaceLocked
        {
            get { return rootNamespaceLocked; }
            set
            {
                OnPropertyChanged(ref rootNamespaceLocked, value);
            }
        }

        public bool PackageNameLocked
        {
            get { return packageNameLocked; }
            set
            {
                OnPropertyChanged(ref packageNameLocked, value);
            }
        }

        public bool ScopePrefixLocked
        {
            get { return scopePrefixLocked; }
            set
            {
                OnPropertyChanged(ref scopePrefixLocked, value);
            }
        }

        #region private methods
        private void UpdateScopePrefix()
        {
            if (scopePrefixLocked)
            {
                var companyPrefix = GetSafeName(companyName.Split(' '));
                ScopePrefix = (companyPrefix.Length > 3 ? companyPrefix.Substring(0,3) : companyPrefix) + "_" + ModuleName;
            }
        }

        private string GetSafeName(string name)
        {
            var safeName = new String(name.Where(Char.IsLetter).ToArray());
            if (string.IsNullOrWhiteSpace(safeName))
            {
                return "";
            }
            return char.ToUpperInvariant(safeName[0]).ToString() + (safeName.Length > 1 ? safeName.Substring(1) : "");
        }

        private void UpdatePackageName()
        {
            if (packageNameLocked)
            {
                var newPackageName = string.Empty;
                newPackageName += GetSafeName(CompanyName.Split(' '));
                newPackageName += "_";
                newPackageName += ModuleName;
                PackageName = newPackageName;
            }
        }

        private string GetSafeName(string[] parts)
        {
            var result = string.Empty;
            foreach (var part in parts)
            {
                result += GetSafeName(part);
            }
            return result;
        }

        private void UpdateRootNamespace()
        {
            if (rootNamespaceLocked)
            {
                RootNamespace = GetSafeName(CompanyName.Split(' ')) + ".Modules." + GetSafeName(ModuleName);
            }
        }

        private void ValidateProperty<T>(T value, string name)
        {
            @Validator.ValidateProperty(value, new ValidationContext(this, null, null)
            {
                MemberName = name
            });
        }
        #endregion
    }
}
