using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using FluentValidation;
using HappyTravel.Edo.Api.Extensions;
using HappyTravel.Edo.Api.Infrastructure;
using HappyTravel.Edo.Api.Models.Agents;
using HappyTravel.Edo.Api.Models.Payments;
using HappyTravel.Edo.Api.Services.Agents;
using HappyTravel.Edo.Common.Enums;
using HappyTravel.Edo.Data;
using HappyTravel.EdoContracts.General.Enums;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json.Linq;

namespace HappyTravel.Edo.Api.Services.Payments
{
    public class PaymentHistoryService : IPaymentHistoryService
    {
        public PaymentHistoryService(EdoContext edoContext)
        {
            _edoContext = edoContext;
        }


        public Result<IQueryable<PaymentHistoryData>> GetAgentHistory(PaymentHistoryRequest paymentHistoryRequest, AgentContext agent)
        {
            var validationResult = Validate(paymentHistoryRequest);
            if (validationResult.IsFailure)
                return Result.Failure<IQueryable<PaymentHistoryData>>(validationResult.Error);

            var query = from booking in _edoContext.Bookings
                join accountAuditLogEntry in _edoContext.AccountBalanceAuditLogs
                    on booking.ReferenceCode equals accountAuditLogEntry.ReferenceCode into join1
                from accountAuditLogEntry in join1.DefaultIfEmpty()
                join cardAuditLogEntry in _edoContext.CreditCardAuditLogs
                    on booking.ReferenceCode equals cardAuditLogEntry.ReferenceCode into join2
                from cardAuditLogEntry in join2.DefaultIfEmpty()
                where
                    booking.AgentId == agent.AgentId &&
                    accountAuditLogEntry.UserId == agent.AgentId &&
                    accountAuditLogEntry.UserType == UserTypes.Agent &&
                    accountAuditLogEntry.Created <= paymentHistoryRequest.ToDate &&
                    accountAuditLogEntry.Created >= paymentHistoryRequest.FromDate ||
                    cardAuditLogEntry.UserId == agent.AgentId &&
                    cardAuditLogEntry.UserType == UserTypes.Agent &&
                    cardAuditLogEntry.Created <= paymentHistoryRequest.ToDate &&
                    cardAuditLogEntry.Created >= paymentHistoryRequest.FromDate
                let isAccountLogEntry = accountAuditLogEntry != null && cardAuditLogEntry == null
                let eventType = isAccountLogEntry ? ToPaymentHistoryType(accountAuditLogEntry.Type) : ToPaymentHistoryType(cardAuditLogEntry.Type)
                select new PaymentHistoryData
                {
                    Created = isAccountLogEntry ? accountAuditLogEntry.Created : cardAuditLogEntry.Created,
                    Amount = isAccountLogEntry ? accountAuditLogEntry.Amount : cardAuditLogEntry.Amount,
                    EventData = JObject.Parse(isAccountLogEntry ? accountAuditLogEntry.EventData : cardAuditLogEntry.EventData),
                    Currency = booking.Currency,
                    AgentId = booking.AgentId,
                    EventType = eventType,
                    PaymentMethod = isAccountLogEntry ? PaymentMethods.BankTransfer : PaymentMethods.CreditCard,
                    AccommodationName = booking.AccommodationName,
                    LeadingPassenger = booking.MainPassengerName,
                    BookingId = booking.Id,
                    ReferenceCode = booking.ReferenceCode
                };

            return Result.Success(query);
        }


