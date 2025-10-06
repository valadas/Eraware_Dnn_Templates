// MIT License
// Copyright $ext_companyname$

using DotNetNuke.Web.Api;

namespace $ext_rootnamespace$.Controllers
{
    /// <summary>
    /// Implements the Dnn IServiceRouteMapper to register this module routes.
    /// </summary>
    public class ServiceRouteMapper : IServiceRouteMapper
    {
        /// <inheritdoc/>
        public void RegisterRoutes(IMapRoute mapRouteManager)
        {
            mapRouteManager?.MapHttpRoute("$ext_packagename$", "default", "{controller}/{action}", new[] { typeof(ServiceRouteMapper).Namespace });
        }
    }
}