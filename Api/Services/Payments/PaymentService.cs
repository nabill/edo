using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using FluentValidation;
using HappyTravel.Edo.Api.Infrastructure;
using HappyTravel.Edo.Api.Models.Payments;
using HappyTravel.Edo.Api.Models.Payments.Payfort;
using HappyTravel.Edo.Api.Services.Management;
using HappyTravel.Edo.Common.Enums;
using HappyTravel.Edo.Data;
using HappyTravel.Edo.Data.Customers;
using HappyTravel.Edo.Data.Payments;
using Microsoft.EntityFrameworkCore;

namespace HappyTravel.Edo.Api.Services.Payments
{
    public class PaymentService : IPaymentService
    {
        public PaymentService(IAdministratorContext adminContext,
            IPaymentProcessingService paymentProcessingService,
            EdoContext context,
            IPayfortService payfortService,
            IDateTimeProvider dateTimeProvider)
        {
            _adminContext = adminContext;
            _paymentProcessingService = paymentProcessingService;
            _context = context;
            _payfortService = payfortService;
            _dateTimeProvider = dateTimeProvider;
        }

        public IReadOnlyCollection<Currencies> GetCurrencies() => new ReadOnlyCollection<Currencies>(Currencies);
        public IReadOnlyCollection<PaymentMethods> GetAvailableCustomerPaymentMethods() => new ReadOnlyCollection<PaymentMethods>(PaymentMethods);

        public async Task<Result<PaymentResponse>> Pay(PaymentRequest request, string languageCode, string ipAddress, Customer customer, Company company)
        {
            var (_, isFailure, error) = await Validate(request);
            if (isFailure)
                return Result.Fail<PaymentResponse>(error);

            var (_, isPaymentFailure, payment, paymentError) = await _payfortService.Pay(new CreditCardPaymentRequest(amount: request.Amount,
                currency: request.Currency,
                token: request.Token, 
                isOneTime: request.TokenType == PaymentTokenTypes.OneTime,
                customerName: $"{customer.FirstName} {customer.LastName}",
                customerEmail: customer.Email,
                customerIp: ipAddress,
                referenceCode: request.ReferenceCode,
                languageCode: languageCode,
                cardSecurityCode: request.SecurityCode));
            if (isPaymentFailure)
                return Result.Fail<PaymentResponse>(paymentError);
            var booking = await _context.Bookings.FirstAsync(b => b.ReferenceCode == request.ReferenceCode);
            await _context.Payments.AddAsync(new Payment()
            {
                Amount = request.Amount,
                BookingId = booking.Id,
                CustomerIp = ipAddress,
                MaskedNumber = payment.CardNumber,
                Currency = request.Currency,
                Created = _dateTimeProvider.UtcNow(),
                Status = payment.Status
            });
            await _context.SaveChangesAsync();
            return Result.Ok(new PaymentResponse(payment.Secure3d, payment.Status));
        }

        private async Task<Result> Validate(PaymentRequest request)
        {
            var fieldValidateResult = GenericValidator<PaymentRequest>.Validate(v =>
            {
                v.RuleFor(c => c.Amount).NotEmpty();
                v.RuleFor(c => c.Currency).NotEmpty().IsInEnum().Must(c => c != Common.Enums.Currencies.NotSpecified);
                v.RuleFor(c => c.ReferenceCode).NotEmpty();
                v.RuleFor(c => c.Token).NotEmpty();
                v.RuleFor(c => c.SecurityCode).NotEmpty();
                v.RuleFor(c => c.TokenType).IsInEnum().Must(t => t != PaymentTokenTypes.Unknown);
            }, request);

            if (fieldValidateResult.IsFailure)
                return fieldValidateResult;

            return Result.Combine(await CheckReferenceCode(request.ReferenceCode));
        }

        private async Task<Result> CheckReferenceCode(string referenceCode)
        {
            var booking = await _context.Bookings.Where(b => b.ReferenceCode == referenceCode).FirstOrDefaultAsync();
            if (booking == null)
                return Result.Fail("Invalid Reference code");
            
            if (InvalidBookingStatuses.Contains(booking.Status))
                return Result.Fail($"Invalid booking status: {booking.Status.ToString()}");
            
            return Result.Ok();
        }
  
        public Task<Result> ReplenishAccount(int accountId, PaymentData payment)
        {
            return Result.Ok()
                .Ensure(HasPermission, "Permission denied")
                .OnSuccess(AddMoney);

            Task<bool> HasPermission()
            {
                return _adminContext.HasPermission(AdministratorPermissions.AddingMoneyToAccount);
            }

            async Task<Result> AddMoney()
            {
                var userInfo = await _adminContext.GetUserInfo();
                return await _paymentProcessingService.AddMoney(accountId,
                    payment, 
                    userInfo);
            }
        }

        private static readonly Currencies[] Currencies = Enum.GetValues(typeof(Currencies))
            .Cast<Currencies>()
            .ToArray();

        private static readonly PaymentMethods[] PaymentMethods = Enum.GetValues(typeof(PaymentMethods))
            .Cast<PaymentMethods>()
            .ToArray();

        private static readonly BookingStatusCodes[] InvalidBookingStatuses = new[]
            {BookingStatusCodes.Cancelled, BookingStatusCodes.Invalid, BookingStatusCodes.Rejected};

        private readonly IAdministratorContext _adminContext;
        private readonly IPaymentProcessingService _paymentProcessingService;
        private readonly EdoContext _context;
        private readonly IPayfortService _payfortService;
        private readonly IDateTimeProvider _dateTimeProvider;
    }
}
