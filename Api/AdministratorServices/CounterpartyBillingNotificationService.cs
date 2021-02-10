using System;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using HappyTravel.Edo.Api.Infrastructure;
using HappyTravel.Edo.Api.Infrastructure.Options;
using HappyTravel.Edo.Api.Models.Mailing;
using HappyTravel.Edo.Api.Models.Payments;
using HappyTravel.Edo.Api.Services.Agents;
using HappyTravel.Formatters;
using Microsoft.Extensions.Options;

namespace HappyTravel.Edo.Api.AdministratorServices
{
    public class CounterpartyBillingNotificationService : ICounterpartyBillingNotificationService
    {
        public CounterpartyBillingNotificationService(MailSenderWithCompanyInfo mailSender,
            Services.Agents.IAgentService agentService,
            ICounterpartyService counterpartyService,
            IOptions<CounterpartyBillingNotificationServiceOptions> options)
        {
            _mailSender = mailSender;
            _agentService = agentService;
            _counterpartyService = counterpartyService;
            _options = options.Value;
        }


        public Task<Result> NotifyAdded(int counterpartyId, PaymentData paymentData)
        {
            return GetEmail()
                .Bind(SendNotification);


            async Task<Result<string>> GetEmail()
            {
                var (_, isFailure, counterpartyInfo, error) = await _counterpartyService.Get(counterpartyId);
                if (isFailure)
                    return Result.Failure<string>(error);

                if (!string.IsNullOrWhiteSpace(counterpartyInfo.BillingEmail))
                    return counterpartyInfo.BillingEmail;

                var defaultAgency = await _counterpartyService.GetDefaultAgency(counterpartyId);
                return await _agentService.GetMasterAgent(defaultAgency.Id)
                    .Map(master => master.Email);
            }


            Task<Result> SendNotification(string email)
            {
                var payload = new CounterpartyAccountAddedNotificationData
                {
                    Amount = MoneyFormatter.ToCurrencyString(paymentData.Amount, paymentData.Currency)
                };

                return _mailSender.Send(_options.CounterpartyAccountAddedTemplateId, email, payload);
            }
        }


        private readonly MailSenderWithCompanyInfo _mailSender;
        private readonly Services.Agents.IAgentService _agentService;
        private readonly ICounterpartyService _counterpartyService;
        private readonly CounterpartyBillingNotificationServiceOptions _options;
    }
}
