using System;
using Elasticsearch.Net;
using HappyTravel.Edo.Api.Infrastructure.Logging;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Logging;

namespace HappyTravel.Edo.Api.Infrastructure.Analytics
{
    public class AnalyticsService : IAnalyticsService
    {
        public AnalyticsService(IElasticLowLevelClient elasticClient,
            IDateTimeProvider dateTimeProvider, 
            IWebHostEnvironment environment,
            ILogger<AnalyticsService> logger)
        {
            _elasticClient = elasticClient;
            _dateTimeProvider = dateTimeProvider;
            _environment = environment;
            _logger = logger;
        }


        public void LogEvent(object eventData, string name)
        {
            var date = new DateTimeOffset(_dateTimeProvider.UtcNow(), TimeSpan.Zero);
            var environmentName = _environment.EnvironmentName.ToLowerInvariant();
            
            var indexName = $"{environmentName}-{ServicePrefix}-{name}-{date:yyyy-MM-dd}";
           
            _elasticClient.IndexAsync<BytesResponse>(indexName, PostData.Serializable(eventData))
                .ContinueWith(task =>
                {
                    if (task.IsFaulted)
                    {
                       _logger.LogElasticAnalyticsEventSendError($"Error executing task: {task.Exception?.Message}");
                       return;
                    }

                    var response = task.Result;
                    if(!response.Success)
                        _logger.LogElasticAnalyticsEventSendError($"Error sending request: {response.OriginalException.Message}");
                });
        }
        
        
        private const string ServicePrefix = "edo";
        
        private readonly IElasticLowLevelClient _elasticClient;
        private readonly IDateTimeProvider _dateTimeProvider;
        private readonly IWebHostEnvironment _environment;
        private readonly ILogger<AnalyticsService> _logger;
    }
}