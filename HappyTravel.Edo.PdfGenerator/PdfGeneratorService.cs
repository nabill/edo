using System.Collections.Concurrent;
using CSharpFunctionalExtensions;
using HappyTravel.Edo.PdfGenerator.WeasyprintClient;
using RazorEngineCore;

namespace HappyTravel.Edo.PdfGenerator;

public class PdfGeneratorService : IPdfGeneratorService
{
    public PdfGeneratorService(IWeasyprintClient weasyprintClient)
    {
        _weasyprintClient = weasyprintClient;
    }


    public async Task<Result<byte[]>> Generate<T>(T model)
    {
       return await RazorEngineCompiledTemplate(typeof(T).Name)
                .Bind(async templateTask => Result.Success(await templateTask))
                .Bind(async template => Result.Success(await template.RunAsync(model)))
                .Bind(htmlString => _weasyprintClient.GeneratePdf(htmlString));
    }


    private Result<Task<IRazorEngineCompiledTemplate>> RazorEngineCompiledTemplate(string key)
    {
        var templatePath = Path.Combine(AppContext.BaseDirectory, "Templates", $"{key}.cshtml");
        if (!File.Exists(templatePath))
            return Result.Failure<Task<IRazorEngineCompiledTemplate>>($"Template file not found. Template name - {key}");

        return _templateCache.GetOrAdd(key.GetHashCode(), _ =>
        {
            IRazorEngine razorEngine = new RazorEngine();
            var template = File.ReadAllText(templatePath);
            return razorEngine.CompileAsync(template);
        });
    }


    private readonly IWeasyprintClient _weasyprintClient;
    private readonly ConcurrentDictionary<int, Task<IRazorEngineCompiledTemplate>> _templateCache = new();
}