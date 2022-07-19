using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using FloxDc.CacheFlow;
using HappyTravel.Edo.Api.Infrastructure.Options;
using HappyTravel.Edo.Api.Models.Bookings;
using HappyTravel.Edo.Api.Models.Users;
using HappyTravel.Edo.Api.Services.Accommodations.Bookings.Management;
using HappyTravel.Edo.Common.Enums;
using HappyTravel.Edo.Data;
using HappyTravel.Edo.Data.Bookings;
using HappyTravel.Edo.UnitTests.Mocks;
using HappyTravel.Edo.UnitTests.Utility;
using Microsoft.Extensions.Options;
using Moq;
using Xunit;

namespace HappyTravel.Edo.UnitTests.Tests.Services.Accommodations.Bookings.Management
{
    public class BookingStatusRefreshServiceTests
    {
        [Fact]
        public async Task Should_update_last_refresh_date_for_working_supplier()
        {
            var flowMock = new Mock<IDistributedFlow>();
            
            // Using function instead of a member because the mock changes initial data
            var initialStates = GetInitialStates();
            var capturedStates = new List<BookingStatusRefreshState>();
            
            flowMock.Setup(x => x.GetAsync<List<BookingStatusRefreshState>>(It.IsAny<string>(), default))
                .ReturnsAsync(initialStates);
            
            flowMock.Setup(x => x.SetAsync(It.IsAny<string>(), It.IsAny<List<BookingStatusRefreshState>>(), It.IsAny<TimeSpan>(), default))
                .Callback<string, List<BookingStatusRefreshState>, TimeSpan, CancellationToken>((_, states, _, _) => { capturedStates = states; });

            var supplier = CreateWorkingSupplier();

            var bookingStatusRefreshService = CreateBookingStatusRefreshService(flowMock.Object, supplier);
            await bookingStatusRefreshService.RefreshStatuses(BookingIds, ApiCaller.InternalServiceAccount);

            Assert.Equal(2, capturedStates[0].RefreshStatusCount);
            Assert.Equal(DateTimeNow, capturedStates[0].LastRefreshDate);
        }
        
        
        [Fact]
        public async Task Should_update_last_refresh_date_for_failing_supplier()
        {
            var flowMock = new Mock<IDistributedFlow>();
            
            // Using function instead of a member because the mock changes initial data
            var initialStates = GetInitialStates();
            var capturedStates = new List<BookingStatusRefreshState>();
            
            flowMock.Setup(x => x.GetAsync<List<BookingStatusRefreshState>>(It.IsAny<string>(), default))
                .ReturnsAsync(initialStates);
            
            flowMock.Setup(x => x.SetAsync(It.IsAny<string>(), It.IsAny<List<BookingStatusRefreshState>>(), It.IsAny<TimeSpan>(), default))
                .Callback<string, List<BookingStatusRefreshState>, TimeSpan, CancellationToken>((_, states, _, _) => { capturedStates = states; });

            var supplier = CreateFailingSupplier();

            var bookingStatusRefreshService = CreateBookingStatusRefreshService(flowMock.Object, supplier);
            await bookingStatusRefreshService.RefreshStatuses(BookingIds, ApiCaller.InternalServiceAccount);

            Assert.Equal(2, capturedStates[0].RefreshStatusCount);
            Assert.Equal(DateTimeNow, capturedStates[0].LastRefreshDate);
        }


        [Theory]
        [InlineData(2, "Supplier2")]
        [InlineData(2, "Supplier1")]
        [InlineData(1, "Supplier1", "Supplier2")]
        public async Task Should_filter_bookings_from_disabled_suppliers(int expectedCount, params string[] disabledSuppliers)
        {
            var flowMock = new Mock<IDistributedFlow>();
            flowMock.Setup(x => x.GetAsync<List<BookingStatusRefreshState>>(It.IsAny<string>(), default))
                .ReturnsAsync(new List<BookingStatusRefreshState>(0));

            var bookingStatusRefreshService = CreateBookingStatusRefreshService(flowMock.Object, Mock.Of<ISupplierBookingManagementService>(), disabledSuppliers.ToList());
            var result = await bookingStatusRefreshService.GetBookingsToRefresh();

            Assert.Equal(expectedCount, result.Count);
        }


