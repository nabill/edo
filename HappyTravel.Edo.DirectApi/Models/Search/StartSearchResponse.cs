using System;
using System.Text.Json.Serialization;

namespace HappyTravel.Edo.DirectApi.Models.Search
{
    public readonly struct StartSearchResponse
    {
        [JsonConstructor]
        public StartSearchResponse(Guid searchId)
        {
            SearchId = searchId;
        }

        public Guid SearchId { get; }
    }
}