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
                var datetime = DateTimeOffset.UtcNow;
                client.IndexAsync<BytesResponse>($"accommodation-availability-requested-{datetime:d}", PostData.Serializable(accommodationName));
            };
        }


        public static void LogAccommodationAvailabilityRequested(this IElasticLowLevelClient client, object accommodationName) 
            => AccommodationAvailabilityRequested(client, accommodationName);


        private static readonly Action<IElasticLowLevelClient, object> AccommodationAvailabilityRequested;
    }
}
