using System;
using System.IO;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using HappyTravel.Edo.Api.Models.Agents;
using Microsoft.AspNetCore.Http;

namespace HappyTravel.Edo.Api.Services.Files
{
    public interface IContractFileService
    {
        Task<Result> Add(int counterpartyId, IFormFile file);

        Task<Result<(Stream stream, string contentType)>> GetForAgent(AgentContext agentContext);

        Task<Result<(Stream stream, string contentType)>> Get(int counterpartyId);
    }
}
