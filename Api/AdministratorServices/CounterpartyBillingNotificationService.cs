using System;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using HappyTravel.Edo.Api.Infrastructure.Logging;
using HappyTravel.Edo.Api.Infrastructure.Options;
using HappyTravel.Edo.Api.Models.Mailing;
using HappyTravel.Edo.Api.Models.Payments;
using HappyTravel.Edo.Api.Services.Agents;
using HappyTravel.DataFormatters;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using HappyTravel.Edo.Notifications.Enums;
using HappyTravel.Edo.Api.NotificationCenter.Services;
using HappyTravel.Edo.Api.Models.Agents;

namespace HappyTravel.Edo.Api.AdministratorServices
{
    public class CounterpartyBillingNotificationService : ICounterpartyBillingNotificationService
    {
        public CounterpartyBillingNotificationService(INotificationService notificationService,
            Services.Agents.IAgentService agentService,
            ICounterpartyService counterpartyService,
            ILogger<CounterpartyBillingNotificationService> logger,
            IOptions<CounterpartyBillingNotificationServiceOptions> options)
        {
            _notificationService = notificationService;
            _agentService = agentService;
            _counterpartyService = counterpartyService;
            _logger = logger;
            _options = options.Value;
        }


        public Task NotifyAdded(int counterpartyId, PaymentData paymentData)
        {
            return GetEmailAndAgent()
                .Bind(SendNotification)
                .OnFailure(LogNotificationFailure);


            async Task<Result<(string, SlimAgentContext)>> GetEmailAndAgent()
            {
                var rootAgency = await _counterpartyService.GetRootAgency(counterpartyId);

                var (_, isFailure, agent, error) = await _agentService.GetMasterAgent(rootAgency.Id);
                if (isFailure)
                    return Result.Failure<(string, SlimAgentContext)>(error);

                var slimAgent = new SlimAgentContext(agent.Id, rootAgency.Id);

                var email = string.IsNullOrWhiteSpace(rootAgency.BillingEmail)
                    ? agent.Email
                    : rootAgency.BillingEmail;

                return (email, slimAgent);
            }


            async Task<Result> SendNotification((string, SlimAgentContext) receiver)
            {
                var payload = new CounterpartyAccountAddedNotificationData
                {
                    Amount = MoneyFormatter.ToCurrencyString(paymentData.Amount, paymentData.Currency)
                };

                var (email, agent) = receiver;

                return await _notificationService.Send(agent: agent,
                    messageData: payload,
                    notificationType: NotificationTypes.AccountBalanceReplenished,
                    email: email,
                    templateId: _options.CounterpartyAccountAddedTemplateId);
            }


            void LogNotificationFailure(string error) => _logger.LogCounterpartyAccountAddedNotificationFailure(error);
        }


        private readonly INotificationService _notificationService;
        private readonly Services.Agents.IAgentService _agentService;
        private readonly ICounterpartyService _counterpartyService;
        private readonly ILogger<CounterpartyBillingNotificationService> _logger;
        private readonly CounterpartyBillingNotificationServiceOptions _options;
    }
}