using System;
using System.Collections.Generic;
using System.Linq;
using HappyTravel.Edo.Common.Enums.AgencySettings;
using HappyTravel.EdoContracts.General.Enums;

namespace HappyTravel.Edo.Api.Services.Accommodations.Availability.Steps.BookingEvaluation
{
    public static class BookingPaymentMethodsHelper
    {
        public static List<PaymentMethods> GetAvailablePaymentMethods(in EdoContracts.Accommodations.RoomContractSetAvailability availability,
            AccommodationBookingSettings settings, DateTime date)
            => AllAvailablePaymentMethods
                .Intersect(GetAprPaymentMethods(availability, settings))
                .Intersect(GetPassedDeadlinePaymentMethods(availability, settings, date))
                .ToList();


        private static List<PaymentMethods> GetAprPaymentMethods(in EdoContracts.Accommodations.RoomContractSetAvailability availability,
            AccommodationBookingSettings settings)
        {
            if (!availability.RoomContractSet.IsAdvancePurchaseRate)
                return AllAvailablePaymentMethods;

            return settings.AprMode switch
            {
                AprMode.Hide => EmptyPaymentMethodsList,
                AprMode.DisplayOnly => EmptyPaymentMethodsList,
                AprMode.CardPurchasesOnly => new List<PaymentMethods> {PaymentMethods.CreditCard},
                AprMode.CardAndAccountPurchases => new List<PaymentMethods> {PaymentMethods.BankTransfer, PaymentMethods.CreditCard},
                _ => throw new ArgumentOutOfRangeException(nameof(settings.AprMode), $"Invalid value {settings.AprMode}")
            };
        }


        private static List<PaymentMethods> GetPassedDeadlinePaymentMethods(in EdoContracts.Accommodations.RoomContractSetAvailability availability,
            AccommodationBookingSettings settings, DateTime date)
        {
            var deadlineDate = availability.RoomContractSet.Deadline.Date ?? availability.CheckInDate;
            if (deadlineDate.Date <= date.AddDays(-1))
                return AllAvailablePaymentMethods;

            return settings.PassedDeadlineOffersMode switch
            {
                PassedDeadlineOffersMode.Hide => EmptyPaymentMethodsList,
                PassedDeadlineOffersMode.DisplayOnly => EmptyPaymentMethodsList,
                PassedDeadlineOffersMode.CardPurchasesOnly => new List<PaymentMethods> {PaymentMethods.CreditCard},
                PassedDeadlineOffersMode.CardAndAccountPurchases => new List<PaymentMethods> {PaymentMethods.BankTransfer, PaymentMethods.CreditCard},
                _ => throw new ArgumentOutOfRangeException(nameof(settings.PassedDeadlineOffersMode), $"Invalid value {settings.PassedDeadlineOffersMode}")
            };
        }


        private static readonly List<PaymentMethods> AllAvailablePaymentMethods = new()
        {
            PaymentMethods.BankTransfer,
            PaymentMethods.CreditCard
        };

        private static readonly List<PaymentMethods> EmptyPaymentMethodsList = new(0);
    }
}