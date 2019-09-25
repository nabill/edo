using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using Newtonsoft.Json;

namespace HappyTravel.Edo.Api.Services.Markups.Templates
{
    public class MarkupPolicyTemplate
    {
        public int Id { get; set; }
        public string Title { get; set; }
        
        [JsonIgnore]
        public Func<IDictionary<string, decimal>, Expression<Func<decimal, decimal>>> ExpressionFactory { get; set; } 
        
        [JsonIgnore]
        public Func<IDictionary<string, decimal>, bool> SettingsValidator { get; set; } 
    }
}