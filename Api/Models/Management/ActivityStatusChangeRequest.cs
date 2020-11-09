using HappyTravel.Edo.Api.Models.Management.Enums;

namespace HappyTravel.Edo.Api.Models.Management
{
    public readonly struct ActivityStatusChangeRequest
    {

        public ActivityStatusChangeRequest(string reason)
        {
            Reason = reason;
        }
        public string Reason { get; }
    }
}