// MIT License
// Copyright $ext_companyname$

using DotNetNuke.DependencyInjection;
using DotNetNuke.Instrumentation;
using $ext_rootnamespace$.Services;
using System;
using System.Net;
using System.Net.Http;
using System.Runtime.CompilerServices;
using System.Web.Http.Filters;

namespace $ext_rootnamespace$.Controllers
{
    /// <summary>
    /// Handles exceptions on API endpoint calls.
    /// </summary>
    internal class ModuleExceptionFilterAttribute : ExceptionFilterAttribute
    {
        /// <summary>
        /// Gets or sets the logger source.
        /// </summary>
        [DotNetNuke.DependencyInjection.Dependency]
        public ILoggerSource LoggerSource { get; set; }
        
        /// <inheritdoc/>
        public override void OnException(HttpActionExecutedContext actionExecutedContext)
        {
            var exception = actionExecutedContext.Exception;

            if (exception is null)
            {
                return;
            }

            var log = this.LoggerSource.GetLogger(nameof(ModuleExceptionFilterAttribute));
            log.Error(exception.Message, exception);

            if (exception is ArgumentException)
            {
                actionExecutedContext.Response = new
                    HttpResponseMessage(HttpStatusCode.BadRequest)
                {
                    Content = new StringContent(exception.Message),
                };
                return;
            }

            if (exception is NotImplementedException)
            {
                actionExecutedContext.Response = new
                    HttpResponseMessage(HttpStatusCode.NotImplemented)
                {
                    Content = new StringContent(exception.Message),
                };
                return;
            }

            if (exception is TimeoutException)
            {
                actionExecutedContext.Response = new
                    HttpResponseMessage(HttpStatusCode.RequestTimeout)
                {
                    Content = new StringContent(exception.Message),
                };
                return;
            }

            if (exception is UnauthorizedAccessException)
            {
                actionExecutedContext.Response = new
                    HttpResponseMessage(HttpStatusCode.Unauthorized)
                {
                    Content = new StringContent(exception.Message),
                };
                return;
            }

            // This fallback prevents exposing details about
            // unexpected exceptions for security reasons.
            actionExecutedContext.Response = new
                HttpResponseMessage(HttpStatusCode.InternalServerError)
            {
                Content = new StringContent("An unexpected error occurred."),
            };
        }
    }
}