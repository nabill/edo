using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using HappyTravel.Edo.Api.Models.Payments;
using HappyTravel.Edo.Api.Services.Management;
using HappyTravel.Edo.Common.Enums;

namespace HappyTravel.Edo.Api.Services.Payments
{
    public class PaymentService : IPaymentService
    {
        public PaymentService(IAdministratorContext adminContext, 
            IPaymentProcessingService paymentProcessingService)
        {
            _adminContext = adminContext;
            _paymentProcessingService = paymentProcessingService;
        }
        
        private static readonly Currencies[] Currencies = Enum.GetValues(typeof(Currencies))
            .Cast<Currencies>()
            .ToArray();
        
        private static readonly PaymentMethods[] PaymentMethods = Enum.GetValues(typeof(PaymentMethods))
            .Cast<PaymentMethods>()
            .ToArray();

        public IReadOnlyCollection<Currencies> GetCurrencies() => new ReadOnlyCollection<Currencies>(Currencies);
        
        public IReadOnlyCollection<PaymentMethods> GetAvailableCustomerPaymentMethods() => new ReadOnlyCollection<PaymentMethods>(PaymentMethods);
        
        public Task<Result> ReplenishAccount(int accountId, PaymentData payment)
        {
            return Result.Ok()
                .Ensure(HasPermission, "Permission denied")
                .OnSuccess(AddMoney);

            Task<bool> HasPermission()
            {
                return _adminContext.HasPermission(AdministratorPermissions.AccountReplenish);
            }

            async Task<Result> AddMoney()
            {
                var userInfo = await _adminContext.GetUserInfo();
                return await _paymentProcessingService.AddMoney(accountId,
                    payment, 
                    userInfo);
            }
        }
        
        private readonly IAdministratorContext _adminContext;
        private readonly IPaymentProcessingService _paymentProcessingService;
    }
}