using System;
using System.IO;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;

namespace HappyTravel.Edo.Api.Services.Reports
{
    public interface IDirectConnectivityReportService
    {
        public Task<Result<Stream>> GetSupplierWiseReport(DateTime fromDate, DateTime endDate);

        public Task<Result<Stream>> GetAgencyWiseReport(DateTime fromDate, DateTime endDate);

        public Task<Result<Stream>> GetFullBookingsReport(DateTime fromDate, DateTime endTime);
    }
}