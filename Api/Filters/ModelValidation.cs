using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace HappyTravel.Edo.Api.Filters
{
    public class ModelValidation : IActionFilter
    {
        public void OnActionExecuting(ActionExecutingContext filterContext)
        {
            if (!filterContext.ModelState.IsValid)
                filterContext.Result = new BadRequestObjectResult(filterContext.ModelState);
        }

        public void OnActionExecuted(ActionExecutedContext context) { }
    }
}