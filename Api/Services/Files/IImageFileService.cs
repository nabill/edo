using System.Collections.Generic;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using HappyTravel.Edo.Api.Models.Agencies;
using Microsoft.AspNetCore.Http;

namespace HappyTravel.Edo.Api.Services.Files
{
    public interface IImageFileService
    {
        Task<Result> Add(IFormFile file);

        Task<Result> Delete(string fileName);
        
        Task<List<SlimUploadedImage>> GetImages();
    }
}