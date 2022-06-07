using System;
using System.Threading.Tasks;

namespace HappyTravel.Edo.Api.Services.Accommodations.Availability;

public interface IEvaluationTokenStorage
{
    Task<string> GetAndSet(Guid roomContractSetId);
    Task<bool> IsExists(string evaluationToken, Guid roomContractSetId);
}