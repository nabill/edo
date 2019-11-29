using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using FluentValidation;
using HappyTravel.Edo.Api.Infrastructure;
using HappyTravel.Edo.Api.Models.Payments;
using HappyTravel.Edo.Api.Services.Customers;
using HappyTravel.Edo.Common.Enums;
using HappyTravel.Edo.Data;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json.Linq;

namespace HappyTravel.Edo.Api.Services.Payments
{
    public class PaymentHistoryService : IPaymentHistoryService
    {
        public PaymentHistoryService(EdoContext edoContext, ICustomerContext customerContext, IPermissionChecker permissionChecker)
        {
            _edoContext = edoContext;
            _customerContext = customerContext;
            _permissionChecker = permissionChecker;
        }


        public async Task<Result<List<PaymentHistoryData>>> GetCustomerHistory(PaymentHistoryRequest paymentHistoryRequest)
        {
            var validationResult = Validate(paymentHistoryRequest);
            if (validationResult.IsFailure)
                return Result.Fail<List<PaymentHistoryData>>(validationResult.Error);

            var customerInfoResult = await _customerContext.GetCustomerInfo();
            if (customerInfoResult.IsFailure)
                return Result.Fail<List<PaymentHistoryData>>(customerInfoResult.Error);

            var customerInfo = customerInfoResult.Value;

            var historyData = await _edoContext.PaymentAccounts.Where(a => a.CompanyId == customerInfo.CompanyId)
                .Join(_edoContext.AccountBalanceAuditLogs
                        .Where(i => i.UserId == customerInfo.CustomerId)
                        .Where(i => i.UserType == UserTypes.Customer),
                    pa => pa.Id,
                    bl => bl.AccountId,
                    (pa, bl) => new PaymentHistoryData(bl.Created,
                        bl.Amount,
                        JObject.Parse(bl.EventData),
                        pa.Currency.ToString()))
                .ToListAsync();

            return Result.Ok(historyData);
        }


        public async Task<Result<List<PaymentHistoryData>>> GetCompanyHistory(PaymentHistoryRequest paymentHistoryRequest)
        {
            var validationResult = Validate(paymentHistoryRequest);
            if (validationResult.IsFailure)
                return Result.Fail<List<PaymentHistoryData>>(validationResult.Error);

            var customerResult = await _customerContext.GetCustomerInfo();
            if (customerResult.IsFailure)
                return Result.Fail<List<PaymentHistoryData>>(customerResult.Error);

            var customerInfo = customerResult.Value;

            var checkPermissionsResult = await _permissionChecker.CheckInCompanyPermission(customerInfo, 
                InCompanyPermissions.ViewCompanyPaymentHistory);
            if (checkPermissionsResult.IsFailure)
                return Result.Fail<List<PaymentHistoryData>>(checkPermissionsResult.Error);

            var historyData = await _edoContext.PaymentAccounts.Where(a => a.CompanyId == customerInfo.CompanyId)
                .Join(_edoContext.AccountBalanceAuditLogs,
                    pa => pa.Id,
                    bl => bl.AccountId,
                    (pa, bl) => new PaymentHistoryData(bl.Created,
                        bl.Amount, JObject.Parse(bl.EventData),
                        pa.Currency.ToString()))
                .ToListAsync();

            return Result.Ok(historyData);
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


        private readonly EdoContext _edoContext;
        private readonly ICustomerContext _customerContext;
        private readonly IPermissionChecker _permissionChecker;
        private const int MaxRequestDaysNumber = 3650;
    }
}