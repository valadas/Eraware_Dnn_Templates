// MIT License
// Copyright $ext_companyname$

namespace $ext_rootnamespace$.Controllers
{
    using DotNetNuke.Services.Localization;
    using System.Net;
    using System.Net.Http;
    using System.Text;
    using System.Web.Hosting;
    using System.Web.Http.Controllers;
    using System.Web.Http.Filters;

    /// <summary>
    /// Validates the DTO for the request.
    /// </summary>
    internal class ValidateModelAttribute : ActionFilterAttribute
    {
        private string resourceFileRoot;

        private string ResourceFileRoot
        {
            get
            {
                if (string.IsNullOrWhiteSpace(this.resourceFileRoot))
                {
                    this.resourceFileRoot = HostingEnvironment.MapPath("~/DesktopModules/$ext_modulefoldername$/resources/App_LocalResources/ModelValidation.resx");
                }

                return this.resourceFileRoot;
            }
        }

        /// <summary>
        /// When the action executes, validates the model state.
        /// </summary>
        /// <param name="actionContext">The context of the action.</param>
        public override void OnActionExecuting(HttpActionContext actionContext)
        {
            if (actionContext.ModelState.IsValid == false)
            {
                var localization = new LocalizationProvider();
                var sb = new StringBuilder();
                foreach (var value in actionContext.ModelState.Values)
                {
                    foreach (var error in value.Errors)
                    {
                        var localized = localization.GetString(error.ErrorMessage, this.ResourceFileRoot);
                        if (string.IsNullOrWhiteSpace(localized))
                        {
                            localized = error.ErrorMessage;
                        }

                        sb.AppendLine(localized);
                    }
                }

                actionContext.Response = actionContext.Request.CreateErrorResponse(
                    HttpStatusCode.BadRequest,
                    sb.ToString());
            }
        }
    }
}
