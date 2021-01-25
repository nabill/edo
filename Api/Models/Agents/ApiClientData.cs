using Newtonsoft.Json;

namespace HappyTravel.Edo.Api.Models.Agents
{
    public readonly struct ApiClientData
    {
        [JsonConstructor]
        public ApiClientData(string name, string password)
        {
            Name = name;
            Password = password;
        }
        
        /// <summary>
        /// Client name
        /// </summary>
        public string Name { get; }
        
        /// <summary>
        /// Client password
        /// </summary>
        public string Password { get; }
    }
}