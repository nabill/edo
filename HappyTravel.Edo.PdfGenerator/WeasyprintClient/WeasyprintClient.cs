using CSharpFunctionalExtensions;
using Microsoft.Extensions.Options;

namespace HappyTravel.Edo.PdfGenerator.WeasyprintClient;

public class WeasyprintClient : IWeasyprintClient
{
    public WeasyprintClient(HttpClient httpClient, IOptions<WeasyprintClientOptions> options)
    {
        _httpClient = httpClient;
        _options = options.Value;
    }
   
    public async Task<Result<byte[]>> GeneratePdf(string html)
    {
        var request = new HttpRequestMessage(HttpMethod.Post, _options.WeasyprintEndpoint + "/pdf?filename=result.pdf")
        {
            Content = new StringContent(html)
        };
        
        using var response = await _httpClient.SendAsync(request);
        return await GetContent(response);
    }

    private static async Task<Result<byte[]>> GetContent(HttpResponseMessage response)
    {
        return response.IsSuccessStatusCode
            ? Result.Success(await response.Content.ReadAsByteArrayAsync())
            : Result.Failure<byte[]>(await response.Content.ReadAsStringAsync());
    }

    private readonly HttpClient _httpClient;
    private readonly WeasyprintClientOptions _options;
}