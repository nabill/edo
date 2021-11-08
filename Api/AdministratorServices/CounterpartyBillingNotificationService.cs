using CSharpFunctionalExtensions;
using HappyTravel.DataFormatters;
using HappyTravel.Edo.Api.Infrastructure.Logging;
using HappyTravel.Edo.Api.Models.Agents;
using HappyTravel.Edo.Api.Models.Mailing;
using HappyTravel.Edo.Api.Models.Payments;
using HappyTravel.Edo.Api.NotificationCenter.Services;
using HappyTravel.Edo.Api.Services.Agents;
using HappyTravel.Edo.Notifications.Enums;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace HappyTravel.Edo.Api.AdministratorServices
{
    public class CounterpartyBillingNotificationService : ICounterpartyBillingNotificationService
    {
        public CounterpartyBillingNotificationService(INotificationService notificationService,
            Services.Agents.IAgentService agentService,
            ICounterpartyService counterpartyService,
            ILogger<CounterpartyBillingNotificationService> logger)
        {
            _notificationService = notificationService;
            _agentService = agentService;
            _counterpartyService = counterpartyService;
            _logger = logger;
        }


        public Task NotifyAdded(int counterpartyId, PaymentData paymentData)
        {
            return GetEmailAndAgent(counterpartyId)
                .Bind(receiver => SendNotification(receiver, paymentData, NotificationTypes.CounterpartyAccountBalanceReplenished))
                .OnFailure(LogNotificationFailure);


            void LogNotificationFailure(string error) => _logger.LogCounterpartyAccountAddedNotificationFailure(counterpartyId, error);
        }


        public Task NotifySubtracted(int counterpartyId, PaymentData paymentData)
        {
            return GetEmailAndAgent(counterpartyId)
                .Bind(receiver => SendNotification(receiver, paymentData, NotificationTypes.CounterpartyAccountBalanceSubtracted))
                .OnFailure(LogNotificationFailure);


            void LogNotificationFailure(string error) => _logger.LogCounterpartyAccountSubtractedNotificationFailure(counterpartyId, error);
        }


        public Task NotifyIncreasedManually(int counterpartyId, PaymentData paymentData)
        {
            return GetEmailAndAgent(counterpartyId)
                .Bind(receiver => SendNotification(receiver, paymentData, NotificationTypes.CounterpartyAccountBalanceIncreasedManually))
                .OnFailure(LogNotificationFailure);


            void LogNotificationFailure(string error) => _logger.LogCounterpartyAccountIncreasedManuallyNotificationFailure(counterpartyId, error);
        }


        public Task NotifyDecreasedManually(int counterpartyId, PaymentData paymentData)
        {
            return GetEmailAndAgent(counterpartyId)
                .Bind(receiver => SendNotification(receiver, paymentData, NotificationTypes.CounterpartyAccountBalanceDecreasedManually))
                .OnFailure(LogNotificationFailure);


            void LogNotificationFailure(string error) => _logger.LogCounterpartyAccountDecreasedManuallyNotificationFailure(counterpartyId, error);
        }


        private async Task<Result<(string, SlimAgentContext)>> GetEmailAndAgent(int counterpartyId)
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


        private async Task<Result> SendNotification((string, SlimAgentContext) receiver, PaymentData paymentData, NotificationTypes notificationType)
        {
            var payload = new CounterpartyAccountAddedNotificationData
            {
                Amount = MoneyFormatter.ToCurrencyString(paymentData.Amount, paymentData.Currency)
            };
            var (email, agent) = receiver;

            return await _notificationService.Send(agent: agent,
                messageData: payload,
                notificationType: notificationType,
                email: email);
        }


        private readonly INotificationService _notificationService;
        private readonly Services.Agents.IAgentService _agentService;
        private readonly ICounterpartyService _counterpartyService;
        private readonly ILogger<CounterpartyBillingNotificationService> _logger;
    }
}