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

        class Controller : ModuleApiController
        {
        }
    }
}