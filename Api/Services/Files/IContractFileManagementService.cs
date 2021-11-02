using System;
using System.IO;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using Microsoft.AspNetCore.Http;

namespace HappyTravel.Edo.Api.Services.Files
{
    public interface IContractFileManagementService
    {
        Task<Result> Add(int agencyId, IFormFile file);

        Task<Result<(Stream stream, string contentType)>> Get(int agencyId);
    }
}
