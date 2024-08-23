using $ext_rootnamespace$.Services.Items;
using $ext_rootnamespace$.Services.Localization;
using FluentValidation;
using NSubstitute;
using System.Threading.Tasks;
using Xunit;

namespace UnitTests.Services.Items
{
    public class UpdateItemDtoValidatorTests
    {
        LocalizationViewModel localizationViewModel;
        ILocalizationService localizationService;
        private readonly IValidator<UpdateItemDTO> validator;

        public UpdateItemDtoValidatorTests()
        {
            this.localizationViewModel = new LocalizationViewModel
            {
                ModelValidation = new LocalizationViewModel.ModelValidationInfo
                {
                    NameRequired = "Name is required",
                    IdGreaterThanZero = "Id must be greater than zero",
                },
            };
            this.localizationService = Substitute.For<ILocalizationService>();
            localizationService.ViewModel.Returns(localizationViewModel);
            this.validator = new UpdateItemDtoValidator(localizationService);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData(" ")]
        public async Task RequiresName(string name)
        {
            // Arrange
            var item = new UpdateItemDTO
            {
                Id = 123,
                Name = name,
            };

            // Act
            var result = await validator.ValidateAsync(item);

            // Assert
            Assert.False(result.IsValid);
            Assert.Contains(result.Errors, e => e.PropertyName == nameof(item.Name));
            var nameError = result.Errors.Find(e => e.PropertyName == nameof(item.Name));
            Assert.Equal(this.localizationViewModel.ModelValidation.NameRequired, nameError.ErrorMessage);
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-1)]
        public async Task RequiresIdGreaterThanZero(int id)
        {
            // Arrange
            var item = new UpdateItemDTO
            {
                Name = "test name",
                Id = id,
            };

            // Act
            var result = await validator.ValidateAsync(item);

            // Assert
            Assert.False(result.IsValid);
            Assert.Contains(result.Errors, e => e.PropertyName == nameof(item.Id));
            var idError = result.Errors.Find(e => e.PropertyName == nameof(item.Id));
            Assert.Equal(this.localizationViewModel.ModelValidation.IdGreaterThanZero, idError.ErrorMessage);
        }

        [Fact]
        public async Task ValidEntity_ReturnsSuccess()
        {
            // Arrange
            var item = new UpdateItemDTO
            {
                Name = "test name",
                Id = 123,
            };

            // Act
            var result = await validator.ValidateAsync(item);

            // Assert
            Assert.True(result.IsValid);
        }
    }
}