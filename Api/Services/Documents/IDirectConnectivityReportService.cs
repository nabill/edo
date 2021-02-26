using System;
using System.IO;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;

namespace HappyTravel.Edo.Api.Services.Documents
{
    public interface IDirectConnectivityReportService
    {
        public Task<Result> GetReport(Stream stream, DateTime fromDate, DateTime endDate);
    }
}