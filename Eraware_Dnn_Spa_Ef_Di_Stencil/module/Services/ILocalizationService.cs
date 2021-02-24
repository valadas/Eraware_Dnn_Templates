// MIT License
// Copyright $ext_companyname$
using $ext_rootnamespace$.ViewModels;

namespace $ext_rootnamespace$.Services
{
    /// <summary>
    /// Provides strongly typed localization services for this module.
    /// </summary>
    public interface ILocalizationService
    {
        /// <summary>
        /// Gets viewmodel that strongly types all resource keys.
        /// </summary>
        LocalizationViewModel ViewModel { get; }
    }
}