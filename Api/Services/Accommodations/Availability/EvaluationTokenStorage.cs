using System;
using System.Threading.Tasks;
using FloxDc.CacheFlow;
using FloxDc.CacheFlow.Extensions;
using shortid;
using shortid.Configuration;

namespace HappyTravel.Edo.Api.Services.Accommodations.Availability;

public class EvaluationTokenStorage : IEvaluationTokenStorage
{
    public EvaluationTokenStorage(IDistributedFlow flow)
    {
        _flow = flow;
    }

    
    public async Task<string> GetAndSet(Guid roomContractSetId)
    {
        var evaluationToken = ShortId.Generate(Options);
        await _flow.SetAsync(GetKey(roomContractSetId), evaluationToken, Expiration);
        return evaluationToken;
    }


    public async Task<bool> IsExists(string evaluationToken, Guid roomContractSetId)
    {
        var value = await _flow.GetAsync<string>(GetKey(roomContractSetId));
        return value == evaluationToken;
    }


    private string GetKey(Guid roomContractSetId) 
        => _flow.BuildKey(nameof(EvaluationTokenStorage), roomContractSetId.ToString());


    private static readonly GenerationOptions Options = new(useSpecialCharacters: false);
    private static readonly TimeSpan Expiration = TimeSpan.FromHours(1);
    private readonly IDistributedFlow _flow;
}