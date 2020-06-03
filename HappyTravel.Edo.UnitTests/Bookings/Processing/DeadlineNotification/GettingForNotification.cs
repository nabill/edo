using System.Collections.Generic;
using System.Threading.Tasks;
using HappyTravel.Edo.Api.Services.Accommodations.Bookings;
using HappyTravel.Edo.Api.Services.Payments;
using HappyTravel.Edo.Data.Booking;
using HappyTravel.Edo.UnitTests.Infrastructure;
using HappyTravel.Edo.UnitTests.Infrastructure.DbSetMocks;
using Moq;
using Xunit;

namespace HappyTravel.Edo.UnitTests.Bookings.Processing.DeadlineNotification
{
    public class GettingForNotification
    {
        //[Fact]
        // public async Task ddd()
        // {
        //     var service = CreateProcessingService()
        // }
        //
        // private BookingsProcessingService CreateProcessingService(IEnumerable<Booking> bookings)
        // {
        //     var context = MockEdoContext.Create();
        //     context.Setup(c => c.Bookings)
        //         .Returns(DbSetMockProvider.GetDbSetMock(bookings));
        //
        //     var service = new BookingsProcessingService(Mock.Of<IBookingPaymentService>(),
        //         Mock.Of<IPaymentNotificationService>(),
        //         Mock.Of<IBookingService>(),
        //         context.Object);
        //     
        //     return service;
        // }
    }
}