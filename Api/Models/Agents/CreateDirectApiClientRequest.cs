namespace HappyTravel.Edo.Api.Models.Agents
{
    public readonly struct CreateDirectApiClientRequest
    {
        public CreateDirectApiClientRequest(string clientId, string name, string password)
        {
            ClientId = clientId;
            Name = name;
            Password = password;
        }


        public string ClientId { get; }
        public string Name { get; }
        public string Password { get; }
    }
}