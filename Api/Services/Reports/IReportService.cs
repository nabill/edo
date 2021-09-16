using System;
using System.IO;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;

namespace HappyTravel.Edo.Api.Services.Reports
{
    public interface IReportService
    {
        public Task<Result<Stream>> GetPayableToSupplierReport(DateTime fromDate, DateTime endDate);

        public Task<Result<Stream>> GetAgencyWiseReport(DateTime fromDate, DateTime endDate);

        public Task<Result<Stream>> GetFullBookingsReport(DateTime fromDate, DateTime endDate);

        public Task<Result<Stream>> GetSalesBookingsReport(DateTime fromDate, DateTime endDate);
        
        public Task<Result<Stream>> AgenciesProductivityReport(DateTime fromDate, DateTime endDate);
        
        public Task<Result<Stream>> PendingSupplierReferenceReport(DateTime fromDate, DateTime endDate);
        public Task<Result<Stream>> ConfirmedBookingsReport(DateTime fromDate, DateTime endDate);
        public Task<Result<Stream>> GetHotelWiseBookingReport(DateTime fromDate, DateTime endDate);
        public Task<Result<Stream>> GetCancellationDeadlineReport(DateTime fromDate, DateTime endDate);
        public Task<Result<Stream>> GetThirdPartySuppliersReport(DateTime fromDate, DateTime endDate);
        public Task<Result<Stream>> GetVccBookingReport(DateTime fromDate, DateTime endDate);
        public Task<Result<Stream>> GetCancelledBookingsReport(DateTime from, DateTime end);
        public Task<Result<Stream>> GetAgentWiseReport(DateTime fromDate, DateTime endDate);
        public Task<Result<Stream>> GetHotelProductivityReport(DateTime @from, DateTime end);
    }
}