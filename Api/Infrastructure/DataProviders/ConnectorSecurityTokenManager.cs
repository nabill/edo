using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using HappyTravel.Edo.Api.Infrastructure.Constants;
using HappyTravel.Edo.Api.Infrastructure.Logging;
using IdentityModel.Client;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace HappyTravel.Edo.Api.Infrastructure.DataProviders
{
    public class ConnectorSecurityTokenManager : IConnectorSecurityTokenManager
    {
        public ConnectorSecurityTokenManager(IHttpClientFactory clientFactory,
            IDateTimeProvider dateTimeProvider,
             IOptions<TokenRequestSettings> tokenRequestSettings,
            ILogger<ConnectorSecurityTokenManager> logger)
        {
            _clientFactory = clientFactory;
            _dateTimeProvider = dateTimeProvider;
            _logger = logger;
            _tokenRequestSettings = tokenRequestSettings.Value;
        }


        public async Task Refresh()
        {
            // If someone refreshes token right now, there is no need to refresh it again.
            if(_refreshTokenSemaphore.CurrentCount == 0)
                return;

            await _refreshTokenSemaphore.WaitAsync();
            try
            {
                var now = _dateTimeProvider.UtcNow();
                using var client = _clientFactory.CreateClient(HttpClientNames.Identity);

                var tokenResponse = await client.RequestClientCredentialsTokenAsync(new ClientCredentialsTokenRequest
                {
                    Address = _tokenRequestSettings.Address,
                    Scope = _tokenRequestSettings.Scope,
                    ClientId = _tokenRequestSettings.ClientId,
                    ClientSecret = _tokenRequestSettings.ClientSecret,
                    GrantType = _tokenRequestSettings.GrantType
                });

                if (tokenResponse.IsError)
                {
                    var errorMessage = $"Something went wrong while requesting the access token. Error: {tokenResponse.Error}. " +
                        $"Using existing token: '{_tokenInfo.Token}' with expiry date '{_tokenInfo.ExpiryDate}'";

                    _logger.LogGetTokenForConnectorError(errorMessage);
                }
                else
                {
                    _tokenInfo = (tokenResponse.AccessToken, now.AddSeconds(tokenResponse.ExpiresIn));
                }
            }
            finally
            {
                _refreshTokenSemaphore.Release();
            }
        }


        public async Task<string> Get()
        {
            try
            {
                var now = _dateTimeProvider.UtcNow();
                await _getTokenSemaphore.WaitAsync();
                // We need to cache token because users can send several requests in short periods.
                // Covered situation when after checking expireDate token will expire immediately.
                if (_tokenInfo.Equals(default) || (_tokenInfo.ExpiryDate - now).TotalSeconds <= 5)
                    await Refresh();

                return _tokenInfo.Token;
            }
            finally
            {
                _getTokenSemaphore.Release();
            }
        }
        
        private readonly SemaphoreSlim _getTokenSemaphore = new SemaphoreSlim(1, 1);
        private readonly SemaphoreSlim _refreshTokenSemaphore = new SemaphoreSlim(1, 1);
        private readonly IHttpClientFactory _clientFactory;
        private readonly IDateTimeProvider _dateTimeProvider;
        private readonly ILogger<ConnectorSecurityTokenManager> _logger;
        private (string Token, DateTime ExpiryDate) _tokenInfo;
        private readonly TokenRequestSettings _tokenRequestSettings;
    }
}