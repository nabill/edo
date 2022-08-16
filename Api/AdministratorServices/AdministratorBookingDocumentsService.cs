using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using HappyTravel.Edo.Api.Services.Accommodations.Bookings.Documents;
using HappyTravel.Edo.Api.Services.Accommodations.Bookings.Management;
using HappyTravel.Edo.PdfGenerator;

namespace HappyTravel.Edo.Api.AdministratorServices
{
    public class AdministratorBookingDocumentsService : IAdministratorBookingDocumentsService
    {
        public AdministratorBookingDocumentsService(
            IBookingDocumentsService documentsService,
            IBookingRecordManager recordManager,
            IPdfGeneratorService pdfGeneratorService)
        {
            _documentsService = documentsService;
            _recordManager = recordManager;
            _pdfGeneratorService = pdfGeneratorService;
        }

        public Task<Result<byte[]>> GenerateVoucherPdf(int bookingId, string languageCode)
            => _recordManager.Get(bookingId)
                .Bind(b => _documentsService.GenerateVoucher(b, languageCode))
                .Bind(data => _pdfGeneratorService.Generate(data));


        private readonly IBookingDocumentsService _documentsService;
        private readonly IBookingRecordManager _recordManager;
        private readonly IPdfGeneratorService _pdfGeneratorService;
    }
}