using System;

namespace HappyTravel.Edo.Api.Models.Availabilities
{
    public readonly struct SearchStateChangedToastMessage
    {
        public SearchStateChangedToastMessage(Guid searchId)
        {
            SearchId = searchId;
        }
        
        public Guid SearchId { get; }
    }
}