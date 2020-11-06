using HappyTravel.Edo.Api.Models.Management.Enums;

namespace HappyTravel.Edo.Api.Models.Management
{
    public readonly struct ActivityStatusChangeRequest
    {

        public ActivityStatusChangeRequest(ActivityStatus status, string reason)
        {
            Status = status;
            Reason = reason;
        }
        
        public ActivityStatus Status {get;}
        public string Reason { get; }
    }
}