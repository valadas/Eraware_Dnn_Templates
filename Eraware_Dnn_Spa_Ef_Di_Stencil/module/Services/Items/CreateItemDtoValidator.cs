// MIT License
// Copyright $ext_companyname$

using $ext_rootnamespace$.Services.Localization;
using FluentValidation;

namespace $ext_rootnamespace$.Services.Items
{
    /// <summary>
    /// Validates the <see cref="CreateItemDTO"/>.
    /// </summary>
    public class CreateItemDtoValidator : AbstractValidator<CreateItemDTO>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CreateItemDtoValidator"/> class.
        /// </summary>
        /// <param name="localizationService">The localization service.</param>
        public CreateItemDtoValidator(ILocalizationService localizationService)
        {
            var modelValidationViewModel = localizationService.ViewModel.ModelValidation;

            this.RuleFor(x => x.Name)
                .NotEmpty()
                .WithMessage(modelValidationViewModel.NameRequired);
        }
    }
}
