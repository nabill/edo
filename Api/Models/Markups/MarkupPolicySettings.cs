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
        
        public string Description { get; }
        public int TemplateId { get; }
        public Dictionary<string, decimal> TemplateSettings { get; }
        public int Order { get; }
    }
}