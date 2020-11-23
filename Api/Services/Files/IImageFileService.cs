using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using HappyTravel.Edo.Api.Models.Agencies;
using HappyTravel.Edo.Api.Models.Agents;
using Microsoft.AspNetCore.Http;

namespace HappyTravel.Edo.Api.Services.Files
{
    public interface IImageFileService
    {
        Task<Result> AddOrReplaceBanner(IFormFile file, AgentContext agentContext);

        Task<Result> AddOrReplaceLogo(IFormFile file, AgentContext agentContext);

        Task<Result> DeleteBanner(AgentContext agentContext);

        Task<Result> DeleteLogo(AgentContext agentContext);

        Task<Result<SlimUploadedImage>> GetBanner(AgentContext agentContext);

        Task<Result<SlimUploadedImage>> GetLogo(AgentContext agentContext);
    }
}