using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using HappyTravel.Edo.Common.Enums;

namespace HappyTravel.Edo.Api.Services.Management
{
    public interface IManagementAuditService
    {
        Task<Result> Write(ManagementEventType eventType, string eventData);
    }
}