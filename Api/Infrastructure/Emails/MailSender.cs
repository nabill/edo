using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using HappyTravel.Edo.Api.Infrastructure.Constants;
using HappyTravel.Edo.Api.Infrastructure.Logging;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SendGrid;
using SendGrid.Helpers.Mail;

namespace HappyTravel.Edo.Api.Infrastructure.Emails
{
    public class MailSender : IMailSender
    {
        public MailSender(IOptions<SenderOptions> senderOptions,
            ILogger<MailSender> logger,
            IHttpClientFactory httpClientFactory)
        {
            _senderOptions = senderOptions.Value;
            _logger = logger;
            _httpClientFactory = httpClientFactory;
        }

        public async Task<Result> Send<TMessageData>(string templateId, string recipientAddress, TMessageData messageData)
        {
            var client = new SendGridClient(_httpClientFactory.CreateClient(HttpClientNames.SendGrid), _senderOptions.ApiKey);
            var message = new SendGridMessage
            {
                TemplateId = templateId,
                From = _senderOptions.SenderAddress
            };
            message.SetTemplateData(messageData);
            message.AddTo(recipientAddress);

            try
            {
                var response = await client.SendEmailAsync(message);
                if (response.StatusCode != HttpStatusCode.Accepted)
                {
                    var error = await response.Body.ReadAsStringAsync();
                    throw new Exception($"Could not send e-mail to {recipientAddress}, server responded: '{error}' with status code '{response.StatusCode}'");
                }
               
                _logger.LogSendMailInformation($"Successfully e-mail to {recipientAddress}");
                return Result.Ok();
            }
            catch (Exception ex)
            {
                _logger.LogSendMailException(ex);
                return Result.Fail("Could not send e-mail");
            }
        }
        
        private readonly ILogger<MailSender> _logger;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly SenderOptions _senderOptions;
    }
}