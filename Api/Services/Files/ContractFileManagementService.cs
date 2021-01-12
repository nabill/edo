using System.IO;
using System.Threading.Tasks;
using Amazon.S3;
using CSharpFunctionalExtensions;
using HappyTravel.AmazonS3Client.Services;
using HappyTravel.Edo.Api.Infrastructure.Options;
using HappyTravel.Edo.Data;
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


        public async Task<Result> Add(int counterpartyId, IFormFile file)
        {
            return await ValidateFile()
                .Bind(Upload)
                .Bind(UpdateCounterparty);


            Result ValidateFile() =>
                Result.Success()
                    .Ensure(() => file != null, "Couldn't get any file")
                    .Ensure(() => file.Length > 0, "Got an empty file")
                    .Ensure(() => Path.GetExtension(file?.FileName)?.ToLower() == PdfFileExtension, $"The file must have extension '{PdfFileExtension}'");


            async Task<Result> Upload()
            {
                await using var stream = file.OpenReadStream();
                return await _amazonS3ClientService.Add(_bucketName, GetKey(counterpartyId), stream, S3CannedACL.Private);
            }


            async Task<Result> UpdateCounterparty()
            {
                var counterparty = await _context.Counterparties.FindAsync(counterpartyId);
                if (counterparty is null)
                    return Result.Failure($"Could not find counterparty with id {counterpartyId}");

                counterparty.HasContract = true;
                _context.Update(counterparty);
                await _context.SaveChangesAsync();
                return Result.Success();
            }
        }


        public async Task<Result<(Stream stream, string contentType)>> Get(int counterpartyId)
        {
            var (_, isFailure, stream, _) = await _amazonS3ClientService.Get(_bucketName, GetKey(counterpartyId));

            if (isFailure)
                return Result.Failure<(Stream, string)>("Couldn't get a contract file");

            return (stream, PdfContentType);
        }


        private string GetKey(int counterpartyId) 
            => $"{_s3FolderName}/{counterpartyId}.pdf";


        private const string PdfFileExtension = ".pdf";
        private const string PdfContentType = "application/pdf";

        private readonly string _s3FolderName;
        private readonly string _bucketName;
        private readonly IAmazonS3ClientService _amazonS3ClientService;
        private readonly EdoContext _context;
    }
}
