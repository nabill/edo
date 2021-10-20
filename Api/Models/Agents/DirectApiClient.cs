namespace HappyTravel.Edo.Api.Models.Agents
{
    public readonly struct DirectApiClient
    {
        public DirectApiClient(string clientId, string description, string password, string isActive)
        {
            ClientId = clientId;
            Description = description;
            Password = password;
            IsActive = isActive;
        }


        public string ClientId { get; }
        public string Description { get; }
        public string Password { get; }
        public string IsActive { get; }
    }
}