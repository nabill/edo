using System.Threading.Tasks;
using CSharpFunctionalExtensions;

namespace HappyTravel.Edo.Api.AdministratorServices;

public interface IAdministratorBookingDocumentsService
{
    Task<Result<byte[]>> GenerateVoucherPdf(int bookingId, string languageCode);
    Task<Result> SendVoucherPdf(int bookingId, string email, string languageCode);
}