using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using HappyTravel.Edo.Api.Infrastructure;
using HappyTravel.Edo.Api.Services.Management;
using HappyTravel.Edo.Common.Enums;
using HappyTravel.Edo.Data;
using HappyTravel.Edo.Data.Payments;
using Newtonsoft.Json;

namespace HappyTravel.Edo.Api.Services.Payments
{
    public class AccountAuditService : IAccountAuditService
    {
        public AccountAuditService(EdoContext context,
            IDateTimeProvider dateTimeProvider,
            IAdministratorContext administratorContext)
        {
            _context = context;
            _dateTimeProvider = dateTimeProvider;
            _administratorContext = administratorContext;
        }
        
        public async Task<Result> Write<TEventData>(AccountEventType eventType, TEventData eventData)
        {
            var (_, isFailure, admin, error) = await _administratorContext.GetCurrent();
            if(isFailure)
                return Result.Fail(error);
            var logEntry = new AccountAuditLogEntry
            {
                Created = _dateTimeProvider.UtcNow(),
                Type = eventType,
                AdministratorId = admin.Id,
                EventData = JsonConvert.SerializeObject(eventData)
            };

            _context.AccountAuditLog.Add(logEntry);
            await _context.SaveChangesAsync();
            return Result.Ok();
        }
        
        private readonly EdoContext _context;
        private readonly IDateTimeProvider _dateTimeProvider;
        private readonly IAdministratorContext _administratorContext;
    }
}
