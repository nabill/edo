using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using HappyTravel.Edo.Api.Models.Payments;
using HappyTravel.Edo.Data.Customers;

namespace HappyTravel.Edo.Api.Services.Payments
{
    public interface ITokenizationService
    {
        Task<Result<GetTokenResponse>> GetToken(GetTokenRequest request, Customer customer, Company company);
        Task<Result<GetTokenResponse>> GetOneTimeToken(GetOneTimeTokenRequest request, string languageCode, Customer customer);
        Result<StoredTokenInfo> GetStoredToken(string tokenId, Customer customer);
    }
}
