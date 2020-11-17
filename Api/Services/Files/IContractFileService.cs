using System;
using System.IO;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using Microsoft.AspNetCore.Http;

namespace HappyTravel.Edo.Api.Services.Files
{
    public interface IContractFileService
    {
        Task<Result> Add(int counterpartyId, IFormFile file);

        Task<Result<(Stream stream, string contentType)>> Get(int counterpartyId);
    }
}
