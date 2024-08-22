using DotNetNuke.Entities.Users;
using $ext_rootnamespace$.Controllers;
using $ext_rootnamespace$.Services.Localization;
using Moq;
using System.Web.Http.Results;
using Xunit;

namespace UnitTests.Controllers
{
    public class LocalizationControllerTests
    {
        private readonly Mock<ILocalizationService> localizationService;
        private readonly LocalizationController localizationController;

        public LocalizationControllerTests()
        {
            this.localizationService = new Mock<ILocalizationService>();
            this.localizationController = new FakeLocalizationController(this.localizationService.Object);
        }

        [Fact]
        public void GetLocalization_CallsLocalizationService()
        {
            var expectedResponse = new LocalizationViewModel();
            this.localizationService.Setup(s => s.ViewModel)
                .Returns(expectedResponse);

            var result = this.localizationController.GetLocalization();

            var response = Assert.IsType<OkNegotiatedContentResult<LocalizationViewModel>>(result);
            Assert.Equal(expectedResponse, response.Content);
        }
    }

    public class FakeLocalizationController : LocalizationController
    {
        private bool canEdit = false;
        public readonly ILocalizationService localizationService;

        public FakeLocalizationController(ILocalizationService localizationService)
            : base(localizationService)
        {
            this.localizationService = localizationService;
        }

        public override UserInfo UserInfo => new UserInfo() { UserID = 123 };

        public override bool CanEdit
        {
            get { return this.canEdit; }
            set { this.canEdit = value; }
        }
    }
}
