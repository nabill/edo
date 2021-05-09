using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using HappyTravel.Edo.Api.Infrastructure.Constants;
using IdentityModel.Client;

namespace HappyTravel.Edo.Api.Infrastructure
{
    public class IdentityUserInfoService : IIdentityUserInfoService
    {
        public IdentityUserInfoService(ITokenInfoAccessor tokenInfoAccessor,
            IHttpClientFactory httpClientFactory)
        {
            _tokenInfoAccessor = tokenInfoAccessor;
            _httpClientFactory = httpClientFactory;
        }


        public async Task<string> GetUserEmail()
        {
            using var identityClient = _httpClientFactory.CreateClient(HttpClientNames.Identity);

            var doc = await identityClient.GetDiscoveryDocumentAsync();
            var token = await _tokenInfoAccessor.GetAccessToken();

            return (await identityClient.GetUserInfoAsync(new UserInfoRequest { Token = token, Address = doc.UserInfoEndpoint }))
                .Claims
                .SingleOrDefault(c => c.Type == "email")
                ?.Value;
        }


        private readonly ITokenInfoAccessor _tokenInfoAccessor;
        private readonly IHttpClientFactory _httpClientFactory;
    }
}