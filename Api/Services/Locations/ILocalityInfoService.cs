using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using HappyTravel.Edo.Api.Models.Locations;

namespace HappyTravel.Edo.Api.Services.Locations
{
    public interface ILocalityInfoService
    {
        Task<Result<LocalityInfo>> GetLocalityInfo(string id);
    }
}