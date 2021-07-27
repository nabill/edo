using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace HappyTravel.Edo.Api.Services.Reports.RecordManagers
{
    public interface IRecordManager<TData>
    {
        Task<IEnumerable<TData>> Get(DateTime fromDate, DateTime endDate);
    }
}