namespace $ext_rootnamespace$.Services
{
    using DotNetNuke.Instrumentation;
    using DotNetNuke.Web.Api;

    /// <summary>
    /// Provides common features to all module controller.
    /// </summary>
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
    }
}