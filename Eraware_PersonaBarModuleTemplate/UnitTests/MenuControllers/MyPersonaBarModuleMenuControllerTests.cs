using Dnn.PersonaBar.Library.Controllers;
using Dnn.PersonaBar.Library.Model;
using $ext_rootnamespace$.MenuControllers;
using Xunit;

namespace UnitTests.MenuControllers
{
    public class $ext_safeprojectname$MenuControllerTests
    {
        IMenuItemController menuItemController;

        public $ext_safeprojectname$MenuControllerTests()
        {
            this.menuItemController = new $ext_safeprojectname$MenuController();
        }

        [Fact]
        public void IsVisible()
        {
            var visible = this.menuItemController.Visible(new MenuItem());
            Assert.True(visible);
        }

        [Fact]
        public void UpdateParametersDoesNothing()
        {
            this.menuItemController.UpdateParameters(new MenuItem());
        }

        [Fact]
        public void GetSettings_ReturnsSampleSetting()
        {
            var menuItem = new MenuItem();
            var settings = this.menuItemController.GetSettings(menuItem);
            Assert.Equal("SampleValue", settings["SampleSetting"]);
        }
    }
}
