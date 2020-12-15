using Newtonsoft.Json;

namespace HappyTravel.Edo.Api.Models.Markups
{
    public readonly struct MarkupInfo
    {
        [JsonConstructor]
        public MarkupInfo(int id, MarkupPolicySettings settings)
        {
            Id = id;
            Settings = settings;
        }
        
        /// <summary>
        /// Policy id
        /// </summary>
        public int Id { get; }
        
        /// <summary>
        /// Policy settings
        /// </summary>
        public MarkupPolicySettings Settings { get; }
    }
}