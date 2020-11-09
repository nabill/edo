using System;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using Microsoft.AspNetCore.Http;

namespace HappyTravel.Edo.Api.Services.Files
{
    public interface IContractFileService
    {
        Task<Result> Save(int counterpartyId, IFormFile file);
    }
}
