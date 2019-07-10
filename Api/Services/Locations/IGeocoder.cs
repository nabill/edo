using System.Collections.Generic;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using HappyTravel.Edo.Api.Models.Locations.Google;
using Prediction = HappyTravel.Edo.Api.Models.Locations.Prediction;

namespace HappyTravel.Edo.Api.Services.Locations
{
    public interface IGeoCoder
    {
        Task<Result<Place>> GetPlace(string placeId, string sessionId);

        ValueTask<Result<List<Prediction>>> GetPlacePredictions(string query, string sessionId, string languageCode);
    }
}