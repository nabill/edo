using System.Collections.Generic;
using HappyTravel.Edo.Common.Enums.Markup;
using HappyTravel.Money.Enums;
using Newtonsoft.Json;

namespace HappyTravel.Edo.Api.Models.Markups
{
    public readonly struct MarkupPolicySettings
    {
        [JsonConstructor]
        public MarkupPolicySettings(
            string description, 
            int templateId,
            IDictionary<string, decimal> templateSettings, 
            int order, 
            Currencies currency,
            DestinationMarkupScopeTypes destinationScopeType = DestinationMarkupScopeTypes.NotSpecified,
            string destinationScopeId = ""
            )
        {
            Description = description;
            TemplateId = templateId;
            TemplateSettings = templateSettings;
            Order = order;
            Currency = currency;
            DestinationScopeType = destinationScopeType;
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
        ///     Type of destination markup scope
        /// </summary>
        public DestinationMarkupScopeTypes DestinationScopeType { get; }
        
        
        /// <summary>
        ///     Id of scope for destination. Could be Locality_XXXX, City_XXXX, Country_XXXX
        /// </summary>
        public string DestinationScopeId { get; }
    }
}