        [Fact]
        public async Task Should_filter_bookings_with_checkin_in_past()
        {
            var flowMock = new Mock<IDistributedFlow>();
            flowMock.Setup(x => x.GetAsync<List<BookingStatusRefreshState>>(It.IsAny<string>(), default))
                .ReturnsAsync(new List<BookingStatusRefreshState>(0));

            var bookingStatusRefreshService = CreateBookingStatusRefreshService(flowMock.Object, Mock.Of<ISupplierBookingManagementService>());
            var result = await bookingStatusRefreshService.GetBookingsToRefresh();

            Assert.Equal(3, result.Count);
        }

        
        private static BookingStatusRefreshService CreateBookingStatusRefreshService(IDistributedFlow flow, ISupplierBookingManagementService supplierService, List<string>? disabledSuppliers = null)
        {
            var context = CreateContext();
            var dateTimeProvider = new DateTimeProviderMock(DateTimeNow);
            var monitor = Mock.Of<IOptionsMonitor<BookingStatusUpdateOptions>>(_ => _.CurrentValue == new BookingStatusUpdateOptions
            {
                DisabledSuppliers = disabledSuppliers ?? new List<string>()
            });
            
            return new BookingStatusRefreshService(
                flow,
                dateTimeProvider,
                supplierService,
                context,
                monitor,
                Mock.Of<IBookingChangeLogService>()
            );
        }


        private static EdoContext CreateContext()
        {
            var context = MockEdoContextFactory.Create();
            context
                .Setup(x => x.Bookings)
                .Returns(DbSetMockProvider.GetDbSetMock(Bookings));
            return context.Object;
        }


        private static ISupplierBookingManagementService CreateWorkingSupplier()
        {
            var supplierMock = new Mock<ISupplierBookingManagementService>();
            supplierMock
                .Setup(x => x.RefreshStatus(It.IsAny<Booking>(), It.IsAny<ApiCaller>(), It.IsAny<BookingChangeEvents>()))
                .ReturnsAsync(Result.Success);
            return supplierMock.Object;
        }
        
        
        private static ISupplierBookingManagementService CreateFailingSupplier()
        {
            var supplierMock = new Mock<ISupplierBookingManagementService>();
            supplierMock
                .Setup(x => x.RefreshStatus(It.IsAny<Booking>(), It.IsAny<ApiCaller>(), It.IsAny<BookingChangeEvents>()))
                .ReturnsAsync(Result.Failure("error"));
            return supplierMock.Object;
        }


        private static readonly DateTime DateTimeNow = new(2021, 6, 1, 0, 15, 0);
        
        private static readonly List<Booking> Bookings = new()
        {
            new Booking
            {
                Id = 1,
                Status = BookingStatuses.Pending,
                SupplierCode = "Supplier2",
                CheckInDate = DateTimeNow.AddDays(10)
            },
            new Booking
            {
                Id = 2,
                Status = BookingStatuses.Pending,
                SupplierCode = "Supplier1",
                CheckInDate = DateTimeNow.AddDays(10)
            },
            new Booking
            {
                Id = 3,
                Status = BookingStatuses.Pending,
                SupplierCode = "Supplier3",
                CheckInDate = DateTimeNow.AddDays(10)
            },
            new Booking
            {
                Id = 4,
                Status = BookingStatuses.Pending,
                SupplierCode = "Supplier4",
                CheckInDate = DateTimeNow.AddDays(-10)
            }
        };

        private static readonly List<int> BookingIds = new()
        {
            1
        };


        private static List<BookingStatusRefreshState> GetInitialStates()
        {
            return new() 
            {
                new()
                {
                    BookingId = 1,
                    LastRefreshDate = new DateTime(2021, 6, 1, 0, 0, 0),
                    RefreshStatusCount = 1
                }
            };
        }
    }
}