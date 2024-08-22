using $ext_rootnamespace$.Services.Items;
using $ext_rootnamespace$.Services.Localization;
using FluentValidation;
using Moq;
using System.Threading.Tasks;
using Xunit;

namespace UnitTests.Services.Items
{
    public class CreateItemDtoValidatorTests
    {
        LocalizationViewModel localizationViewModel;
        Mock<ILocalizationService> localizationService;
        private readonly IValidator<CreateItemDTO> validator;

        public CreateItemDtoValidatorTests()
        {
            this.localizationViewModel = new LocalizationViewModel
            {
                ModelValidation = new LocalizationViewModel.ModelValidationInfo
                {
                    NameRequired = "Name is required",
                },
            };
            this.localizationService = new Mock<ILocalizationService>();
            localizationService.Setup(l => l.ViewModel).Returns(localizationViewModel);
            this.validator = new CreateItemDtoValidator(localizationService.Object);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData(" ")]
        public async Task RequiresName(string name)
        {
            // Arrange
            var item = new CreateItemDTO
            {
                Name = name
            };

            // Act
            var result = await validator.ValidateAsync(item);

            // Assert
            Assert.False(result.IsValid);
            Assert.Contains(result.Errors, e => e.PropertyName == nameof(item.Name));
            var nameError = result.Errors.Find(e => e.PropertyName == nameof(item.Name));
            Assert.Equal(this.localizationViewModel.ModelValidation.NameRequired, nameError.ErrorMessage);
        }
    }
}