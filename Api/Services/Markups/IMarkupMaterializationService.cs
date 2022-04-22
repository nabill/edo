using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using HappyTravel.Edo.Api.Models.Bookings;
using HappyTravel.Edo.Api.Models.Markups;

namespace HappyTravel.Edo.Api.Services.Markups
{
    public interface IMarkupBonusMaterializationService
    {
        Task<List<int>> GetForMaterialize(DateTimeOffset dateTime);

        Task<Result<BatchOperationResult>> Materialize(List<int> appliedMarkups);
    }
}