using System.Threading.Tasks;
using HappyTravel.Edo.Api.Services.Customers;
using HappyTravel.Edo.Common.Enums;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.DependencyInjection;

namespace HappyTravel.Edo.Api.Filters
{
    public class InCompanyPermissionsFilter  : ActionFilterAttribute
    {
        private readonly InCompanyPermissions _permissions;


        public InCompanyPermissionsFilter(InCompanyPermissions permissions)
        {
            _permissions = permissions;
        }
        
        public override async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            var permissionChecker = context.HttpContext.RequestServices.GetRequiredService<IPermissionChecker>();
            var customerContext = context.HttpContext.RequestServices.GetRequiredService<ICustomerContext>();
            var customer = await customerContext.GetCustomer();
            var (_, isFailure, _) = await permissionChecker.CheckInCompanyPermission(customer, _permissions);
            if (isFailure)
            {
                context.Result = new ForbidResult();
                return;
            }

            await next();
        }
    }
}