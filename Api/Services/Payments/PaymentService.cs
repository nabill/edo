using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using HappyTravel.Edo.Common.Enums;

namespace HappyTravel.Edo.Api.Services.Payments
{
    public class PaymentService : IPaymentService
    {
        private static readonly Currency[] Currencies = Enum.GetValues(typeof(Currency))
            .Cast<Currency>()
            .ToArray();
        
        private static readonly PaymentMethod[] PaymentMethods = Enum.GetValues(typeof(PaymentMethod))
            .Cast<PaymentMethod>()
            .ToArray();

        public IReadOnlyCollection<Currency> GetCurrencies() => new ReadOnlyCollection<Currency>(Currencies);
        public IReadOnlyCollection<PaymentMethod> GetAvailableCustomerPaymentMethods() => new ReadOnlyCollection<PaymentMethod>(PaymentMethods);
    }
}