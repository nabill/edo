using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Api.Models.Mailing;
using CSharpFunctionalExtensions;
using HappyTravel.Edo.Api.AdministratorServices.Models;
using HappyTravel.Edo.Api.NotificationCenter.Services;
using HappyTravel.Edo.Common.Enums;
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

            foreach (var item in agenciesInfo.Where(a => a.Balance != null && a.CreditLimit != null))
            {
                var fourtyPercent = item.CreditLimit!.Value * 0.4m;
                var twentyPercent = item.CreditLimit!.Value * 0.2m;
                var tenPercent = item.CreditLimit!.Value * 0.1m;

                switch (item.Balance)
                {
                    case decimal balance when (balance > fourtyPercent
                        && item.CreditLimitNotifications != CreditLimitNotifications.MoreThanFourty):
                        await SendNotifications(item, CreditLimitNotifications.MoreThanFourty);
                        break;
                    case decimal balance when (balance > twentyPercent && balance <= fourtyPercent
                        && item.CreditLimitNotifications != CreditLimitNotifications.FourtyOrLess):
                        await SendNotifications(item, CreditLimitNotifications.FourtyOrLess);
                        break;
                    case decimal balance when (balance > tenPercent && balance <= twentyPercent
                        && item.CreditLimitNotifications != CreditLimitNotifications.TwentyOrLess):
                        await SendNotifications(item, CreditLimitNotifications.TwentyOrLess);
                        break;
                    case decimal balance when (balance <= tenPercent
                        && item.CreditLimitNotifications != CreditLimitNotifications.TenOrLess):
                        await SendNotifications(item, CreditLimitNotifications.TenOrLess);
                        break;
                    default:
                        break;
                }
            }

            return Result.Success();
        }


        private async Task SendNotifications(AgencyBalanceLimitInfo agencyBalanceLimitInfo, CreditLimitNotifications targetNotification)
        {
            // if (targetNotification != CreditLimitNotifications.MoreThanFourty)
            // {
            //     var masterAgent = await _context.AgentAgencyRelations
            //         .Where(a => a.AgentRoleIds)

            //     var messageData = new CreditLimitData
            //     {
            //         percentage = (int)targetNotification,
            //         agentName = 
            //     };
            // }
        }


        private Func<Agency, AgencyAccount, AgencyBalanceLimitInfo> ToBalanceLimitProjection()
            => (agency, account)
                => new AgencyBalanceLimitInfo(agency, account);


        private readonly EdoContext _context;
        private readonly INotificationService _notificationService;
    }
}