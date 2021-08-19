using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using HappyTravel.Edo.Api.Infrastructure;
using HappyTravel.Edo.Common.Enums;
using HappyTravel.Money.Enums;
using Microsoft.Extensions.Options;

namespace HappyTravel.Edo.Api.Services.Payments
{
    public class PaymentSettingsService : IPaymentSettingsService
    {
        public PaymentSettingsService(IOptionsMonitor<PaymentSystemOption> optionsMonitor)
        {
            _optionsMonitor = optionsMonitor;
        }
        
        
        public IReadOnlyCollection<Currencies> GetCurrencies() 
            => new ReadOnlyCollection<Currencies>(Values<Currencies>());

        public IReadOnlyCollection<PaymentTypes> GetAvailableAgentPaymentTypes() 
            => new ReadOnlyCollection<PaymentTypes>(AvailablePaymentMethods);


        public IReadOnlyCollection<PaymentSystems> GetPaymentSystems() 
            => new ReadOnlyCollection<PaymentSystems>(Values<PaymentSystems>());


        public PaymentSystems GetEnabledPaymentSystem() 
            => _optionsMonitor.CurrentValue.EnabledPaymentSystem;


        private static T[] Values<T>() where T : Enum
            => Enum.GetValues(typeof(T)).Cast<T>().ToArray();
        

        private static readonly PaymentTypes[] AvailablePaymentMethods = {PaymentTypes.VirtualAccount, PaymentTypes.CreditCard};
        
        
        private readonly IOptionsMonitor<PaymentSystemOption> _optionsMonitor;
    }
}