using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;

namespace HappyTravel.Edo.PdfGenerator.WeasyprintClient;

public static class WeasyprintClientServiceCollectionExtensions
{
    public static IServiceCollection AddWeasyprintClient(this IServiceCollection services, Action<WeasyprintClientOptions> options)
    {
        services.AddSingleton<IWeasyprintClient, WeasyprintClient>();
        services.Configure(options);
        return services;
    }
}