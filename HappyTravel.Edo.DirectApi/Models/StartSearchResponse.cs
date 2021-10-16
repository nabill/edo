using System;

namespace HappyTravel.Edo.DirectApi.Models
{
    public readonly struct StartSearchResponse
    {
        public StartSearchResponse(Guid searchId)
        {
            SearchId = searchId;
        }

        public Guid SearchId { get; }
    }
}