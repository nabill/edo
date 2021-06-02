﻿using CSharpFunctionalExtensions;
using HappyTravel.Edo.Api.Infrastructure;
using HappyTravel.Edo.Api.Models.Users;
using HappyTravel.Edo.Api.NotificationCenter.Models;
using HappyTravel.Edo.Api.NotificationCenter.Services;
using HappyTravel.Edo.Api.Services.Management;
using HappyTravel.Edo.Notifications.Enums;
using HappyTravel.Edo.Notifications.Models;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;

namespace HappyTravel.Edo.Api.Controllers.AdministratorControllers
{
    [ApiController]
    [ApiVersion("1.0")]
    [Route("api/{v:apiVersion}/admin/notifications")]
    [Produces("application/json")]
    public class NotificationController : BaseController
    {
        public NotificationController(IAdministratorContext administratorContext, INotificationService notificationService, INotificationOptionsService notificationOptionsService)
        {
            _administratorContext = administratorContext;
            _notificationService = notificationService;
            _notificationOptionsService = notificationOptionsService;
        }


        /// <summary>
        ///     Gets the notification history of the current administrator
        /// </summary>
        /// <param name="skip">Skip</param>
        /// <param name="top">Top</param>
        /// <returns>List of notifications</returns>
        [HttpGet]
        [ProducesResponseType(typeof(List<SlimNotification>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(ProblemDetails), (int)HttpStatusCode.BadRequest)]
        public async Task<IActionResult> GetNotifications([FromQuery] int skip = 0, [FromQuery] int top = 1000)
        {
            var (_, isFailure, admin, error) = await _administratorContext.GetCurrent();
            if (isFailure)
                return BadRequest(ProblemDetailsBuilder.Build(error));

            return Ok(await _notificationService.Get(new SlimAdminContext(admin.Id), skip, top));
        }


        /// <summary>
        ///     Gets the notification options of the current administrator
        /// </summary>
        /// <returns>List of notification options</returns>
        [HttpGet("options")]
        [ProducesResponseType(typeof(Dictionary<NotificationTypes, SlimNotificationOptions>), (int)HttpStatusCode.OK)]
        public async Task<IActionResult> GetNotificationOptions()
        {
            var (_, isFailure, admin, error) = await _administratorContext.GetCurrent();
            if (isFailure)
                return BadRequest(ProblemDetailsBuilder.Build(error));

            return Ok(await _notificationOptionsService.Get(new SlimAdminContext(admin.Id)));
        }


        private readonly IAdministratorContext _administratorContext;
        private readonly INotificationService _notificationService;
        private readonly INotificationOptionsService _notificationOptionsService;
    }
}
