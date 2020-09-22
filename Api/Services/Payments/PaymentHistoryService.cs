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


        public async Task<Result<List<PaymentHistoryData>>> GetAgentHistory(PaymentHistoryRequest paymentHistoryRequest, int agencyId, AgentContext agent)
        {
            var validationResult = Validate(paymentHistoryRequest);
            if (validationResult.IsFailure)
                return Result.Failure<List<PaymentHistoryData>>(validationResult.Error);

            var accountHistoryData = await _edoContext.AgencyAccounts.Where(a => a.AgencyId == agencyId)
                    .Join(_edoContext.AccountBalanceAuditLogs
                            .Where(i => i.UserId == agent.AgentId)
                            .Where(i => i.UserType == UserTypes.Agent)
                            .Where(i => i.Created <= paymentHistoryRequest.ToDate &&
                                paymentHistoryRequest.FromDate <= i.Created),
                        pa => pa.Id,
                        bl => bl.AccountId,
                        (pa, bl) => new PaymentHistoryData(bl.Created,
                            bl.Amount,
                            JObject.Parse(bl.EventData),
                            pa.Currency.ToString(),
                            bl.UserId,
                            ToPaymentHistoryType(bl.Type),
                            PaymentMethods.BankTransfer))
                .ToListAsync();

            var cardHistoryData = await _edoContext.CreditCardAuditLogs
                .Where(i => i.AgentId == agent.AgentId
                    && i.Created <= paymentHistoryRequest.ToDate
                    && paymentHistoryRequest.FromDate <= i.Created)
                .Select(a => new PaymentHistoryData(a.Created,
                    a.Amount, JObject.Parse(a.EventData),
                    a.Currency.ToString(),
                    a.UserId,
                    ToPaymentHistoryType(a.Type),
                    PaymentMethods.CreditCard))
                .ToListAsync();

            var result = accountHistoryData.Union(cardHistoryData).OrderByDescending(h => h.Created).ToList();
            return Result.Ok(result);
        }


        public async Task<Result<List<PaymentHistoryData>>> GetAgencyHistory(PaymentHistoryRequest paymentHistoryRequest, int agencyId, AgentContext agent)
        {
            var validationResult = Validate(paymentHistoryRequest);
            if (validationResult.IsFailure)
                return Result.Failure<List<PaymentHistoryData>>(validationResult.Error);

            if (!agent.IsUsingAgency(agencyId))
                return Result.Failure<List<PaymentHistoryData>>("You can only observe history of an agency you are currently using");

            var accountHistoryData = await _edoContext.AgencyAccounts.Where(i => i.AgencyId == agencyId)
                    .Join(_edoContext.AccountBalanceAuditLogs.Where(i => i.Created <= paymentHistoryRequest.ToDate &&
                            paymentHistoryRequest.FromDate <= i.Created),
                        pa => pa.Id,
                        bl => bl.AccountId,
                        (pa, bl) => new PaymentHistoryData(bl.Created,
                            bl.Amount, JObject.Parse(bl.EventData),
                            pa.Currency.ToString(),
                            bl.UserId,
                            ToPaymentHistoryType(bl.Type),
                            PaymentMethods.BankTransfer))
                .ToListAsync();

            var cardHistoryData = await _edoContext.CreditCardAuditLogs
                .Where(i => i.AgentId == agent.AgentId
                    && i.Created <= paymentHistoryRequest.ToDate
                    && paymentHistoryRequest.FromDate <= i.Created)
                .Select(a => new PaymentHistoryData(a.Created,
                    a.Amount, JObject.Parse(a.EventData),
                    a.Currency.ToString(),
                    a.UserId,
                    ToPaymentHistoryType(a.Type),
                    PaymentMethods.CreditCard))
                    .ToListAsync();

            var result = accountHistoryData.Union(cardHistoryData).OrderByDescending(h => h.Created).ToList();
            return Result.Ok(result);
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