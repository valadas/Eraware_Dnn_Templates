using DotNetNuke.Entities.Users;
using $ext_rootnamespace$.Controllers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace UnitTests.Controllers
{
    public class ModuleApiControllerTests
    {
        [Fact]
        public void CanEdit_Set_Throws()
        {
            var controller = new Controller();

            var ex = Assert.Throws<Exception>(() => controller.CanEdit = true);
            Assert.False(string.IsNullOrWhiteSpace(ex.Message));
        }

        [Fact]
        public void CanEdit_DefaultsToFalse()
        {
            var controller = new Controller();

            var canEdit = controller.CanEdit;

            Assert.False(canEdit);
        }

        [Fact]
        public void HasUserInfo()
        {
            var controller = new Controller();
            UserInfo userInfo;

            Action getUserInfo = () => userInfo = controller.UserInfo;

            // Not a great test but keeps coverage at 100%.
            // Could be improved is we get better abstactions in Dnn for UserInfo.
            Assert.Throws<NullReferenceException>(getUserInfo);
        }

        class Controller : ModuleApiController
        {
        }
    }
}