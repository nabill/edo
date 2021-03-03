using System;
using System.Linq;
using System.Threading.Tasks;
using HappyTravel.Edo.Data;
using HappyTravel.Edo.Data.Agents;
using Microsoft.EntityFrameworkCore;

namespace HappyTravel.Edo.Api.Services.Accommodations.Availability
{
    public class CounterpartySystemSettingsService : ICounterpartySystemSettingsService
    {
        public CounterpartySystemSettingsService(EdoContext context)
        {
            _context = context;
        }


        public async Task<CounterpartyAccommodationBookingSettings> GetAccommodationBookingSettings(int agentCounterpartyId)
        {
            return new()
            {
                CancellationPolicyProcessSettings = new CancellationPolicyProcessSettings
                {
                    PolicyStartDateShift = await GetPolicyDateShift()
                }
            };


            async Task<TimeSpan> GetPolicyDateShift()
            {
                var contractKind = await _context.Counterparties
                    .Where(c => c.Id == agentCounterpartyId)
                    .Select(c => c.ContractKind)
                    .SingleOrDefaultAsync();

                return contractKind switch
                {
                    CounterpartyContractKind.CashPayments => TimeSpan.FromDays(-3),
                    CounterpartyContractKind.CreditPayments => TimeSpan.FromDays(-1),
                    null => DefaultPolicyDateShift,
                    _ => throw new ArgumentOutOfRangeException(nameof(contractKind), contractKind, "Unknown contract kind")
                };
            }
        }


        private static readonly TimeSpan DefaultPolicyDateShift = TimeSpan.FromDays(-3);
        private readonly EdoContext _context;
    }
}