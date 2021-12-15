using System.Threading.Tasks;
using CSharpFunctionalExtensions;

namespace HappyTravel.Edo.Api.AdministratorServices
{
    public interface IAgencyRemovalService
    {
        Task<Result> Delete(int agencyId);
    }
}