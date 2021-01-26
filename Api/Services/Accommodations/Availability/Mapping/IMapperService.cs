using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using HappyTravel.Edo.Api.Models.Availabilities.Mapping;

namespace HappyTravel.Edo.Api.Services.Accommodations.Availability.Mapping
{
    public interface IMapperService
    {
        Task<Result<LocationDescriptor>> GetLocationDescriptor(string htId);
    }
}