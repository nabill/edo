using System;
using System.IO;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;

namespace HappyTravel.Edo.Api.Services.Reports
{
    public interface IAgenciesProductivityReportService
    {
        public Task<Result<Stream>> GetReport(DateTime fromDate, DateTime endDate);
    }
}