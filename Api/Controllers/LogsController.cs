using System;
using System.Diagnostics;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace HappyTravel.Edo.Api.Controllers
{
    [ApiController]
    [AllowAnonymous]
    [ApiVersion("1.0")]
    [Route("api/{v:apiVersion}/logs")]
    [Produces("application/json")]
    public class LogsController : ControllerBase
    {
        public LogsController(ILoggerFactory loggerFactory)
        {
            _logger = loggerFactory.CreateLogger<LogsController>();
        }


        [HttpGet("es")]
        public IActionResult LogViaEventSource()
        {
            var stopwatch = new Stopwatch();

            stopwatch.Start();
            for (var i = 0; i < 10000; i++)
                _logger.LogInformation($"Eod log test: {DateTime.UtcNow.Ticks}");

            return Ok(stopwatch.ElapsedMilliseconds);
        }


        private readonly ILogger<LogsController> _logger;
    }
}