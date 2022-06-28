using System;
using System.Collections.Generic;
using System.Linq;
using HappyTravel.Edo.Common.Enums;
using HappyTravel.Edo.Common.Enums.AgencySettings;
using HappyTravel.Edo.Data.Agents;

namespace HappyTravel.Edo.Api.Services.Accommodations.Availability.Steps.BookingEvaluation
{
    public static class BookingPaymentTypesHelper
    {
        public static List<PaymentTypes> GetAvailablePaymentTypes(in EdoContracts.Accommodations.RoomContractSetAvailability availability,
            AccommodationBookingSettings settings, ContractKind contractKind, DateTimeOffset nowDate, bool isBalanceEnough)
            => AllAvailablePaymentTypes
                .Intersect(GetAprPaymentTypes(availability, settings))
                .Intersect(GetPassedDeadlinePaymentMethods(availability, settings, nowDate, isBalanceEnough))
                .Intersect(GetContractKindPaymentTypes(contractKind))
                .ToList();


        public static PaymentTypes GetDefaultPaymentType(ContractKind? contractKind)
            => contractKind switch
            {
                ContractKind.OfflineOrCreditCardPayments => PaymentTypes.Offline,
                ContractKind.VirtualAccountOrCreditCardPayments => PaymentTypes.CreditCard,
                ContractKind.CreditCardPayments => PaymentTypes.VirtualAccount,
                _ => PaymentTypes.NotSpecified
            };
        

        private static List<PaymentTypes> GetAprPaymentTypes(in EdoContracts.Accommodations.RoomContractSetAvailability availability,
            AccommodationBookingSettings settings)
        {
            if (!availability.RoomContractSet.IsAdvancePurchaseRate)
                return AllAvailablePaymentTypes;

            return settings.AprMode switch
            {
                AprMode.Hide => EmptyPaymentTypesList,
                AprMode.DisplayOnly => EmptyPaymentTypesList,
                AprMode.CardPurchasesOnly => new List<PaymentTypes> { PaymentTypes.CreditCard },
                AprMode.CardAndAccountPurchases => new List<PaymentTypes> { PaymentTypes.VirtualAccount, PaymentTypes.CreditCard },
                _ => throw new ArgumentOutOfRangeException(nameof(settings.AprMode), $"Invalid value {settings.AprMode}")
            };
        }


        private static List<PaymentTypes> GetPassedDeadlinePaymentMethods(in EdoContracts.Accommodations.RoomContractSetAvailability availability,
            AccommodationBookingSettings settings, DateTimeOffset nowDate, bool isBalanceEnough)
        {
            var deadlineDate = availability.RoomContractSet.Deadline.Date ?? availability.CheckInDate;

            if (nowDate < deadlineDate.Date - OfflinePaymentAdditionalDays)
                return AllAvailablePaymentTypes;

            if (nowDate < deadlineDate.Date)
                return new List<PaymentTypes> { PaymentTypes.VirtualAccount, PaymentTypes.CreditCard };

            if (!isBalanceEnough && settings.PassedDeadlineOffersMode == PassedDeadlineOffersMode.CardAndAccountPurchases)
                return new List<PaymentTypes> {PaymentTypes.CreditCard};

            return settings.PassedDeadlineOffersMode switch
            {
                PassedDeadlineOffersMode.Hide => EmptyPaymentTypesList,
                PassedDeadlineOffersMode.DisplayOnly => EmptyPaymentTypesList,
                PassedDeadlineOffersMode.CardPurchasesOnly => new List<PaymentTypes> { PaymentTypes.CreditCard },
                PassedDeadlineOffersMode.CardAndAccountPurchases => new List<PaymentTypes> { PaymentTypes.VirtualAccount, PaymentTypes.CreditCard },
                _ => throw new ArgumentOutOfRangeException(nameof(settings.PassedDeadlineOffersMode), $"Invalid value {settings.PassedDeadlineOffersMode}")
            };
        }

        
        private static List<PaymentTypes> GetContractKindPaymentTypes(ContractKind contractKind)
        {
            return contractKind switch
            {
                ContractKind.OfflineOrCreditCardPayments => new List<PaymentTypes> { PaymentTypes.Offline, PaymentTypes.CreditCard },
                ContractKind.CreditCardPayments => new List<PaymentTypes> { PaymentTypes.CreditCard },
                ContractKind.VirtualAccountOrCreditCardPayments => new List<PaymentTypes> { PaymentTypes.CreditCard, PaymentTypes.VirtualAccount },
                ContractKind.NotSpecified => EmptyPaymentTypesList,
                _ => throw new ArgumentOutOfRangeException(nameof(contractKind), $"Invalid value {contractKind}")
            };
        }


        private static readonly List<PaymentTypes> AllAvailablePaymentTypes = new()
        {
            PaymentTypes.VirtualAccount,
            PaymentTypes.CreditCard,
            PaymentTypes.Offline
        };


        public static readonly TimeSpan OfflinePaymentAdditionalDays = TimeSpan.FromDays(3);

        private static readonly List<PaymentTypes> EmptyPaymentTypesList = new(0);
    }
}