using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Api.Models.Mailing;
using CSharpFunctionalExtensions;
using HappyTravel.Edo.Api.AdministratorServices.Models;
using HappyTravel.Edo.Api.Models.Agents;
using HappyTravel.Edo.Api.NotificationCenter.Services;
using HappyTravel.Edo.Common.Enums;
using HappyTravel.Edo.Data;
using HappyTravel.Edo.Data.Agents;
using HappyTravel.Edo.Data.Payments;
using HappyTravel.Edo.Notifications.Enums;
using Microsoft.EntityFrameworkCore;

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
                    && a.CreditLimit != null && a.IsActive)
                .Join(_context.AgencyAccounts, agency => agency.Id, account => account.AgencyId, ToBalanceLimitProjection())
                .ToList();

            foreach (var item in agenciesInfo.Where(a => a.Balance != null && a.CreditLimit != null))
            {
                var fourtyPercent = item.CreditLimit!.Value * 0.4m;
                var twentyPercent = item.CreditLimit!.Value * 0.2m;
                var tenPercent = item.CreditLimit!.Value * 0.1m;

                switch (item.Balance)
                {
                    case decimal balance when (balance <= tenPercent
                        && item.CreditLimitNotifications != CreditLimitNotifications.TenOrLess):
                        await SendNotificationsIfNeed(item, CreditLimitNotifications.TenOrLess, cancellationToken);
                        break;
                    case decimal balance when (balance <= twentyPercent
                        && item.CreditLimitNotifications != CreditLimitNotifications.TwentyOrLess):
                        await SendNotificationsIfNeed(item, CreditLimitNotifications.TwentyOrLess, cancellationToken);
                        break;
                    case decimal balance when (balance <= fourtyPercent
                        && item.CreditLimitNotifications != CreditLimitNotifications.FourtyOrLess):
                        await SendNotificationsIfNeed(item, CreditLimitNotifications.FourtyOrLess, cancellationToken);
                        break;
                    case decimal balance when (balance > fourtyPercent
                        && item.CreditLimitNotifications != CreditLimitNotifications.MoreThanFourty):
                        await SendNotificationsIfNeed(item, CreditLimitNotifications.MoreThanFourty, cancellationToken);
                        break;
                    default:
                        break;
                }
            }

            return Result.Success();
        }


        private async Task SendNotificationsIfNeed(AgencyBalanceLimitInfo agencyBalanceLimitInfo,
            CreditLimitNotifications targetNotification, CancellationToken cancellationToken)
        {
            var agency = agencyBalanceLimitInfo.Agency;

            if (targetNotification != CreditLimitNotifications.MoreThanFourty)
            {
                var masterAgents = await (
                    from a in _context.Agents
                    join rel in _context.AgentAgencyRelations on a.Id equals rel.AgentId
                    where rel.AgencyId == agency.Id && rel.Type == AgentAgencyRelationTypes.Master
                    select a).ToListAsync(cancellationToken);

                var messageData = new CreditLimitData
                {
                    Percentage = (int)targetNotification,
                    AgencyName = agency!.Name,
                    ContactDetails = agency!.BillingEmail
                };

                foreach (var agent in masterAgents)
                    await _notificationService.Send(new SlimAgentContext(agent.Id, agency.Id),
                        messageData,
                        NotificationTypes.CreditLimitRunOutBalance,
                        agent.Email);

                if (agency.BillingEmail is not null)
                    await _notificationService.Send(messageData, NotificationTypes.CreditLimitRunOutBalance, agency.BillingEmail);
            }

            agency!.CreditLimitNotifications = targetNotification;

            _context.Agencies.Update(agency);
            await _context.SaveChangesAsync(cancellationToken);
        }


        private Func<Agency, AgencyAccount, AgencyBalanceLimitInfo> ToBalanceLimitProjection()
            => (agency, account)
                => new AgencyBalanceLimitInfo(agency, account);


        private readonly EdoContext _context;
        private readonly INotificationService _notificationService;
    }
}