        public Result<IQueryable<PaymentHistoryData>> GetAgencyHistory(PaymentHistoryRequest paymentHistoryRequest, AgentContext agent)
        {
            var validationResult = Validate(paymentHistoryRequest);
            if (validationResult.IsFailure)
                return Result.Failure<IQueryable<PaymentHistoryData>>(validationResult.Error);

            var query = from booking in _edoContext.Bookings
                join accountAuditLogEntry in _edoContext.AccountBalanceAuditLogs
                    on booking.ReferenceCode equals accountAuditLogEntry.ReferenceCode into join1
                from accountAuditLogEntry in join1.DefaultIfEmpty()
                join cardAuditLogEntry in _edoContext.CreditCardAuditLogs
                    on booking.ReferenceCode equals cardAuditLogEntry.ReferenceCode into join2
                from cardAuditLogEntry in join2.DefaultIfEmpty()
                where
                    booking.AgencyId == agent.AgencyId &&
                    accountAuditLogEntry.UserId == agent.AgentId &&
                    accountAuditLogEntry.UserType == UserTypes.Agent &&
                    accountAuditLogEntry.Created <= paymentHistoryRequest.ToDate &&
                    accountAuditLogEntry.Created >= paymentHistoryRequest.FromDate ||
                    cardAuditLogEntry.UserId == agent.AgentId &&
                    cardAuditLogEntry.UserType == UserTypes.Agent &&
                    cardAuditLogEntry.Created <= paymentHistoryRequest.ToDate &&
                    cardAuditLogEntry.Created >= paymentHistoryRequest.FromDate
                let isAccountLogEntry = accountAuditLogEntry != null && cardAuditLogEntry == null
                let eventType = isAccountLogEntry ? ToPaymentHistoryType(accountAuditLogEntry.Type) : ToPaymentHistoryType(cardAuditLogEntry.Type)
                select new PaymentHistoryData
                {
                    Created = isAccountLogEntry ? accountAuditLogEntry.Created : cardAuditLogEntry.Created,
                    Amount = isAccountLogEntry ? accountAuditLogEntry.Amount : cardAuditLogEntry.Amount,
                    EventData = JObject.Parse(isAccountLogEntry ? accountAuditLogEntry.EventData : cardAuditLogEntry.EventData),
                    Currency = booking.Currency,
                    AgentId = booking.AgentId,
                    EventType = eventType,
                    PaymentMethod = isAccountLogEntry ? PaymentMethods.BankTransfer : PaymentMethods.CreditCard,
                    AccommodationName = booking.AccommodationName,
                    LeadingPassenger = booking.MainPassengerName,
                    BookingId = booking.Id,
                    ReferenceCode = booking.ReferenceCode
                };

            return Result.Success(query);
        }


        private Result Validate(PaymentHistoryRequest paymentHistoryRequest)
        {
            return GenericValidator<PaymentHistoryRequest>.Validate(setup =>
            {
                setup.RuleFor(i => i.ToDate)
                    .GreaterThanOrEqualTo(request => request.FromDate)
                    .WithMessage($"{nameof(paymentHistoryRequest.ToDate)} must be greater then {nameof(paymentHistoryRequest.FromDate)}");
                setup.RuleFor(i => (i.ToDate - i.FromDate).Days)
                    .LessThanOrEqualTo(MaxRequestDaysNumber)
                    .WithMessage(
                        $"Total days between {nameof(paymentHistoryRequest.FromDate)} and {nameof(paymentHistoryRequest.ToDate)} should be less or equal {MaxRequestDaysNumber}");
            }, paymentHistoryRequest);
        }


        private static PaymentHistoryType ToPaymentHistoryType(AccountEventType type)
        {
            switch (type)
            {
                case AccountEventType.None: return PaymentHistoryType.None;
                case AccountEventType.Add: return PaymentHistoryType.Add;
                case AccountEventType.Charge: return PaymentHistoryType.Charge;
                case AccountEventType.Authorize: return PaymentHistoryType.Authorize;
                case AccountEventType.Capture: return PaymentHistoryType.Capture;
                case AccountEventType.Void: return PaymentHistoryType.Void;
                case AccountEventType.Refund: return PaymentHistoryType.Refund;
                case AccountEventType.CounterpartyTransferToAgency: return PaymentHistoryType.Add;
                default:
                    throw new ArgumentOutOfRangeException(nameof(type), type, null);
            }
        }


        private static PaymentHistoryType ToPaymentHistoryType(CreditCardEventType type)
        {
            switch (type)
            {
                case CreditCardEventType.Authorize: return PaymentHistoryType.Authorize;
                case CreditCardEventType.Capture: return PaymentHistoryType.Capture;
                case CreditCardEventType.Void: return PaymentHistoryType.None;
                case CreditCardEventType.None: return PaymentHistoryType.None;
                default:
                    throw new ArgumentOutOfRangeException(nameof(type), type, null);
            }
        }


        private const int MaxRequestDaysNumber = 3650;

        private readonly EdoContext _edoContext;
    }
}