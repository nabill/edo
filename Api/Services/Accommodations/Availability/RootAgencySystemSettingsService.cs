using System;
using System.Linq;
using System.Threading.Tasks;
using HappyTravel.Edo.Data;
using HappyTravel.Edo.Data.Agents;
using Microsoft.EntityFrameworkCore;

namespace HappyTravel.Edo.Api.Services.Accommodations.Availability
{
    public class RootAgencySystemSettingsService : IRootAgencySystemSettingsService
    {
        public RootAgencySystemSettingsService(EdoContext context)
        {
            _context = context;
        }


        public async Task<RootAgencyAccommodationBookingSettings> GetAccommodationBookingSettings(int agentAgencyId)
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
                var contractKind = await _context.Agencies
                    .Where(c => c.Id == agentAgencyId)
                    .Join(_context.Agencies,
                        a => a.Ancestors.Any()
                            ? a.Ancestors[0]
                            : a.Id,
                        ra => ra.Id,
                        (agency, rootAgency) => rootAgency)
                    .Select(c => c.ContractKind)
                    .SingleOrDefaultAsync();

                return contractKind switch
                {
                    ContractKind.OfflineOrCreditCardPayments => TimeSpan.FromDays(-2),
                    ContractKind.CreditCardPayments => TimeSpan.FromDays(-1),
                    ContractKind.VirtualAccountOrCreditCardPayments => TimeSpan.FromDays(-1),
                    null => DefaultPolicyDateShift,
                    _ => throw new ArgumentOutOfRangeException(nameof(contractKind), contractKind, "Unknown contract kind")
                };
            }
        }


        private static readonly TimeSpan DefaultPolicyDateShift = TimeSpan.FromDays(-3);
        private readonly EdoContext _context;
    }
}