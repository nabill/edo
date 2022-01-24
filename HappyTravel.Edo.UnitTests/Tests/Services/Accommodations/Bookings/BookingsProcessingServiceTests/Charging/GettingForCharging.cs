using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using HappyTravel.Edo.Api.Infrastructure;
using HappyTravel.Edo.Api.Services.Accommodations.Bookings;
using HappyTravel.Edo.Api.Services.Accommodations.Bookings.BatchProcessing;
using HappyTravel.Edo.Api.Services.Accommodations.Bookings.Mailing;
using HappyTravel.Edo.Api.Services.Accommodations.Bookings.Management;
using HappyTravel.Edo.Api.Services.Accommodations.Bookings.Payments;
using HappyTravel.Edo.Common.Enums;
using HappyTravel.Edo.Data.Bookings;
using HappyTravel.Edo.UnitTests.Utility;
using Moq;
using Xunit;

namespace HappyTravel.Edo.UnitTests.Tests.Services.Accommodations.Bookings.BookingsProcessingServiceTests.Charging
{
    public class GettingForCharging
    {
        [Fact]
        public async Task Should_return_bookings_within_given_deadline()
        {
            var date = new DateTime(2021, 12, 3);
            var bookings = new []
            {
                CreateBooking(id: 1, deadlineDate: new DateTime(2021, 12, 2)),
                CreateBooking(id: 2, deadlineDate: new DateTime(2021, 12, 3)),
                CreateBooking(id: 3, deadlineDate: new DateTime(2021, 12, 4)),
                CreateBooking(id: 4, deadlineDate: new DateTime(2021, 12, 5)),
                CreateBooking(id: 5, deadlineDate: null)
            };
            var service = CreateProcessingService(bookings);

            var bookingsToCapture = await service.GetForCharge(date);

            Assert.Contains(1, bookingsToCapture);
            Assert.Contains(2, bookingsToCapture);
            Assert.DoesNotContain(3, bookingsToCapture);
            Assert.DoesNotContain(4, bookingsToCapture);
            Assert.DoesNotContain(5, bookingsToCapture);

            static Booking CreateBooking(int id, DateTime? deadlineDate) => new Booking
            {
                Id = id, 
                PaymentStatus = BookingPaymentStatuses.NotPaid,
                Status = BookingStatuses.Confirmed,
                PaymentType = PaymentTypes.VirtualAccount,
                DeadlineDate = deadlineDate,
                CheckInDate = DateTime.MaxValue
            };
        }
        
        [Fact]
        public async Task Should_return_bookings_within_valid_payment_methods()
        {
            var date = new DateTime(2021, 12, 2);
            var bookings = new []
            {
                CreateBooking(id: 1, paymentMethod: PaymentTypes.Offline),
                CreateBooking(id: 2, paymentMethod: PaymentTypes.None),
                CreateBooking(id: 3, paymentMethod: PaymentTypes.VirtualAccount),
                CreateBooking(id: 4, paymentMethod: PaymentTypes.CreditCard),
            };
            var service = CreateProcessingService(bookings);

            var bookingsToCapture = await service.GetForCharge(date);

            Assert.DoesNotContain(1, bookingsToCapture);
            Assert.DoesNotContain(2, bookingsToCapture);
            Assert.Contains(3, bookingsToCapture);
            Assert.DoesNotContain(4, bookingsToCapture);

            static Booking CreateBooking(int id, PaymentTypes paymentMethod) => new Booking
            {
                Id = id, 
                PaymentStatus = BookingPaymentStatuses.NotPaid,
                Status = BookingStatuses.Confirmed,
                PaymentType = paymentMethod,
                DeadlineDate = DateTimeOffset.MinValue,
                CheckInDate = DateTimeOffset.MinValue,
                Created = DateTimeOffset.MinValue,
            };
        }
        
        
        [Fact]
        public async Task Should_return_not_payed_bookings()
        {
            var date = new DateTime(2021, 12, 2);
            var bookings = new []
            {
                CreateBooking(id: 1, paymentStatus: BookingPaymentStatuses.Authorized),
                CreateBooking(id: 2, paymentStatus: BookingPaymentStatuses.Captured),
                CreateBooking(id: 3, paymentStatus: BookingPaymentStatuses.Refunded),
                CreateBooking(id: 4, paymentStatus: BookingPaymentStatuses.Voided),
                CreateBooking(id: 5, paymentStatus: BookingPaymentStatuses.NotPaid)
            };
            var service = CreateProcessingService(bookings);

            var bookingsToCapture = await service.GetForCharge(date);

            Assert.DoesNotContain(1, bookingsToCapture);
            Assert.DoesNotContain(2, bookingsToCapture);
            Assert.DoesNotContain(3, bookingsToCapture);
            Assert.DoesNotContain(4, bookingsToCapture);
            Assert.Contains(5, bookingsToCapture);

            static Booking CreateBooking(int id, BookingPaymentStatuses paymentStatus) => new Booking
            {
                Id = id, 
                PaymentStatus = paymentStatus,
                Status = BookingStatuses.Confirmed,
                PaymentType = PaymentTypes.VirtualAccount,
                DeadlineDate = DateTimeOffset.MinValue,
                CheckInDate = DateTimeOffset.MinValue
            };
        }

        
        [Fact]
        public async Task Should_return_confirmed_or_pending_bookings()
        {
            var date = new DateTime(2021, 12, 2);
            var bookings = new []
            {
                CreateBooking(id: 1, statusCode: BookingStatuses.Cancelled),
                CreateBooking(id: 2, statusCode: BookingStatuses.Confirmed),
                CreateBooking(id: 3, statusCode: BookingStatuses.Invalid),
                CreateBooking(id: 4, statusCode: BookingStatuses.Pending),
                CreateBooking(id: 5, statusCode: BookingStatuses.Rejected),
                CreateBooking(id: 6, statusCode: BookingStatuses.WaitingForResponse)
            };
            var service = CreateProcessingService(bookings);

            var bookingsToCapture = await service.GetForCharge(date);

            Assert.DoesNotContain(1, bookingsToCapture);
            Assert.Contains(2, bookingsToCapture);
            Assert.DoesNotContain(3, bookingsToCapture);
            Assert.Contains(4, bookingsToCapture);
            Assert.DoesNotContain(5, bookingsToCapture);
            Assert.Contains(6, bookingsToCapture);

            static Booking CreateBooking(int id, BookingStatuses statusCode) => new Booking
            {
                Id = id, 
                PaymentStatus = BookingPaymentStatuses.NotPaid,
                Status = statusCode,
                PaymentType = PaymentTypes.VirtualAccount,
                DeadlineDate = DateTimeOffset.MinValue,
                CheckInDate = DateTimeOffset.MinValue
            };
        }
        

        private BookingsProcessingService CreateProcessingService(IEnumerable<Booking> bookings)
        {
            var context = MockEdoContextFactory.Create();
            context.Setup(c => c.Bookings)
                .Returns(DbSetMockProvider.GetDbSetMock(bookings));

            var service = new BookingsProcessingService(Mock.Of<IBookingAccountPaymentService>(),
                Mock.Of<IBookingCreditCardPaymentService>(),
                Mock.Of<ISupplierBookingManagementService>(),
                Mock.Of<IBookingNotificationService>(),
                Mock.Of<IBookingReportsService>(),
                context.Object,
                Mock.Of<IBookingRecordsUpdater>(),
                Mock.Of<IDateTimeProvider>());
            
            return service;
        }
    }
}