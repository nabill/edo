using System.Threading.Tasks;
using HappyTravel.Edo.Api.Services.Customers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.DependencyInjection;

namespace HappyTravel.Edo.Api.Filters
{
    public class CustomerContextFilter : ActionFilterAttribute
    {
        public override async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            var customerContext = context.HttpContext.RequestServices.GetRequiredService<ICustomerContext>();
            var (_, isFailure, _, _) = await customerContext.GetCustomerInfo();
            if (isFailure)
            {
                context.Result = new ForbidResult();
                return;
            }

            await next();
        }
    }
}