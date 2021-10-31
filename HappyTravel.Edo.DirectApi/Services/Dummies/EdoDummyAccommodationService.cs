using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using HappyTravel.Edo.Api.Models.Accommodations;
using HappyTravel.Edo.Api.Services.Accommodations;
using Microsoft.AspNetCore.Mvc;

namespace HappyTravel.Edo.DirectApi.Services.Dummies
{
    public class EdoDummyAccommodationService : IAccommodationService
    {
        public Task<Result<Accommodation, ProblemDetails>> Get(string htId, string languageCode) 
            => Task.FromResult(Result.Success<Accommodation, ProblemDetails>(default));
    }
}