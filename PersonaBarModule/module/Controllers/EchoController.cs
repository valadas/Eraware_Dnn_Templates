// MIT License
// Copyright $ext_companyname$

using Dnn.PersonaBar.Library;
using Dnn.PersonaBar.Library.Attributes;
using DotNetNuke.Web.Api;
using $ext_rootnamespace$.Services.EchoService;
using NSwag.Annotations;
using System;
using System.Net;
using System.Threading;
using System.Web.Http;

namespace $ext_rootnamespace$.Controllers
{
    /// <summary>
    /// A REST service to echo messages back.
    /// </summary>
    public class EchoController : PersonaBarApiController
    {
        private readonly IEchoService echoService;

        /// <summary>
        /// Initializes a new instance of the <see cref="EchoController"/> class.
        /// </summary>
        /// <param name="echoService">A service to echo back messages.</param>
        public EchoController(IEchoService echoService)
        {
            this.echoService = echoService;
        }

        /// <summary>
        /// Echoes the specified message.
        /// </summary>
        /// <param name="dto">The details about the message to echo back.</param>
        /// <returns>A viewmodel containing the message.</returns>
        [HttpPost]
        [RequireHost]
        [SwaggerResponse(HttpStatusCode.OK, typeof(EchoViewModel), Description = "OK")]
        public IHttpActionResult Echo(EchoDto dto)
        {
            var result = this.echoService.Echo(dto);
            Thread.Sleep(2000);
            return this.Ok(result);
        }
    }
}
