using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using HappyTravel.Edo.Api.AdministratorServices.Models;
using HappyTravel.Edo.Api.NotificationCenter.Services;
using HappyTravel.Edo.Data;
using HappyTravel.Edo.Data.Agents;
using HappyTravel.Edo.Data.Payments;

namespace Api.AdministratorServices
{
    public class BalanceNotificationsService : IBalanceNotificationsService
    {
        public BalanceNotificationsService(EdoContext context, INotificationService notificationService)
        {
            _context = context;
            _notificationService = notificationService;
        }


        public async Task<Result> NotifyCreditLimitRunOutBalance(CancellationToken cancellationToken = default)
        {
            var agenciesInfo = _context.Agencies
                .Where(a => a.ContractKind == ContractKind.VirtualAccountOrCreditCardPayments
                    && a.CreditLimit != null)
                .Join(_context.AgencyAccounts, agency => agency.Id, account => account.AgencyId, ToBalanceLimitProjection())
                .ToList();


        }


        private Func<Agency, AgencyAccount, AgencyBalanceLimitInfo> ToBalanceLimitProjection()
            => (agency, account)
                => new AgencyBalanceLimitInfo(agency, account);


        private readonly EdoContext _context;
        private readonly INotificationService _notificationService;
    }
}