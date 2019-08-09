using System;
using System.Threading;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using Microsoft.AspNetCore.Mvc;

namespace HappyTravel.Edo.Api.Infrastructure
{
    public interface INetClient
    {
        Task<Result<TOut, ProblemDetails>> Post<T, TOut>(Uri url, T requestContent, string languageCode = LocalizationHelper.DefaultLanguageCode, CancellationToken cancellationToken = default);
    }
}