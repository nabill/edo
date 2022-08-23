using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using HappyTravel.Edo.Api.Models.Bookings.Vouchers;
using HappyTravel.Edo.Api.Services.Accommodations.Bookings.Documents;
using HappyTravel.Edo.Api.Services.Accommodations.Bookings.Mailing;
using HappyTravel.Edo.Api.Services.Accommodations.Bookings.Management;
using HappyTravel.Edo.PdfGenerator;

namespace HappyTravel.Edo.Api.AdministratorServices
{
    public class AdministratorBookingDocumentsService : IAdministratorBookingDocumentsService
    {
        public AdministratorBookingDocumentsService(IBookingDocumentsMailingService mailingService,
            IBookingDocumentsService documentsService,
            IBookingRecordManager recordManager,
            IPdfGeneratorService pdfGeneratorService)
        {
            _documentsService = documentsService;
            _recordManager = recordManager;
            _mailingService = mailingService;
            _pdfGeneratorService = pdfGeneratorService;
        }

        public Task<Result<byte[]>> GenerateVoucherPdf(int bookingId, string languageCode)
            => _recordManager.Get(bookingId)
                .Bind(b => _documentsService.GenerateVoucher(b, languageCode))
                .Bind(data => _pdfGeneratorService.Generate(data));


        public Task<Result> SendVoucherPdf(int bookingId, string email, string languageCode)
        {
            return _recordManager.Get(bookingId)
                .Bind(b => _documentsService.GenerateVoucher(b, languageCode))
                .Bind(data => GeneratePdfAndSend(data));


            async Task<Result> GeneratePdfAndSend(BookingVoucherData data)
            {
                var (_, isFailureGenerate, voucherPdf, errorGenerate) = await _pdfGeneratorService.Generate(data);
                if (isFailureGenerate)
                    return Result.Failure(errorGenerate);

                var (_, isFailureSend, errorSend) = await _mailingService.SendVoucherPdf(data, voucherPdf, email);
                if (isFailureSend)
                    return Result.Failure(errorSend);

                return Result.Success();
            }

        }


        private readonly IBookingDocumentsService _documentsService;
        private readonly IBookingRecordManager _recordManager;
        private readonly IBookingDocumentsMailingService _mailingService;
        private readonly IPdfGeneratorService _pdfGeneratorService;
    }
}