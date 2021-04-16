using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using HappyTravel.Edo.Api.Infrastructure;
using HappyTravel.Edo.Api.Services.Accommodations.Bookings.Mailing;
using HappyTravel.Edo.Api.Services.Accommodations.Bookings.Management;
using HappyTravel.Edo.Api.Services.Accommodations.Bookings.Payments;
using HappyTravel.Edo.Api.Services.SupplierOrders;
using HappyTravel.Edo.Common.Enums;
using HappyTravel.Edo.Data;
using HappyTravel.Edo.Data.Bookings;
using HappyTravel.Edo.UnitTests.Utility;
using HappyTravel.EdoContracts.Accommodations.Enums;
using HappyTravel.EdoContracts.Accommodations.Internals;
using HappyTravel.EdoContracts.General;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace HappyTravel.Edo.UnitTests.Tests.Services.Accommodations.Bookings.BookingRecordsUpdaterTests
{
    public class UpdateWithSupplierDataTests
    {
        [Fact]
        public async Task Updating_rooms_with_same_number_of_rooms_should_change_rooms_supplier_code()
        {
            var context = CreateContext();
            var service = CreateBookingRecordsUpdaterService(context);

            await service.UpdateWithSupplierData(Bookings.First(), default, default, UpdatedRoomsSameNumber);

            var rooms = context.Bookings.First().Rooms;
            Assert.Equal(rooms[0].SupplierRoomReferenceCode, UpdatedRoomsSameNumber[0].SupplierRoomReferenceCode);
            Assert.Equal(rooms[1].SupplierRoomReferenceCode, UpdatedRoomsSameNumber[1].SupplierRoomReferenceCode);
            Assert.Equal(rooms[2].SupplierRoomReferenceCode, UpdatedRoomsSameNumber[2].SupplierRoomReferenceCode);
        }


        [Fact]
        public async Task Updating_rooms_with_different_number_shouldnt_change_rooms_supplier_code()
        {
            var context = CreateContext();
            var service = CreateBookingRecordsUpdaterService(context);

            await service.UpdateWithSupplierData(Bookings.First(), default, default, UpdatedRoomsDifferentNumber);

            var rooms = context.Bookings.First().Rooms;
            Assert.Equal(BookedRooms[0].SupplierRoomReferenceCode, rooms[0].SupplierRoomReferenceCode);
            Assert.Equal(BookedRooms[1].SupplierRoomReferenceCode, rooms[1].SupplierRoomReferenceCode);
            Assert.Equal(BookedRooms[2].SupplierRoomReferenceCode, rooms[2].SupplierRoomReferenceCode);
        }


        private EdoContext CreateContext()
        {
            var context = MockEdoContextFactory.Create();

            context.SetupProperty(x => x.Bookings);
            
            context
                .Setup(x => x.Bookings)
                .Returns(DbSetMockProvider.GetDbSetMock(Bookings));

            return context.Object;
        }
        
        
        private BookingRecordsUpdater CreateBookingRecordsUpdaterService(EdoContext context)
        {
            return new BookingRecordsUpdater(
                Mock.Of<IDateTimeProvider>(), 
                Mock.Of<IBookingInfoService>(), 
                Mock.Of<IBookingNotificationService>(),
                Mock.Of<IBookingMoneyReturnService>(),
                Mock.Of<IBookingDocumentsMailingService>(),
                Mock.Of<ISupplierOrderService>(),
                Mock.Of<IBookingChangeLogService>(),
                context, 
                Mock.Of<ILogger<BookingRecordsUpdater>>());
        }
        
        private static readonly List<BookedRoom> BookedRooms = new() 
        {
            MakeBookedRoom("a"),
            MakeBookedRoom("b"),
            MakeBookedRoom("c")
        };

        private static readonly List<SlimRoomOccupation> UpdatedRoomsSameNumber = new()
        {
            MakeSlimRoomOccupation("e"),
            MakeSlimRoomOccupation("d"),
            MakeSlimRoomOccupation("f")
        };
        
        private static readonly List<SlimRoomOccupation> UpdatedRoomsDifferentNumber = new()
        {
            MakeSlimRoomOccupation("e"),
            MakeSlimRoomOccupation("d"),
        };
        
        private static readonly Booking Booking = new()
        {
            Id = 1, 
            PaymentStatus = BookingPaymentStatuses.Authorized, 
            Status = BookingStatuses.Pending, 
            PaymentMethod = PaymentTypes.CreditCard, 
            AgentId = 1, 
            AgencyId = 1,
            Rooms = BookedRooms
        };
        
        private static BookedRoom MakeBookedRoom(string supplierRoomReferenceCode) =>
            new(default, default, default, default, default, default, default, default, default, default, default, supplierRoomReferenceCode);

        private static SlimRoomOccupation MakeSlimRoomOccupation(string supplierRoomReferenceCode)
            => new(RoomTypes.Single, new List<Pax>(), supplierRoomReferenceCode);
        
        private static readonly Booking[] Bookings =
        {
            Booking
        };
    }
}