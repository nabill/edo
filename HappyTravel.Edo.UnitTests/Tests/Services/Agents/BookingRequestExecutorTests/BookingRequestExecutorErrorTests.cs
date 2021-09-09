﻿using System.Threading.Tasks;
using HappyTravel.Edo.Api.Infrastructure;
using HappyTravel.Edo.Api.Services.Accommodations.Bookings.BookingExecution;
using HappyTravel.Edo.Api.Services.Accommodations.Bookings.Management;
using HappyTravel.Edo.Api.Services.Accommodations.Bookings.ResponseProcessing;
using HappyTravel.Edo.Api.Services.Analytics;
using HappyTravel.Edo.Api.Services.Connectors;
using HappyTravel.Edo.Data.Bookings;
using HappyTravel.EdoContracts.Accommodations.Enums;
using HappyTravel.SuppliersCatalog;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace HappyTravel.Edo.UnitTests.Tests.Services.Agents.BookingRequestExecutorTests
{
    public class BookingRequestExecutorErrorTests
    {
        [Theory]
        [InlineData(BookingFailureCodes.ConnectorValidationFailed, true, false)]
        [InlineData(BookingFailureCodes.ValuationResultNotFound, true, false)]
        [InlineData(BookingFailureCodes.PreBookingFailed, true, false)]
        [InlineData(BookingFailureCodes.SupplierValidationFailed, true, false)]
        [InlineData(BookingFailureCodes.SupplierRejected, true, false)]
        [InlineData(BookingFailureCodes.RequestException, true, true)]
        [InlineData(BookingFailureCodes.UnknownRequestError, true, true)]
        [InlineData(BookingFailureCodes.Unknown, false, true)]
        public async Task Booking_failure_code_should_lead_to_failure_or_success_depending_on_error_code(
            BookingFailureCodes failureCode,
            bool isErrorDefined,
            bool isSuccess)
        {
            InitializeMocks();
            var service = CreateBookingRequestExecutor();
            var booking = new Booking();
            var request = Utility.CreateAccommodationBookingRequest();
            var problemDetails = Utility.CreateProblemDetailsWithFailureCode(isErrorDefined, failureCode);
            Utility.SetupConnectorBookFailure(_supplierConnectorMock, problemDetails);

            var result = await service.Execute(request, default, booking, default, default);

            Assert.Equal(isSuccess, result.IsSuccess);
        }


        [Fact]
        public async Task Exception_should_lead_to_success()
        {
            InitializeMocks();
            var service = CreateBookingRequestExecutor();
            var booking = new Booking();
            var request = Utility.CreateAccommodationBookingRequest();
            Utility.SetupConnectorBookThrowsException(_supplierConnectorMock);

            var result = await service.Execute(request, default, booking, default, default);

            Assert.True(result.IsSuccess);
        }


        private void InitializeMocks()
        {
            _supplierConnectorManagerMock = new Mock<ISupplierConnectorManager>();
            _responseProcessorMock = new Mock<IBookingResponseProcessor>();
            _bookingAnalyticsServiceMock = new Mock<IBookingAnalyticsService>();
            _bookingRecordsUpdaterMock = new Mock<IBookingRecordsUpdater>();
            _dateTimeProviderMock = new Mock<IDateTimeProvider>();
            _loggerMock = new Mock<ILogger<BookingRequestExecutor>>();
            _supplierConnectorMock = new Mock<ISupplierConnector>();

            _supplierConnectorManagerMock
                .Setup(x => x.Get(It.IsAny<Suppliers>()))
                .Returns(_supplierConnectorMock.Object);
        }


        private BookingRequestExecutor CreateBookingRequestExecutor()
        {
            return new BookingRequestExecutor(
                _supplierConnectorManagerMock.Object,
                _responseProcessorMock.Object,
                _bookingAnalyticsServiceMock.Object,
                _bookingRecordsUpdaterMock.Object,
                _dateTimeProviderMock.Object,
                _loggerMock.Object);
        }


#pragma warning disable CS8618
        private Mock<ISupplierConnectorManager> _supplierConnectorManagerMock;
        private Mock<IBookingResponseProcessor> _responseProcessorMock;
        private Mock<IBookingAnalyticsService> _bookingAnalyticsServiceMock;
        private Mock<IBookingRecordsUpdater> _bookingRecordsUpdaterMock;
        private Mock<IDateTimeProvider> _dateTimeProviderMock;
        private Mock<ILogger<BookingRequestExecutor>> _loggerMock;
        private Mock<ISupplierConnector> _supplierConnectorMock;
#pragma warning restore CS8618
    }
}
