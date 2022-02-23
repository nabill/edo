using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using HappyTravel.Edo.Api.Infrastructure;
using HappyTravel.Edo.Api.Models.Users;
using HappyTravel.Edo.Api.Services.Accommodations.Bookings;
using HappyTravel.Edo.Api.Services.Accommodations.Bookings.BatchProcessing;
using HappyTravel.Edo.Api.Services.Accommodations.Bookings.Mailing;
using HappyTravel.Edo.Api.Services.Accommodations.Bookings.Management;
using HappyTravel.Edo.Api.Services.Accommodations.Bookings.Payments;
using HappyTravel.Edo.Api.Services.CodeProcessors;
using HappyTravel.Edo.Api.Services.SupplierOrders;
using HappyTravel.Edo.Common.Enums;
using HappyTravel.Edo.Data.Bookings;
using HappyTravel.Edo.Data.Management;
using HappyTravel.Edo.UnitTests.Utility;
using Moq;
using Xunit;

namespace HappyTravel.Edo.UnitTests.Tests.Services.Accommodations.Bookings.BookingsProcessingServiceTests.Charging
{
    public class ExecutingCharging
    {
        [Fact]
        public async Task Charge_valid_bookings_should_succeed()
        {
            var service = CreateProcessingService();

            var (isSuccess, _, _, error) = await service.Charge(new List<int> { 2 }, ServiceAccount);

            Assert.True(isSuccess);
        }
        
        
        [Fact]
        public async Task Charge_invalid_booking_should_fail()
        {
            var service = CreateProcessingService();

            var (_, isFailure, _, error) = await service.Charge(new List<int> { 1, 4, 5 }, ServiceAccount);

            Assert.True(isFailure);
        }


        [Fact]
        public async Task All_bookings_should_be_charged()
        {
            var bookingPaymentServiceMock = new Mock<IBookingAccountPaymentService>();
            var service = CreateProcessingService(bookingPaymentServiceMock.Object);

            var result = await service.Charge(new List<int> { 2, 3 }, ServiceAccount);

            bookingPaymentServiceMock
                .Verify(
                    b => b.Charge(It.IsAny<Booking>(), It.IsAny<ApiCaller>()),
                    Times.Exactly(2)
                );
        }


        [Fact]
        public async Task When_payment_fails_bookings_should_be_set_manual_correction_needed()
        {
            var bookingAccountPaymentServiceMock = new Mock<IBookingAccountPaymentService>();
            var bookingServiceMock = new Mock<ISupplierBookingManagementService>();
            var bookingRecordsUpdater = new Mock<IBookingRecordsUpdater>();

            bookingAccountPaymentServiceMock.Setup(s => s.Charge(It.IsAny<Booking>(), It.IsAny<ApiCaller>()))
                .Returns(Task.FromResult(Result.Failure<string>("Err")));

            var service = CreateProcessingService(bookingAccountPaymentServiceMock.Object, bookingServiceMock.Object, bookingRecordsUpdater.Object);

            await service.Charge(new List<int> { 2, 3 }, ServiceAccount);

            bookingServiceMock
                .Verify(
                    b => b.Cancel(It.IsAny<Booking>(), It.IsAny<ApiCaller>(), It.IsAny<BookingChangeEvents>()),
                    Times.Exactly(0)
                );
            bookingRecordsUpdater.Verify(s => s.ChangeStatus(It.IsAny<Booking>(),
                BookingStatuses.ManualCorrectionNeeded, It.IsAny<DateTime>(), It.IsAny<ApiCaller>(), It.IsAny<BookingChangeReason>()));
        }


        private BookingsProcessingService CreateProcessingService(IBookingAccountPaymentService? bookingPaymentService = null,
            ISupplierBookingManagementService? bookingManagementService = null, IBookingRecordsUpdater? bookingRecordsUpdater = null)
        {
            bookingPaymentService ??= Mock.Of<IBookingAccountPaymentService>();
            bookingManagementService ??= Mock.Of<ISupplierBookingManagementService>();
            bookingRecordsUpdater ??= Mock.Of<IBookingRecordsUpdater?>();

            var context = MockEdoContextFactory.Create();
            context.Setup(c => c.Bookings)
                .Returns(DbSetMockProvider.GetDbSetMock(Bookings));

            var service = new BookingsProcessingService(bookingPaymentService,
                Mock.Of<IBookingCreditCardPaymentService>(),
                bookingManagementService,
                Mock.Of<IBookingNotificationService>(),
                Mock.Of<IBookingReportsService>(),
                context.Object,
                bookingRecordsUpdater,
                Mock.Of<IDateTimeProvider>());
            return service;
        }


        private static readonly ServiceAccount ServiceAccount = new ServiceAccount { ClientId = "ClientId", Id = 11};

        private static readonly Booking[] Bookings =
        {
            new Booking
            {
                Id = 1, 
                PaymentStatus = BookingPaymentStatuses.NotPaid, 
                ReferenceCode = "NNN-222",
                Status = BookingStatuses.Pending,
                PaymentType = PaymentTypes.VirtualAccount,
                CheckInDate = new DateTime(2021, 12, 10)
            },
            new Booking
            {
                Id = 2, 
                PaymentStatus = BookingPaymentStatuses.NotPaid,
                ReferenceCode = "NNN-223", 
                Status = BookingStatuses.Confirmed, 
                PaymentType = PaymentTypes.VirtualAccount,
                CheckInDate = new DateTime(2021, 12, 11),
                DeadlineDate = new DateTime(2021, 12, 9)
            },
            new Booking
            {
                Id = 3,
                PaymentStatus = BookingPaymentStatuses.NotPaid,
                ReferenceCode = "NNN-224",
                Status = BookingStatuses.Confirmed,
                PaymentType = PaymentTypes.VirtualAccount,
                CheckInDate = new DateTime(2021, 12, 10),
                DeadlineDate = new DateTime(2021, 11, 9)
            },
            new Booking
            {
                Id = 4,
                PaymentStatus = BookingPaymentStatuses.NotPaid,
                ReferenceCode = "NNN-224",
                Status = BookingStatuses.Cancelled,
                PaymentType = PaymentTypes.VirtualAccount,
                CheckInDate = new DateTime(2021, 12, 20)
            },
            new Booking
            {
                Id = 5,
                PaymentStatus = BookingPaymentStatuses.NotPaid,
                ReferenceCode = "NNN-224",
                Status = BookingStatuses.Rejected,
                PaymentType = PaymentTypes.VirtualAccount,
                CheckInDate = new DateTime(2022, 12, 20)
            }
        };
    }
}