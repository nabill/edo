using System.Globalization;
using CSharpFunctionalExtensions;
using HappyTravel.Edo.Api.Infrastructure;
using Microsoft.AspNetCore.Mvc;

namespace HappyTravel.Edo.Api.Controllers
{
    public class BaseController : ControllerBase
    {
        protected string LanguageCode => CultureInfo.CurrentCulture.Name;

        protected IActionResult OkOrBadRequest<T>(Result<T> model)
        {
            var (_, isFailure, response, error) = model;
            if (isFailure)
                return BadRequest(ProblemDetailsBuilder.Build(error));

            return Ok(response);
        }
    }
}
