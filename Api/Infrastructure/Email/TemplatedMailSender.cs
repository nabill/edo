using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using HappyTravel.Edo.Api.Infrastructure.Constants;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SendGrid;
using SendGrid.Helpers.Mail;

namespace HappyTravel.Edo.Api.Infrastructure.Email
{
    public class TemplatedMailSender : ITemplatedMailSender
    {
        public TemplatedMailSender(IOptions<SenderOptions> senderOptions,
            ILogger<TemplatedMailSender> logger,
            IHttpClientFactory httpClientFactory)
        {
            _senderOptions = senderOptions.Value;
            _logger = logger;
            _httpClientFactory = httpClientFactory;
        }

        public async Task<Result> Send<TMessageData>(string templateId, EmailAddress emailTo, TMessageData messageData)
        {
            var client = new SendGridClient(_httpClientFactory.CreateClient(HttpClientNames.SendGrid), _senderOptions.ApiKey);
            var message = new SendGridMessage
            {
                TemplateId = templateId,
                From = _senderOptions.SenderAddress
            };
            message.SetTemplateData(messageData);
            message.AddTo(emailTo);

            try
            {
                var response = await client.SendEmailAsync(message);
                if (response.StatusCode == HttpStatusCode.Accepted)
                {
                    _logger.LogInformation($"Successfully e-mail to {emailTo}");
                    return Result.Ok();
                }
                else
                {
                    var error = await response.Body.ReadAsStringAsync();
                    _logger.LogError(
                        $"Could not send e-mail to {emailTo}, server responded: '{error}' with status code '{response.StatusCode}'");

                    return Result.Fail(error);
                }
            }
            catch (Exception e)
            {
                return Result.Fail(e.Message);
            }
        }
        
        private readonly ILogger<TemplatedMailSender> _logger;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly SenderOptions _senderOptions;
    }
}