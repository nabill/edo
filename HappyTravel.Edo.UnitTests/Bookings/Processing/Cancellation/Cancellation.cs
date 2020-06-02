using System.Collections.Generic;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using HappyTravel.Edo.Api.Infrastructure;
using HappyTravel.Edo.Api.Services.Accommodations.Bookings;
using HappyTravel.Edo.Api.Services.Payments;
using HappyTravel.Edo.Common.Enums;
using HappyTravel.Edo.Data.Booking;
using HappyTravel.Edo.Data.Management;
using HappyTravel.Edo.UnitTests.Infrastructure;
using HappyTravel.Edo.UnitTests.Infrastructure.DbSetMocks;
using HappyTravel.EdoContracts.Accommodations.Enums;
using HappyTravel.EdoContracts.General.Enums;
using Moq;
using Xunit;

namespace HappyTravel.Edo.UnitTests.Bookings.Processing.Cancellation
{
    public class Cancellation
    {
        [Fact]
        public async Task Cancel_valid_bookings_should_succeed()
        {
            var service = CreateProcessingService(Mock.Of<IBookingService>());

            var (isSuccess, _, _, _) = await service.Cancel(new List<int> {1, 2}, new ServiceAccount() {ClientId = "ClientId", Id = 12});

            Assert.True(isSuccess);
        }
        
        
        [Fact]
        public async Task Cancel_invalid_booking_should_fail()
        {
            var service = CreateProcessingService(Mock.Of<IBookingService>());

            var (_, isFailure, _, _) = await service.Cancel(new List<int> {3, 4}, new ServiceAccount() {ClientId = "ClientId", Id = 12});

            Assert.True(isFailure);
        }
        
        
        [Fact]
        public async Task All_bookings_should_be_cancelled()
        {
            var bookingServiceMock = new Mock<IBookingService>();
            var service = CreateProcessingService(bookingServiceMock.Object);

            await service.Cancel(new List<int> {1, 2}, new ServiceAccount() {ClientId = "ClientId", Id = 12});

            bookingServiceMock
                .Verify(
                    b => b.Cancel(It.IsAny<int>(), It.IsAny<ServiceAccount>()),
                    Times.Exactly(2)
                );
        }


        private BookingsProcessingService CreateProcessingService(IBookingService bookingService)
        {
            var context = MockEdoContext.Create();
            context.Setup(c => c.Bookings)
                .Returns(DbSetMockProvider.GetDbSetMock(Bookings));

            var service = new BookingsProcessingService(Mock.Of<IBookingPaymentService>(), Mock.Of<IPaymentNotificationService>(), bookingService, new DefaultDateTimeProvider(), context.Object);
            return service;
        }


        private static readonly Booking[] Bookings =
        {
            new Booking
            {
                Id = 1, PaymentStatus = BookingPaymentStatuses.NotPaid, ReferenceCode = "NNN-222", Status = BookingStatusCodes.Pending, PaymentMethod = PaymentMethods.BankTransfer
            },
            new Booking
            {
                Id = 2, PaymentStatus = BookingPaymentStatuses.Refunded, ReferenceCode = "NNN-223", Status = BookingStatusCodes.Confirmed, PaymentMethod = PaymentMethods.CreditCard
            },
            new Booking
            {
                Id = 3, PaymentStatus = BookingPaymentStatuses.Authorized, ReferenceCode = "NNN-224", Status = BookingStatusCodes.Confirmed, PaymentMethod = PaymentMethods.CreditCard
            }
        };
    }
}