using System;
using System.Linq;
using System.Linq.Expressions;
using CSharpFunctionalExtensions;
using FluentValidation;
using HappyTravel.Edo.Api.Infrastructure;
using HappyTravel.Edo.Api.Models.Agents;
using HappyTravel.Edo.Api.Models.Payments;
using HappyTravel.Edo.Common.Enums;
using HappyTravel.Edo.Data;
using HappyTravel.Edo.Data.Booking;
using HappyTravel.EdoContracts.General.Enums;
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
            var (_, isFailure, error) = Validate(paymentHistoryRequest);
            return isFailure
                ? Result.Failure<IQueryable<PaymentHistoryData>>(error)
                : Result.Success(GetData(paymentHistoryRequest, agent.AgentId, booking => booking.AgentId == agent.AgentId));
        }


        public Result<IQueryable<PaymentHistoryData>> GetAgencyHistory(PaymentHistoryRequest paymentHistoryRequest, AgentContext agent)
        {
            var (_, isFailure, error) = Validate(paymentHistoryRequest);
            return isFailure
                ? Result.Failure<IQueryable<PaymentHistoryData>>(error)
                : Result.Success(GetData(paymentHistoryRequest, agent.AgentId, booking => booking.AgencyId == agent.AgencyId));
        }


        private IQueryable<PaymentHistoryData> GetData(PaymentHistoryRequest paymentHistoryRequest, int agentId, Expression<Func<Booking, bool>> filterExpression)
        {
            var bookings = _edoContext.Bookings.Where(filterExpression);

            return from booking in bookings
                join accountAuditLogEntry in _edoContext.AccountBalanceAuditLogs
                    on booking.ReferenceCode equals accountAuditLogEntry.ReferenceCode into join1
                from accountAuditLogEntry in join1.DefaultIfEmpty()
                join cardAuditLogEntry in _edoContext.CreditCardAuditLogs
                    on booking.ReferenceCode equals cardAuditLogEntry.ReferenceCode into join2
                from cardAuditLogEntry in join2.DefaultIfEmpty()
                where
                    accountAuditLogEntry.UserId == agentId &&
                    accountAuditLogEntry.UserType == UserTypes.Agent &&
                    accountAuditLogEntry.Created <= paymentHistoryRequest.ToDate &&
                    accountAuditLogEntry.Created >= paymentHistoryRequest.FromDate ||
                    cardAuditLogEntry.UserId == agentId &&
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
        }


        private static Result Validate(PaymentHistoryRequest paymentHistoryRequest)
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
            return type switch
            {
                AccountEventType.None => PaymentHistoryType.None,
                AccountEventType.Add => PaymentHistoryType.Add,
                AccountEventType.Charge => PaymentHistoryType.Charge,
                AccountEventType.Authorize => PaymentHistoryType.Authorize,
                AccountEventType.Capture => PaymentHistoryType.Capture,
                AccountEventType.Void => PaymentHistoryType.Void,
                AccountEventType.Refund => PaymentHistoryType.Refund,
                AccountEventType.CounterpartyTransferToAgency => PaymentHistoryType.Add,
                _ => throw new ArgumentOutOfRangeException(nameof(type), type, null)
            };
        }


        private static PaymentHistoryType ToPaymentHistoryType(CreditCardEventType type)
        {
            return type switch
            {
                CreditCardEventType.Authorize => PaymentHistoryType.Authorize,
                CreditCardEventType.Capture => PaymentHistoryType.Capture,
                CreditCardEventType.Void => PaymentHistoryType.None,
                CreditCardEventType.None => PaymentHistoryType.None,
                _ => throw new ArgumentOutOfRangeException(nameof(type), type, null)
            };
        }


        private const int MaxRequestDaysNumber = 3650;

        private readonly EdoContext _edoContext;
    }
}