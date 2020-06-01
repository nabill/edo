using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using HappyTravel.Edo.Api.Models.Bookings;
using HappyTravel.Edo.Data.Management;

namespace HappyTravel.Edo.Api.Services.Accommodations.Bookings
{
    public interface IBookingsProcessingService
    {
        Task<Result<List<int>>> GetForCapture(DateTime deadlineDate);

        Task<Result<ProcessResult>> Capture(List<int> bookingIds, ServiceAccount serviceAccount);
        
        Task<Result<List<int>>> GetForNotification(DateTime deadlineDate);

        Task<Result<ProcessResult>> NotifyDeadlineApproaching(List<int> bookingIds, ServiceAccount serviceAccount);

        Task<Result<List<int>>> GetForCancellation(DateTime deadlineDate);

        Task<Result<ProcessResult>> Cancel(List<int> bookingIds, ServiceAccount serviceAccount);
    }
}