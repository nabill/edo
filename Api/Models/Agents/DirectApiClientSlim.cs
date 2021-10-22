using System.Text.Json.Serialization;

namespace HappyTravel.Edo.Api.Models.Agents
{
    public readonly struct DirectApiClientSlim
    {
        [JsonConstructor]
        public DirectApiClientSlim(string id, string name, bool isActive)
        {
            Id = id;
            Name = name;
            IsActive = isActive;
        }


        public string Id { get; }
        public string Name { get; }
        public bool IsActive { get; }
    }
}