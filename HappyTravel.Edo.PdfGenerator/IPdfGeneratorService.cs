using CSharpFunctionalExtensions;

namespace HappyTravel.Edo.PdfGenerator;

public interface IPdfGeneratorService
{
    Task<Result<byte[]>> Generate<T>(T model);
}