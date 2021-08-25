using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using HappyTravel.Edo.Api.Models.Management.Administrators;

namespace HappyTravel.Edo.Api.AdministratorServices
{
    public interface IAdministratorService
    {
        Task<Result<RichAdministratorInfo>> GetCurrentWithPermissions();
    }
}