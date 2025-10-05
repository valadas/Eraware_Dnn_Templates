using $ext_rootnamespace$.Controllers;
using DotNetNuke.Web.Api;
using NSubstitute;
using Xunit;

namespace UnitTests.Controllers
{
    public class ServiceRouteMapperTests
    {
        [Fact]
        public void RegisterRoutes_RegistersAtLestOneRoute()
        {
            var mapRouteManager = Substitute.For<IMapRoute>();
            var serviceRouteMapper = new ServiceRouteMapper();

            serviceRouteMapper.RegisterRoutes(mapRouteManager);

            mapRouteManager.MapHttpRoute(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Is<string[]>(x => x != null));
        }
    }
}