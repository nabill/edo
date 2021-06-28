using System;
using System.Collections.Generic;
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
            var doubleFlowMock = new Mock<IDoubleFlow>();
            
            // Using function instead of a member because the mock changes initial data
            var initialStates = GetInitialStates();
            var capturedStates = new List<BookingStatusRefreshState>();
            
            doubleFlowMock.Setup(x => x.GetAsync<List<BookingStatusRefreshState>>(It.IsAny<string>(), It.IsAny<TimeSpan>(), default))
                .ReturnsAsync(initialStates);
            
            doubleFlowMock.Setup(x => x.SetAsync(It.IsAny<string>(), It.IsAny<List<BookingStatusRefreshState>>(), It.IsAny<TimeSpan>(), default))
                .Callback<string, List<BookingStatusRefreshState>, TimeSpan, CancellationToken>((_, states, _, _) => { capturedStates = states; });

            var supplier = CreateWorkingSupplier();

            var bookingStatusRefreshService = CreateBookingStatusRefreshService(doubleFlowMock.Object, supplier);
            await bookingStatusRefreshService.RefreshStatuses(BookingIds, ApiCaller.InternalServiceAccount);

            Assert.Equal(2, capturedStates[0].RefreshStatusCount);
            Assert.Equal(DateTimeNow, capturedStates[0].LastRefreshDate);
        }
        
        
        [Fact]
        public async Task Should_update_last_refresh_date_for_failing_supplier()
        {
            var doubleFlowMock = new Mock<IDoubleFlow>();
            
            // Using function instead of a member because the mock changes initial data
            var initialStates = GetInitialStates();
            var capturedStates = new List<BookingStatusRefreshState>();
            
            doubleFlowMock.Setup(x => x.GetAsync<List<BookingStatusRefreshState>>(It.IsAny<string>(), It.IsAny<TimeSpan>(), default))
                .ReturnsAsync(initialStates);
            
            doubleFlowMock.Setup(x => x.SetAsync(It.IsAny<string>(), It.IsAny<List<BookingStatusRefreshState>>(), It.IsAny<TimeSpan>(), default))
                .Callback<string, List<BookingStatusRefreshState>, TimeSpan, CancellationToken>((_, states, _, _) => { capturedStates = states; });

            var supplier = CreateFailingSupplier();

            var bookingStatusRefreshService = CreateBookingStatusRefreshService(doubleFlowMock.Object, supplier);
            await bookingStatusRefreshService.RefreshStatuses(BookingIds, ApiCaller.InternalServiceAccount);

            Assert.Equal(2, capturedStates[0].RefreshStatusCount);
            Assert.Equal(DateTimeNow, capturedStates[0].LastRefreshDate);
        }

        private static BookingStatusRefreshService CreateBookingStatusRefreshService(IDoubleFlow doubleFlow, ISupplierBookingManagementService supplierService)
        {
            var context = CreateContext();
            var dateTimeProvider = new DateTimeProviderMock(DateTimeNow);
            var bookingOptions = Mock.Of<IOptions<BookingOptions>>();
            
            return new BookingStatusRefreshService(
                doubleFlow,
                dateTimeProvider,
                supplierService,
                context,
                bookingOptions
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
                Status = BookingStatuses.Pending
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