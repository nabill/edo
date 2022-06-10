using CSharpFunctionalExtensions;
using HappyTravel.Edo.Api.Infrastructure;
using Microsoft.AspNetCore.Mvc;

namespace HappyTravel.Edo.DirectApi.Controllers;

public class BaseController : ControllerBase
{
    protected ActionResult<T> OkOrBadRequest<T>(Result<T> model)
    {
        var (isSuccess, _, response, error) = model;

        return isSuccess
            ? response
            : BadRequest(ProblemDetailsBuilder.Build(error));
    }
}