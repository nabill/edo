using System.Collections.Generic;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using HappyTravel.Edo.Api.Models.Locations;
using HappyTravel.Edo.Common.Enums;
using Prediction = HappyTravel.Edo.Api.Models.Locations.Prediction;

namespace HappyTravel.Edo.Api.Services.Locations
{
    public interface IGeoCoder
    {
        Task<Result<Location>> GetLocation(string locationId, string sessionId, LocationTypes type);

        ValueTask<Result<List<Prediction>>> GetLocationPredictions(string query, string sessionId, string languageCode);
    }
}