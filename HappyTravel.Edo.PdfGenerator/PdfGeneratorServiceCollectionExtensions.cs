using HappyTravel.Edo.PdfGenerator.WeasyprintClient;
using Microsoft.Extensions.DependencyInjection;

namespace HappyTravel.Edo.PdfGenerator;

public static class PdfGeneratorServiceCollectionExtensions
{
    public static IServiceCollection AddPdfGenerator(this IServiceCollection services)
    {
        services.AddSingleton<IPdfGeneratorService, PdfGeneratorService>();
        return services;
    }
}