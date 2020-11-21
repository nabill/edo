using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Amazon.S3;
using CSharpFunctionalExtensions;
using HappyTravel.AmazonS3Client.Services;
using HappyTravel.Edo.Api.Infrastructure;
using HappyTravel.Edo.Api.Infrastructure.Options;
using HappyTravel.Edo.Api.Models.Agencies;
using HappyTravel.Edo.Api.Services.Agents;
using Imageflow.Bindings;
using Imageflow.Fluent;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using HappyTravel.Edo.Data;
using HappyTravel.Edo.Data.Agents;
using Microsoft.EntityFrameworkCore;

namespace HappyTravel.Edo.Api.Services.Files
{
    public class ImageFileService : IImageFileService
    {
        public ImageFileService(IAmazonS3ClientService amazonS3ClientService,
            IOptions<ContractFileServiceOptions> options,
            IAgentContextService agentContextService,
            EdoContext edoContext,
            IDateTimeProvider dateTimeProvider)
        {
            _amazonS3ClientService = amazonS3ClientService;
            _bucketName = options.Value.Bucket;
            _s3FolderName = options.Value.S3FolderName;
            _agentContextService = agentContextService;
            _edoContext = edoContext;
            _dateTimeProvider = dateTimeProvider;
        }


        public async Task<Result> Add(IFormFile file)
        {
            var agentContext = await _agentContextService.GetAgent();
             
            return await Validate()
                .Bind(Upload)
                .Map(StoreLink);


            async Task<Result> Validate()
            {
                return await Result.Success()
                    .Ensure(() => file != default, "Could not get the file")
                    .Ensure(() => _imageExtensions.Contains(Path.GetExtension(file?.FileName)?.ToLower()), "The file must have extension of a jpeg image")
                    .Ensure(() => !file.FileName.Intersect(_prohibitedChars).Any(),
                        "File name shouldn't contain the following characters: ` \" ' : ; \\r \\n \\t \\ /")
                    .Map(GetImage)
                    .Bind(ValidateImage);


                async Task<ImageInfo> GetImage()
                {
                    await using var stream = file.OpenReadStream();
                    using var binaryReader = new BinaryReader(stream);
                    var imageBytes = binaryReader.ReadBytes((int)file.Length);
                    return await ImageJob.GetImageInfo(new BytesSource(imageBytes));
                }


                Result ValidateImage(ImageInfo imageInfo)
                {
                    // Here should be checks like aspect ratio, for now only resolution checks
                    return Result.Success()
                        .Ensure(() => imageInfo.ImageWidth >= MinimumWidth && imageInfo.ImageWidth <= MaximumWidth,
                            $"Image width must be in range from {MinimumWidth} to {MaximumWidth}")
                        .Ensure(() => imageInfo.ImageHeight >= MinimumHeight && imageInfo.ImageHeight <= MaximumHeight,
                            $"Image height must be in range from {MinimumWidth} to {MaximumWidth}");
                }
            }


            async Task<Result<string>> Upload()
            {
                await using var stream = file.OpenReadStream();
                return await _amazonS3ClientService.Add(_bucketName, GetKey(agentContext.AgencyId, file.FileName), stream, S3CannedACL.PublicRead);
            }


            async Task StoreLink(string url)
            {
                var fileName = file.FileName.ToLower();

                var oldUploadedImage = await _edoContext.UploadedImages.SingleOrDefaultAsync(i => i.AgencyId == agentContext.AgentId && i.FileName == fileName);

                if (oldUploadedImage == null)
                {
                    var now = _dateTimeProvider.UtcNow();

                    var newUploadedImage = new UploadedImage
                    {
                        AgencyId = agentContext.AgencyId,
                        Url = url,
                        FileName = fileName,
                        Created = now,
                        Updated = now
                    };

                    _edoContext.Add(newUploadedImage);
                }
                else
                {
                    oldUploadedImage.Updated = _dateTimeProvider.UtcNow();
                    oldUploadedImage.Url = url;

                    _edoContext.Update(oldUploadedImage);
                }

                await _edoContext.SaveChangesAsync();
            }
        }


        public async Task<Result> Delete(string fileName)
        {
            var agentContext = await _agentContextService.GetAgent();

            return await GetImageRecord()
                .Bind(DeleteFromS3)
                .Tap(DeleteRecord);


            async Task<Result<UploadedImage>> GetImageRecord()
            {
                var image = await _edoContext.UploadedImages.SingleOrDefaultAsync(i => i.AgencyId == agentContext.AgentId && i.FileName == fileName.ToLower());
                
                return image == null
                    ? Result.Failure<UploadedImage>("Could not find image with specified name")
                    : Result.Success(image);
            }


            async Task<Result<UploadedImage>> DeleteFromS3(UploadedImage image)
            {
                var (isSuccess, _, _) = await _amazonS3ClientService.Delete(_bucketName, GetKey(agentContext.AgencyId, fileName));

                return isSuccess
                    ? Result.Success(image)
                    : Result.Failure<UploadedImage>("Error during deleting image from storage");
            }


            async Task DeleteRecord(UploadedImage image)
            {
                _edoContext.UploadedImages.Remove(image);
                await _edoContext.SaveChangesAsync();
            }
        }


        public async Task<List<SlimUploadedImage>> GetImages()
        {
            var agentContext = await _agentContextService.GetAgent();

            return await _edoContext.UploadedImages
                .Where(i => i.AgencyId == agentContext.AgencyId)
                .OrderBy(i => i.FileName)
                .Select(i => new SlimUploadedImage(i.FileName, i.Url, i.Created, i.Updated))
                .ToListAsync();
        }


        private string GetKey(int agencyId, string fileName) => $"{_s3FolderName}/{agencyId}/{fileName.ToLower()}";


        private const int MinimumWidth = 200;
        private const int MaximumWidth = 800;
        private const int MinimumHeight = 200;
        private const int MaximumHeight = 800;

        private readonly List<string> _imageExtensions = new List<string>{"jpeg", "jpg"};
        private readonly List<char> _prohibitedChars = "\"\r\n\t'`;:|\\/".ToList();

        private readonly IAmazonS3ClientService _amazonS3ClientService;
        private readonly string _bucketName;
        private readonly string _s3FolderName;
        private readonly IAgentContextService _agentContextService;
        private readonly EdoContext _edoContext;
        private readonly IDateTimeProvider _dateTimeProvider;
    }
}
