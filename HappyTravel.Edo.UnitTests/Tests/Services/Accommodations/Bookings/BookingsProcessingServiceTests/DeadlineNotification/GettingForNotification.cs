using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using HappyTravel.Edo.Api.Services.Accommodations.Bookings;
using HappyTravel.Edo.Api.Services.Mailing;
using HappyTravel.Edo.Common.Enums;
using HappyTravel.Edo.Data.Booking;
using HappyTravel.Edo.UnitTests.Utility;
using HappyTravel.EdoContracts.Accommodations.Enums;
using HappyTravel.EdoContracts.General.Enums;
using Moq;
using Xunit;

namespace HappyTravel.Edo.UnitTests.Tests.Services.Accommodations.Bookings.BookingsProcessingServiceTests.DeadlineNotification
{
    public class GettingForNotification
    {
        private BookingsProcessingService CreateProcessingService(IEnumerable<Booking> bookings)
        {
            var context = MockEdoContextFactory.Create();
            context.Setup(c => c.Bookings)
                .Returns(DbSetMockProvider.GetDbSetMock(bookings));

            var service = new BookingsProcessingService(Mock.Of<IBookingPaymentService>(),
                Mock.Of<IBookingService>(),
                Mock.Of<IBookingMailingService>(),
                context.Object);

            return service;
        }


        [Fact]
        public async Task Should_return_bookings__only_if_3_days_left_to_check_in_date()
        {
            var currentDate = new DateTime(2021, 12, 2);
            var bookings = new[]
            {
                CreateBookingWithCheckInDate(1, new DateTime(2021, 12, 1)),
                CreateBookingWithCheckInDate(2, new DateTime(2021, 12, 2)),
                CreateBookingWithCheckInDate(3, new DateTime(2021, 12, 3)),
                CreateBookingWithCheckInDate(4, new DateTime(2021, 12, 4)),
                CreateBookingWithCheckInDate(5, new DateTime(2021, 12, 5)),
                CreateBookingWithCheckInDate(6, new DateTime(2021, 12, 6))
            };
            var service = CreateProcessingService(bookings);

            var bookingsToNotify = await service.GetForNotification(currentDate);

            Assert.DoesNotContain(1, bookingsToNotify);
            Assert.DoesNotContain(2, bookingsToNotify);
            Assert.DoesNotContain(3, bookingsToNotify);
            Assert.DoesNotContain(4, bookingsToNotify);
            Assert.Contains(5, bookingsToNotify);
            Assert.DoesNotContain(6, bookingsToNotify);


            static Booking CreateBookingWithCheckInDate(int id, DateTime checkInDate)
                => new Booking
                {
                    Id = id,
                    PaymentStatus = BookingPaymentStatuses.Authorized,
                    Status = BookingStatusCodes.Confirmed,
                    PaymentMethod = PaymentMethods.BankTransfer,
                    DeadlineDate = null,
                    CheckInDate = checkInDate
                };
        }


        [Fact]
        public async Task Should_return_bookings__only_if_3_days_left_to_deadline()
        {
            var currentDate = new DateTime(2021, 12, 2);
            var bookings = new[]
            {
                CreateBookingWithDeadline(1, new DateTime(2021, 12, 1)),
                CreateBookingWithDeadline(2, new DateTime(2021, 12, 2)),
                CreateBookingWithDeadline(3, new DateTime(2021, 12, 3)),
                CreateBookingWithDeadline(4, new DateTime(2021, 12, 4)),
                CreateBookingWithDeadline(5, new DateTime(2021, 12, 5)),
                CreateBookingWithDeadline(6, new DateTime(2021, 12, 6)),
                CreateBookingWithDeadline(7, null)
            };
            var service = CreateProcessingService(bookings);

            var bookingsToNotify = await service.GetForNotification(currentDate);

            Assert.DoesNotContain(1, bookingsToNotify);
            Assert.DoesNotContain(2, bookingsToNotify);
            Assert.DoesNotContain(3, bookingsToNotify);
            Assert.DoesNotContain(4, bookingsToNotify);
            Assert.Contains(5, bookingsToNotify);
            Assert.DoesNotContain(6, bookingsToNotify);


            static Booking CreateBookingWithDeadline(int id, DateTime? deadlineDate)
                => new Booking
                {
                    Id = id,
                    PaymentStatus = BookingPaymentStatuses.Authorized,
                    Status = BookingStatusCodes.Confirmed,
                    PaymentMethod = PaymentMethods.BankTransfer,
                    DeadlineDate = deadlineDate,
                    CheckInDate = DateTime.MaxValue
                };
        }


        [Fact]
        public async Task Should_return_only_capturable_bookings()
        {
            var currentDate = new DateTime(2021, 12, 5);
            var bookings = new[]
            {
                CreateBooking(1, BookingPaymentStatuses.Authorized, BookingStatusCodes.Confirmed),
                CreateBooking(2, BookingPaymentStatuses.Captured, BookingStatusCodes.Confirmed),
                CreateBooking(3, BookingPaymentStatuses.Authorized, BookingStatusCodes.Cancelled),
                CreateBooking(4, BookingPaymentStatuses.NotPaid, BookingStatusCodes.Confirmed),
                CreateBooking(5, BookingPaymentStatuses.Refunded, BookingStatusCodes.Pending),
                CreateBooking(6, BookingPaymentStatuses.Voided, BookingStatusCodes.Rejected)
            };
            var service = CreateProcessingService(bookings);

            var bookingsToNotify = await service.GetForNotification(currentDate);
            var bookingsToCapture = await service.GetForCapture(currentDate.AddDays(3));

            Assert.True(bookingsToNotify.All(n => bookingsToCapture.Contains(n)));


            static Booking CreateBooking(int id, BookingPaymentStatuses paymentStatus, BookingStatusCodes status)
                => new Booking
                {
                    Id = id,
                    PaymentStatus = paymentStatus,
                    Status = status,
                    PaymentMethod = PaymentMethods.BankTransfer,
                    DeadlineDate = null,
                    CheckInDate = new DateTime(2021, 12, 2)
                };
        }
    }
}