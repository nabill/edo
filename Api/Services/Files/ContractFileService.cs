using System;
using System.IO;
using System.Threading.Tasks;
using Amazon.S3;
using CSharpFunctionalExtensions;
using HappyTravel.AmazonS3Client.Options;
using HappyTravel.AmazonS3Client.Services;
using HappyTravel.Edo.Data.Management;
using Microsoft.AspNetCore.Http;

namespace HappyTravel.Edo.Api.Services.Files
{
    public class ContractFileService : IContractFileService
    {


        public ContractFileService(IAmazonS3ClientService amazonS3ClientService,
            ioptions)
        {
            _amazonS3ClientService = amazonS3ClientService;
        }


        public async Task<Result> Save(int counterpartyId, IFormFile file)
        {
            return await Validate()
                .Bind(Upload);


            Result Validate()
            {
                return Path.GetExtension(file?.FileName)?.ToLower() == "pdf"
                    ? Result.Success()
                    : Result.Failure("The file must have extension '.pdf'");
            }


            async Task<Result> Upload()
            {
                var key = $"{S3FolderName}/{counterpartyId}";

                await using var stream = file.OpenReadStream();
                var (_, isFailure, _, error) = await _amazonS3ClientService.Add(_bucketName, key, stream, S3CannedACL.Private);

                if (isFailure)
                    return Result.Failure(error);

                return Result.Success();
            }
        }


        private const string S3FolderName = "contracts";

        private readonly string _bucketName = ;

        private readonly IAmazonS3ClientService _amazonS3ClientService;
    }
}
