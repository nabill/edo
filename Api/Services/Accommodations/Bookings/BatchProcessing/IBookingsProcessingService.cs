using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using HappyTravel.Edo.Api.Models.Bookings;
using HappyTravel.Edo.Data.Management;

namespace HappyTravel.Edo.Api.Services.Accommodations.Bookings.BatchProcessing
{
    public interface IBookingsProcessingService
    {
        Task<List<int>> GetForCapture(DateTimeOffset date);

        Task<Result<BatchOperationResult>> Capture(List<int> bookingIds, ServiceAccount serviceAccount);

        Task<List<int>> GetForCharge(DateTimeOffset date);

        Task<Result<BatchOperationResult>> Charge(List<int> bookingIds, ServiceAccount serviceAccount);

        Task<List<int>> GetForNotification(DateTimeOffset date);

        Task<Result<BatchOperationResult>> NotifyDeadlineApproaching(List<int> bookingIds, ServiceAccount serviceAccount);

        Task<Result<BatchOperationResult>> NotifyOfflineDeadlineApproaching();

        Task<List<int>> GetForCancellation(DateTimeOffset date);

        Task<Result<BatchOperationResult>> Cancel(List<int> bookingIds, ServiceAccount serviceAccount);

        Task<BatchOperationResult> SendBookingSummaryReports();
    }
}