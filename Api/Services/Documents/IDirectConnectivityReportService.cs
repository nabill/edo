using System;
using System.IO;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using HappyTravel.Edo.Common.Enums;

namespace HappyTravel.Edo.Api.Services.Documents
{
    public interface IDirectConnectivityReportService
    {
        public Task<Result<Stream>> GetSupplierWiseReport(Suppliers supplier, DateTime fromDate, DateTime endDate);

        public Task<Result<Stream>> GetAgencyWiseReport(int agencyId, DateTime fromDate, DateTime endDate);
    }
}