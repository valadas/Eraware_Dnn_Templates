// MIT License
// Copyright $ext_companyname$

namespace $ext_rootnamespace$.Controllers
{
    using DotNetNuke.Instrumentation;
    using DotNetNuke.Security.Permissions;
    using DotNetNuke.Web.Api;
    using System.Diagnostics.CodeAnalysis;

    /// <summary>
    /// Provides common features to all module controller.
    /// </summary>
    [ExcludeFromCodeCoverage]
    public class ModuleApiController : DnnApiController
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ModuleApiController"/> class.
        /// </summary>
        public ModuleApiController()
        {
            this.Logger = LoggerSource.Instance.GetLogger(this.GetType());
        }

        /// <summary>
        /// Logs to the Dnn Log4Net logger.
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1623:Property summary documentation should match accessors", Justification = "We are not really setting to the log, but logging.")]
        protected ILog Logger { get; }

        /// <summary>
        /// Gets a value indicating whether the user can edit this module.
        /// </summary>
        protected bool CanEdit
        {
            get
            {
                return ModulePermissionController.HasModuleAccess(DotNetNuke.Security.SecurityAccessLevel.Edit, "EDIT", this.ActiveModule);
            }
        }
}
}