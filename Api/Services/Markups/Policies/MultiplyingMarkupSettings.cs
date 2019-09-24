using Newtonsoft.Json;

namespace HappyTravel.Edo.Api.Services.Markups.Policies
{
    public readonly struct MultiplyingMarkupSettings
    {
        [JsonConstructor]
        public MultiplyingMarkupSettings(decimal factor)
        {
            Factor = factor;
        }
        
        public decimal Factor { get; }
    }
}