using System.Threading.Tasks;

namespace HappyTravel.Edo.Api.Services.Employees
{
    public interface IEmployeeContext
    {
        Task<bool> HasGlobalPermission(EmployeePermissions permission);
    }
}