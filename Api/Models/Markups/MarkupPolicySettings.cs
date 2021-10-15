using System.Collections.Generic;
using HappyTravel.Money.Enums;
using Newtonsoft.Json;

namespace HappyTravel.Edo.Api.Models.Markups
{
    public readonly struct MarkupPolicySettings
    {
        [JsonConstructor]
        public MarkupPolicySettings(string description, int templateId, IDictionary<string, decimal> templateSettings, 
            int order, Currencies currency, string locationScopeId = "", string destinationScopeId = "")
        {
            Description = description;
            TemplateId = templateId;
            TemplateSettings = templateSettings;
            Order = order;
            Currency = currency;
            LocationScopeId = locationScopeId;
            DestinationScopeId = destinationScopeId;
        }


        /// <summary>
        ///     Policy description.
        /// </summary>
        public string Description { get; }

        /// <summary>
        ///     Template id.
        /// </summary>
        public int TemplateId { get; }

        /// <summary>
        ///     Settings.
        /// </summary>
        public IDictionary<string, decimal> TemplateSettings { get; }

        /// <summary>
        ///     Order.
        /// </summary>
        public int Order { get; }

        /// <summary>
        ///     Currency of policy. Needed for proper currency applying.
        /// </summary>
        public Currencies Currency { get; }
        
        
        /// <summary>
        ///     Location of agent from the mapper
        /// </summary>
        public string LocationScopeId { get; }
        
        /// <summary>
        ///     Destination of booking from the mapper
        /// </summary>
        public string DestinationScopeId { get; }
    }
}