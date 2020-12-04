using System;
using Elasticsearch.Net;
using HappyTravel.Edo.Api.Models.Analytics.Events;

namespace HappyTravel.Edo.Api.Infrastructure.Analytics
{
    public static class ElasticLowLevelClientExtensions
    {
        static ElasticLowLevelClientExtensions()
        {
            AccommodationAvailabilityRequested = (client, accommodationName) =>
            {
                var datetime = DateTimeOffset.UtcNow;
                client.IndexAsync<BytesResponse>($"accommodation-availability-requested-{datetime:yyyy-MM-dd}", PostData.Serializable(accommodationName));
            };
        }


        public static void LogAccommodationAvailabilityRequested(this IElasticLowLevelClient client, AccommodationAvailabilityRequestEvent accommodationInfo) 
            => AccommodationAvailabilityRequested(client, accommodationInfo);


        private static readonly Action<IElasticLowLevelClient, object> AccommodationAvailabilityRequested;
    }
}
