using $ext_rootnamespace$.Controllers;
using DotNetNuke.Web.Api;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace UnitTests.Controllers
{
    public class ServiceRouteMapperTests
    {
        [Fact]
        public void RegisterRoutes_RegistersAtLestOneRoute()
        {
            var mapRouteManager = new Mock<IMapRoute>();
            var serviceRouteMapper = new ServiceRouteMapper();

            serviceRouteMapper.RegisterRoutes(mapRouteManager.Object);

            mapRouteManager.Verify(m => m.MapHttpRoute(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsNotNull<string[]>()), Times.AtLeastOnce);
        }
    }
}