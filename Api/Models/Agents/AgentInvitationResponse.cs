using HappyTravel.Edo.Common.Enums;
using Newtonsoft.Json;

namespace HappyTravel.Edo.Api.Models.Agents
{
    public readonly struct AgentInvitationResponse
    {
        [JsonConstructor]
        public AgentInvitationResponse(string id, string title, string firstName, string lastName,
            string position, string email, string createdBy, string created, UserInvitationStatuses status)
        {
            Id = id;
            Title = title;
            FirstName = firstName;
            LastName = lastName;
            Position = position;
            Email = email;
            CreatedBy = createdBy;
            Created = created;
            Status = status;
        }

        public string Id { get; }
        public string FirstName { get; }
        public string LastName { get; }
        public string Title { get; }
        public string Position { get; }
        public string Email { get; }
        public string CreatedBy { get; }
        public string Created { get; }
        public UserInvitationStatuses Status { get; }
    }
}