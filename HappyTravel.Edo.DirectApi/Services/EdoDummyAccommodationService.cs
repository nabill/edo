using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using HappyTravel.Edo.Api.Models.Accommodations;
using HappyTravel.Edo.Api.Services.Accommodations;
using Microsoft.AspNetCore.Mvc;

namespace HappyTravel.Edo.DirectApi.Services
{
    public class EdoDummyAccommodationService : IAccommodationService
    {
        public async Task<Result<Accommodation, ProblemDetails>> Get(string htId, string languageCode) 
            => throw new System.NotImplementedException();
    }
}