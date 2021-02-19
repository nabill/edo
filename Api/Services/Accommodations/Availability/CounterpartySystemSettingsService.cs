using System;
using System.Threading.Tasks;

namespace HappyTravel.Edo.Api.Services.Accommodations.Availability
{
    public class CounterpartySystemSettingsService : ICounterpartySystemSettingsService
    {
        public async Task<CounterpartyAccommodationBookingSettings> GetAccommodationBookingSettings(int agentCounterpartyId)
        {
            // TODO implement different settings base on contract type
            return new()
            {
                CancellationPolicyProcessSettings = new CancellationPolicyProcessSettings
                {
                    PolicyStartDateShift = TimeSpan.FromDays(-1)
                }
            };
        }
    }
}