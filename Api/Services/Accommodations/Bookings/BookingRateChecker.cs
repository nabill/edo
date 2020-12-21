using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using HappyTravel.Edo.Api.Infrastructure;
using HappyTravel.Edo.Api.Models.Accommodations;
using HappyTravel.Edo.Api.Models.Agents;
using HappyTravel.Edo.Api.Models.Bookings;
using HappyTravel.Edo.Api.Services.Accommodations.Availability;
using HappyTravel.Edo.Common.Enums.AgencySettings;
using HappyTravel.EdoContracts.General.Enums;

namespace HappyTravel.Edo.Api.Services.Accommodations.Bookings
{
    public class BookingRateChecker : IBookingRateChecker
    {
        public BookingRateChecker(IAccommodationBookingSettingsService settingsService,
            IDateTimeProvider dateTimeProvider)
        {
            _settingsService = settingsService;
            _dateTimeProvider = dateTimeProvider;
        }


        public async Task<Result> Check(AccommodationBookingRequest bookingRequest, BookingAvailabilityInfo availabilityInfo, AgentContext agent)
        {
            return await GetSettings()
                .Ensure(AreAprSettingsSuitable, "You can't book the restricted contract without explicit approval from a Happytravel.com officer.")
                .Ensure(AreDeadlineSettingsSuitable, "You can't book the contract within deadline without explicit approval from a Happytravel.com officer.");

                
            async Task<Result<AccommodationBookingSettings>> GetSettings() => await _settingsService.Get(agent);
            
            
            bool AreDeadlineSettingsSuitable(AccommodationBookingSettings settings)
            {
                var deadlineDate = availabilityInfo.RoomContractSet.Deadline.Date ?? availabilityInfo.CheckInDate;
                if (deadlineDate.Date > _dateTimeProvider.UtcTomorrow())
                    return true;

                return settings.PassedDeadlineOffersMode switch
                {
                    PassedDeadlineOffersMode.CardAndAccountPurchases => true,
                    PassedDeadlineOffersMode.CardPurchasesOnly
                        when bookingRequest.PaymentMethod == PaymentMethods.CreditCard => true,
                    _ => false
                };
            }
            
            
            bool AreAprSettingsSuitable(AccommodationBookingSettings settings)
            {
                if (!availabilityInfo.RoomContractSet.IsAdvancePurchaseRate)
                    return true;

                return settings.AprMode switch
                {
                    AprMode.CardAndAccountPurchases => true,
                    AprMode.CardPurchasesOnly
                        when bookingRequest.PaymentMethod == PaymentMethods.CreditCard => true,
                    _ => false
                };
            }
        }
        
        
        private readonly IAccommodationBookingSettingsService _settingsService;
        private readonly IDateTimeProvider _dateTimeProvider;
    }
}