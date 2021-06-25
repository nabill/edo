using System;

namespace HappyTravel.Edo.Api.Infrastructure.Logging
{
    public static class Baggage
    {
        public static void SetSearchId(Guid searchId)
        {
            OpenTelemetry.Baggage.Current.SetBaggage("SearchId", searchId.ToString());
        }
    }
}