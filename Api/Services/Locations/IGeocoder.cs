using System.Collections.Generic;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using HappyTravel.Edo.Api.Models.Locations;

namespace HappyTravel.Edo.Api.Services.Locations
{
    public interface IGeocoder
    {
        ValueTask<Result<List<Prediction>>> GetLocationPredictions(string query, string session, string languageCode);
    }
}