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
    }
}