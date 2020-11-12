using System;
using System.IO;
using System.Threading.Tasks;
using Amazon.S3;
using CSharpFunctionalExtensions;
using HappyTravel.AmazonS3Client.Services;
using HappyTravel.Edo.Api.Infrastructure.Options;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;

namespace HappyTravel.Edo.Api.Services.Files
{
    public class ContractFileService : IContractFileService
    {
        public ContractFileService(IAmazonS3ClientService amazonS3ClientService,
            IOptions<ContractFileServiceOptions> options)
        {
            _amazonS3ClientService = amazonS3ClientService;
            _bucketName = options.Value.Bucket;
            _s3FolderName = options.Value.S3FolderName;
        }


        public async Task<Result> Save(int counterpartyId, IFormFile file)
        {
            return await Validate()
                .Bind(Upload);


            Result Validate() =>
                Path.GetExtension(file?.FileName)?.ToLower() == "pdf"
                    ? Result.Success()
                    : Result.Failure("The file must have extension '.pdf'");


            async Task<Result> Upload()
            {
                var key = $"{_s3FolderName}/{counterpartyId}";

                await using var stream = file.OpenReadStream();
                var (_, isFailure, _, error) = await _amazonS3ClientService.Add(_bucketName, key, stream, S3CannedACL.Private);

                return isFailure
                    ? Result.Failure(error)
                    : Result.Success();
            }
        }


        private readonly string _s3FolderName;
        private readonly string _bucketName;
        private readonly IAmazonS3ClientService _amazonS3ClientService;
    }
}
