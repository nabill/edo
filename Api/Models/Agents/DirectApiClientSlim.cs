namespace HappyTravel.Edo.Api.Models.Agents
{
    public readonly struct DirectApiClientSlim
    {
        public DirectApiClientSlim(string clientId, string name, string isActive)
        {
            ClientId = clientId;
            Name = name;
            IsActive = isActive;
        }


        public string ClientId { get; }
        public string Name { get; }
        public string IsActive { get; }
    }
}