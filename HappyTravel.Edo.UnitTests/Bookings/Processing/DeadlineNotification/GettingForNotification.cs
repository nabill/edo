using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using HappyTravel.Edo.Api.Services.Accommodations.Bookings;
using HappyTravel.Edo.Api.Services.Mailing;
using HappyTravel.Edo.Common.Enums;
using HappyTravel.Edo.Data.Booking;
using HappyTravel.Edo.UnitTests.Infrastructure;
using HappyTravel.Edo.UnitTests.Infrastructure.DbSetMocks;
using HappyTravel.EdoContracts.Accommodations.Enums;
using HappyTravel.EdoContracts.General.Enums;
using Moq;
using Xunit;

namespace HappyTravel.Edo.UnitTests.Bookings.Processing.DeadlineNotification
{
    public class GettingForNotification
    {
        private BookingsProcessingService CreateProcessingService(IEnumerable<Booking> bookings)
        {
            var context = MockEdoContext.Create();
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
            var date = new DateTime(2021, 12, 5);
            var bookings = new[]
            {
                CreateBooking(1, new DateTime(2021, 12, 1)),
                CreateBooking(2, new DateTime(2021, 12, 2)),
                CreateBooking(3, new DateTime(2021, 12, 3)),
                CreateBooking(4, new DateTime(2021, 12, 4)),
                CreateBooking(5, new DateTime(2021, 12, 5)),
                CreateBooking(6, new DateTime(2021, 12, 6))
            };
            var service = CreateProcessingService(bookings);

            var bookingsToNotify = await service.GetForNotification(date);

            Assert.DoesNotContain(1, bookingsToNotify);
            Assert.Contains(2, bookingsToNotify);
            Assert.DoesNotContain(3, bookingsToNotify);
            Assert.DoesNotContain(4, bookingsToNotify);
            Assert.DoesNotContain(5, bookingsToNotify);
            Assert.DoesNotContain(6, bookingsToNotify);


            static Booking CreateBooking(int id, DateTime checkInDate)
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
            var date = new DateTime(2021, 12, 5);
            var bookings = new[]
            {
                CreateBooking(1, new DateTime(2021, 12, 1)),
                CreateBooking(2, new DateTime(2021, 12, 2)),
                CreateBooking(3, new DateTime(2021, 12, 3)),
                CreateBooking(4, new DateTime(2021, 12, 4)),
                CreateBooking(5, new DateTime(2021, 12, 5)),
                CreateBooking(6, new DateTime(2021, 12, 6)),
                CreateBooking(7, null)
            };
            var service = CreateProcessingService(bookings);

            var bookingsToNotify = await service.GetForNotification(date);

            Assert.DoesNotContain(1, bookingsToNotify);
            Assert.Contains(2, bookingsToNotify);
            Assert.DoesNotContain(3, bookingsToNotify);
            Assert.DoesNotContain(4, bookingsToNotify);
            Assert.DoesNotContain(5, bookingsToNotify);
            Assert.DoesNotContain(6, bookingsToNotify);


            static Booking CreateBooking(int id, DateTime? deadlineDate)
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
            var date = new DateTime(2021, 12, 5);
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

            var bookingsToNotify = await service.GetForNotification(date);
            var bookingsToCapture = await service.GetForCapture(date.AddDays(3));

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