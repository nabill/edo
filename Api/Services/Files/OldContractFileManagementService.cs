using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Amazon.S3;
using CSharpFunctionalExtensions;
using HappyTravel.AmazonS3Client.Services;
using HappyTravel.Edo.Api.Infrastructure.Options;
using HappyTravel.Edo.Data;
using HappyTravel.Edo.Data.Agents;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace HappyTravel.Edo.Api.Services.Files
{
    public class OldContractFileManagementService : IOldContractFileManagementService
    {
        // TODO: remove this service when old contracts are transferred to agencies. https://github.com/happy-travel/agent-app-project/issues/786
        public OldContractFileManagementService(IAmazonS3ClientService amazonS3ClientService,
            EdoContext context,
            IOptions<ContractFileServiceOptions> options,
            IContractFileManagementService contractFileManagementService)
        {
            _amazonS3ClientService = amazonS3ClientService;
            _context = context;
            _bucketName = options.Value.Bucket;
            _contractFileManagementService = contractFileManagementService;
        }


        public async Task<string> ReuploadToAgencies()
        {
            var uploadedCounterparties = await _context.Counterparties
                .Where(c => c.IsContractUploaded)
                .ToListAsync();

            var errorsSb = new StringBuilder("errors:\r\n");

            foreach (var counterparty in uploadedCounterparties)
            {
                try
                {
                    // Get root agency
                    var rootAgency = await _context.Agencies.SingleOrDefaultAsync(a => a.CounterpartyId == counterparty.Id && a.ParentId == null);
                    if (rootAgency is null)
                    {
                        errorsSb.AppendLine($"Could not find a root agency for counterparty (Id {counterparty.Id})");
                        continue;
                    }
                    
                    // Download from the old place
                    var (_, isGetFailure, (contractStream, _), getError) = await Get(counterparty.Id);
                    if (isGetFailure)
                    {
                        errorsSb.AppendLine($"Error while downloading contract file, counterparty Id {counterparty.Id}: {getError}");
                        continue;
                    }

                    // Upload to a new place
                    var fileToUpload = new FormFile(contractStream, 0, contractStream.Length, "Contract.pdf", "Contract.pdf");
                    var (_, isUploadFailure, uploadError) = await _contractFileManagementService.Add(rootAgency.Id, fileToUpload);
                    if (isUploadFailure)
                    {
                        errorsSb.AppendLine($"Error while downloading contract file, counterparty Id {counterparty.Id}" +
                            $", rootAgencyId {rootAgency.Id}: {uploadError}");
                    }
                }
                catch (Exception e)
                {
                    errorsSb.AppendLine($"{e.GetType().Name} while processing contract, counterparty Id {counterparty.Id}: {e.Message}");
                }
            }

            return errorsSb.ToString();
        }


        public async Task<Result> Add(int counterpartyId, IFormFile file)
        {
            return await ValidateFile()
                .Bind(() => GetCounterpartyRecord(counterpartyId))
                .Check(Upload)
                .Bind(UpdateCounterparty);


            Result ValidateFile() =>
                Result.Success()
                    .Ensure(() => file != null, "Couldn't get any file")
                    .Ensure(() => file.Length > 0, "Got an empty file")
                    .Ensure(() => Path.GetExtension(file?.FileName)?.ToLower() == PdfFileExtension, $"The file must have extension '{PdfFileExtension}'");


            async Task<Result> Upload(Counterparty _)
            {
                await using var stream = file.OpenReadStream();
                return await _amazonS3ClientService.Add(_bucketName, GetKey(counterpartyId), stream, S3CannedACL.Private);
            }


            async Task<Result> UpdateCounterparty(Counterparty counterparty)
            {
                counterparty.IsContractUploaded = true;
                _context.Entry(counterparty).Property(i => i.IsContractUploaded).IsModified = true;
                await _context.SaveChangesAsync();
                return Result.Success();
            }
        }


        public Task<Result<(Stream stream, string contentType)>> Get(int counterpartyId)
        {
            return GetCounterpartyRecord(counterpartyId)
                .Ensure(c => c.IsContractUploaded, "No contract file was uploaded")
                .Bind(GetContractFileStream);


            async Task<Result<(Stream stream, string contentType)>> GetContractFileStream(Counterparty _)
            {
                var (_, isFailure, stream, _) = await _amazonS3ClientService.Get(_bucketName, GetKey(counterpartyId));

                if (isFailure)
                    return Result.Failure<(Stream, string)>("Couldn't get a contract file");

                return (stream, PdfContentType);
            }
        }


        private string GetKey(int counterpartyId)
            => $"{OldContractsS3FolderName}/{counterpartyId}.pdf";


        private async Task<Result<Counterparty>> GetCounterpartyRecord(int counterpartyId)
        {
            var counterparty = await _context.Counterparties.FindAsync(counterpartyId);
            if (counterparty is null)
                return Result.Failure<Counterparty>($"Could not find counterparty with id {counterpartyId}");

            return counterparty;
        }


        private const string PdfFileExtension = ".pdf";
        private const string PdfContentType = "application/pdf";
        private const string OldContractsS3FolderName = "contracts";
        
        private readonly string _bucketName;
        private readonly IAmazonS3ClientService _amazonS3ClientService;
        private readonly EdoContext _context;
        private readonly IContractFileManagementService _contractFileManagementService;
    }
}
