using System.Collections.Generic;
using Newtonsoft.Json;

namespace HappyTravel.Edo.Api.Models.Markups
{
    public readonly struct MarkupPolicySettings
    {
        [JsonConstructor]
        public MarkupPolicySettings(string description, int templateId,
            Dictionary<string, decimal> templateSettings, int order)
        {
            Description = description;
            TemplateId = templateId;
            TemplateSettings = templateSettings;
            Order = order;
        }
        
        /// <summary>
        /// Policy description.
        /// </summary>
        public string Description { get; }
        
        /// <summary>
        /// Template id.
        /// </summary>
        public int TemplateId { get; }
        
        /// <summary>
        /// Settings.
        /// </summary>
        public Dictionary<string, decimal> TemplateSettings { get; }
        
        /// <summary>
        /// Order.
        /// </summary>
        public int Order { get; }
    }
}