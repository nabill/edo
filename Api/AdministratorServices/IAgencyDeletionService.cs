using System.Threading.Tasks;
using CSharpFunctionalExtensions;

namespace HappyTravel.Edo.Api.AdministratorServices
{
    public interface IAgencyDeletionService
    {
        Task<Result> Delete(int agencyId);
    }
}