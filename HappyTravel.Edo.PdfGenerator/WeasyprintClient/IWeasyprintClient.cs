using CSharpFunctionalExtensions;

namespace HappyTravel.Edo.PdfGenerator.WeasyprintClient;

public interface IWeasyprintClient
{
    Task<Result<byte[]>> GeneratePdf(string html);
}