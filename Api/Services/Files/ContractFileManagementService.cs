using System.IO;
using System.Threading.Tasks;
using Amazon.S3;
using CSharpFunctionalExtensions;
using HappyTravel.AmazonS3Client.Services;
using HappyTravel.Edo.Api.Infrastructure.Options;
using HappyTravel.Edo.Data;
using HappyTravel.Edo.Data.Agents;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;

namespace HappyTravel.Edo.Api.Services.Files
{
    public class ContractFileManagementService : IContractFileManagementService
    {
        public ContractFileManagementService(IAmazonS3ClientService amazonS3ClientService,
            EdoContext context,
            IOptions<ContractFileServiceOptions> options)
        {
            _amazonS3ClientService = amazonS3ClientService;
            _context = context;
            _bucketName = options.Value.Bucket;
            _s3FolderName = options.Value.S3FolderName;
        }


        public async Task<Result> Add(int agencyId, IFormFile file)
        {
            return await ValidateFile()
                .Bind(() => GetAgencyRecord(agencyId))
                .Check(Upload)
                .Bind(UpdateAgency);


            Result ValidateFile() =>
                Result.Success()
                    .Ensure(() => file != null, "Couldn't get any file")
                    .Ensure(() => file.Length > 0, "Got an empty file")
                    .Ensure(() => Path.GetExtension(file?.FileName)?.ToLower() == PdfFileExtension, $"The file must have extension '{PdfFileExtension}'");


            async Task<Result> Upload(Agency _)
            {
                await using var stream = file.OpenReadStream();
                return await _amazonS3ClientService.Add(_bucketName, GetKey(agencyId), stream, S3CannedACL.Private);
            }


            async Task<Result> UpdateAgency(Agency agency)
            {
                agency.IsContractUploaded = true;
                _context.Entry(agency).Property(i => i.IsContractUploaded).IsModified = true;
                await _context.SaveChangesAsync();
                return Result.Success();
            }
        }


        public Task<Result<(Stream stream, string contentType)>> Get(int agencyId)
        {
            return GetAgencyRecord(agencyId)
                .Ensure(a => a.IsContractUploaded, "No contract file was uploaded")
                .Bind(GetContractFileStream);


            async Task<Result<(Stream stream, string contentType)>> GetContractFileStream(Agency _)
            {
                var (_, isFailure, stream, _) = await _amazonS3ClientService.Get(_bucketName, GetKey(agencyId));

                if (isFailure)
                    return Result.Failure<(Stream, string)>("Couldn't get a contract file");

                return (stream, PdfContentType);
            }
        }


        private string GetKey(int agencyId) 
            => $"{_s3FolderName}/{agencyId}.pdf";


        private async Task<Result<Agency>> GetAgencyRecord(int agencyId)
        {
            var agency = await _context.Agencies.FindAsync(agencyId);
            if (agency is null)
                return Result.Failure<Agency>($"Could not find agency with id {agencyId}");

            return agency;
        }


        private const string PdfFileExtension = ".pdf";
        private const string PdfContentType = "application/pdf";

        private readonly string _s3FolderName;
        private readonly string _bucketName;
        private readonly IAmazonS3ClientService _amazonS3ClientService;
        private readonly EdoContext _context;
    }
}
