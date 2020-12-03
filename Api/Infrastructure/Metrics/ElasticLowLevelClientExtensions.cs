using System;
using Elasticsearch.Net;

namespace HappyTravel.Edo.Api.Infrastructure.Metrics
{
    public static class ElasticLowLevelClientExtensions
    {
        static ElasticLowLevelClientExtensions()
        {
            AccommodationAvailabilityRequested = (client, accommodationName) =>
            {
                client.IndexAsync<BytesResponse>("accommodation-availability-requested", PostData.Serializable(accommodationName));
            };
        }


        public static void LogAccommodationAvailabilityRequested(this IElasticLowLevelClient client, object accommodationName) 
            => AccommodationAvailabilityRequested(client, accommodationName);


        private static readonly Action<IElasticLowLevelClient, object> AccommodationAvailabilityRequested;
    }
}
