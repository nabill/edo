using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using HappyTravel.EdoContracts.General.Enums;
using HappyTravel.Money.Enums;

namespace HappyTravel.Edo.Api.Services.Payments
{
    public class PaymentSettingsService : IPaymentSettingsService
    {
        public IReadOnlyCollection<Currencies> GetCurrencies() => new ReadOnlyCollection<Currencies>(Currencies);

        public IReadOnlyCollection<PaymentMethods> GetAvailableAgentPaymentMethods() => new ReadOnlyCollection<PaymentMethods>(AvailablePaymentMethods);

        private static readonly Currencies[] Currencies = Enum.GetValues(typeof(Currencies))
            .Cast<Currencies>()
            .ToArray();

        private static readonly PaymentMethods[] AvailablePaymentMethods = {PaymentMethods.BankTransfer, PaymentMethods.CreditCard};
    }
}