using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using HappyTravel.Edo.Api.Models.Bookings;
using HappyTravel.Edo.Api.Models.Users;
using HappyTravel.Edo.Api.Services.Payments;
using HappyTravel.Edo.Data.Booking;
using HappyTravel.Edo.Data.Management;

namespace HappyTravel.Edo.Api.Services.Accommodations.Bookings
{
    public interface IBookingPaymentService : IPaymentsService
    {
        Task<Result<List<int>>> GetBookingsForCapture(DateTime deadlineDate);

        Task<Result<ProcessResult>> CaptureMoneyForBookings(List<int> bookingIds, ServiceAccount serviceAccount);

        Task<Result> VoidMoney(Booking booking, UserInfo user);

        Task<Result> CompleteOffline(int bookingId, Administrator administratorContext);

        Task<Result<ProcessResult>> NotifyPaymentsNeeded(List<int> bookingIds);
    }
}