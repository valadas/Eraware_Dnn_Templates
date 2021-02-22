// MIT License
// Copyright $ext_companyname$

using DotNetNuke.Instrumentation;
using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace Eraware.Modules.MyModule.Services
{
    /// <summary>
    /// Implements a default logging mechanism.
    /// </summary>
    internal class LoggingService : ILoggingService
    {
        private ILog logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="LoggingService"/> class.
        /// </summary>
        public LoggingService()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="LoggingService"/> class.
        /// </summary>
        /// <remarks>User this constructor only for testing.</remarks>
        /// <param name="logger">The logger to use during testing.</param>
        internal LoggingService(ILog logger)
        {
            this.logger = logger;
        }

        /// <inheritdoc/>
        public void LogError(string message)
        {
            StackFrame frame = new StackFrame(1);
            var method = frame.GetMethod();
            this.LogError(message, method.DeclaringType);
        }

        /// <inheritdoc/>
        public void LogError(string message, Exception ex)
        {
            StackFrame frame = new StackFrame(1);
            var method = frame.GetMethod();
            this.LogError(message, ex, method.DeclaringType);
        }

        private void LogError(string message, Type type)
        {
            this.SetupLogger(type);
            this.logger.Error(message);
        }

        private void LogError(string message, Exception ex, Type type)
        {
            this.SetupLogger(type);
            this.logger.Error(message, ex);
        }

        private void SetupLogger(Type type)
        {
            if (this.logger is null)
            {
                this.logger = LoggerSource.Instance.GetLogger(type);
            }
        }
    }
}