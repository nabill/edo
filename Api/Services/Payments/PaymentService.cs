using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using HappyTravel.Edo.Common.Enums;

namespace HappyTravel.Edo.Api.Services.Payments
{
    public class PaymentService : IPaymentService
    {
        private static readonly Currencies[] Currencies = Enum.GetValues(typeof(Currencies))
            .Cast<Currencies>()
            .ToArray();
        
        private static readonly PaymentMethods[] PaymentMethods = Enum.GetValues(typeof(PaymentMethods))
            .Cast<PaymentMethods>()
            .ToArray();

        public IReadOnlyCollection<Currencies> GetCurrencies() => new ReadOnlyCollection<Currencies>(Currencies);
        public IReadOnlyCollection<PaymentMethods> GetAvailableCustomerPaymentMethods() => new ReadOnlyCollection<PaymentMethods>(PaymentMethods);
    }
}