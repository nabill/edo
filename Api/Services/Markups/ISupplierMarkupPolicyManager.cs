using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Api.Models.Markups.Supplier;
using CSharpFunctionalExtensions;
using HappyTravel.Edo.Api.Models.Markups;

namespace Api.Services.Markups
{
    public interface ISupplierMarkupPolicyManager
    {
        Task<Result> Add(SupplierMarkupRequest request, CancellationToken cancellationToken);
        Task<Result> Modify(int policyId, SupplierMarkupRequest request, CancellationToken cancellationToken);
        Task<Result> Remove(int policyId, CancellationToken cancellationToken);
        Task<List<MarkupInfo>> Get(CancellationToken cancellationToken);
    }
}