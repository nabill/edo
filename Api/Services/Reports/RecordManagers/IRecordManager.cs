using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace HappyTravel.Edo.Api.Services.Reports.RecordManagers
{
    public interface IRecordManager<TProjection>
    {
        Task<IEnumerable<TProjection>> Get(DateTime fromDate, DateTime endDate);
    }
}