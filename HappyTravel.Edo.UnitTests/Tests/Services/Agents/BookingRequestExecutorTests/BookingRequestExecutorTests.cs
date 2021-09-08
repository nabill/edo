using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using HappyTravel.Edo.Api.Infrastructure;
using HappyTravel.Edo.Api.Models.Bookings;
using HappyTravel.Edo.Api.Models.Users;
using HappyTravel.Edo.Api.Services.Accommodations.Bookings.BookingExecution;
using HappyTravel.Edo.Api.Services.Accommodations.Bookings.Management;
using HappyTravel.Edo.Api.Services.Accommodations.Bookings.ResponseProcessing;
using HappyTravel.Edo.Api.Services.Analytics;
using HappyTravel.Edo.Api.Services.Connectors;
using HappyTravel.Edo.Common.Enums;
using HappyTravel.Edo.Data.Bookings;
using HappyTravel.EdoContracts.Accommodations.Enums;
using HappyTravel.EdoContracts.Accommodations.Internals;
using HappyTravel.SuppliersCatalog;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using BookingRequest = HappyTravel.EdoContracts.Accommodations.BookingRequest;

namespace HappyTravel.Edo.UnitTests.Tests.Services.Agents.BookingRequestExecutorTests
{
    public class BookingRequestExecutorTests
    {
        [Theory]
        [InlineData(Suppliers.Illusions)]
        [InlineData(Suppliers.Etg)]
        [InlineData(Suppliers.Columbus)]
        public async Task Correct_supplier_should_be_passed_when_getting_supplier_connector(Suppliers supplier)
        {
            InitializeMocks();

            var service = CreateBookingRequestExecutor();
            var booking = new Booking { Supplier = supplier };
            var request = CreateAccommodationBookingRequest();

            await service.Execute(request, default, booking, default, default);

            _supplierConnectorManagerMock.Verify(x => x.Get(supplier));
        }


        [Theory]
        [InlineData("1", "2", "aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa", true)]
        [InlineData("3", "4", "aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaab", false)]
        [InlineData("5", "6", "aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaac", true)]
        public async Task Correct_request_should_be_passed_when_booking_on_connector(
            string availabilityId,
            string referenceCode,
            string guidString,
            bool rejectIfUnavailable)
        {
            InitializeMocks();

            var roomContractSetId = new Guid(guidString);
            var service = CreateBookingRequestExecutor();
            var booking = new Booking { ReferenceCode = referenceCode };
            var request = CreateAccommodationBookingRequest(roomContractSetId, rejectIfUnavailable);
            BookingRequest actualInnerRequest = default;

            _supplierConnectorMock
                .Setup(x => x.Book(It.IsAny<BookingRequest>(), It.IsAny<string>()))
                .Callback<BookingRequest, string>((req, _) => actualInnerRequest = req)
                .ReturnsAsync(Result.Success<EdoContracts.Accommodations.Booking, ProblemDetails>(default));

            await service.Execute(request, availabilityId, booking, default, default);

            _supplierConnectorMock.Verify(x => x.Book(It.IsAny<BookingRequest>(), It.IsAny<string>()));
            Assert.Equal(availabilityId, actualInnerRequest.AvailabilityId);
            Assert.Equal(roomContractSetId, actualInnerRequest.RoomContractSetId);
            Assert.Equal(rejectIfUnavailable, actualInnerRequest.RejectIfUnavailable);
            Assert.Equal(referenceCode, actualInnerRequest.ReferenceCode);
        }


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
            var request = CreateAccommodationBookingRequest();

            var problemDetails = new ProblemDetails { Detail = "ErrorDetail" };
            if(isErrorDefined)
                problemDetails.Extensions["booking-failure-code"] = failureCode;

            _supplierConnectorMock
                .Setup(x => x.Book(It.IsAny<BookingRequest>(), It.IsAny<string>()))
                .ReturnsAsync(Result.Failure<EdoContracts.Accommodations.Booking, ProblemDetails>(problemDetails));

            var result = await service.Execute(request, default, booking, default, default);

            Assert.Equal(isSuccess, result.IsSuccess);
        }


        [Fact]
        public async Task Exception_should_lead_to_success()
        {
            InitializeMocks();

            var service = CreateBookingRequestExecutor();
            var booking = new Booking();
            var request = CreateAccommodationBookingRequest();

            _supplierConnectorMock
                .Setup(x => x.Book(It.IsAny<BookingRequest>(), It.IsAny<string>()))
                .ThrowsAsync(new Exception());

            var result = await service.Execute(request, default, booking, default, default);

            Assert.True(result.IsSuccess);
        }


        [Fact]
        public async Task When_success_booking_should_be_passed_to_process_response()
        {
            InitializeMocks();

            var service = CreateBookingRequestExecutor();
            var referenceCode = "RefCode12";
            var booking = new Booking { ReferenceCode = referenceCode };
            var request = CreateAccommodationBookingRequest();

            EdoContracts.Accommodations.Booking bookingPassedToProcessResponse = default;

            _responseProcessorMock
                .Setup(x => x.ProcessResponse(It.IsAny<EdoContracts.Accommodations.Booking>(), It.IsAny<ApiCaller>(), It.IsAny<BookingChangeEvents>()))
                .Callback<EdoContracts.Accommodations.Booking, ApiCaller, BookingChangeEvents>((b, _, _) => bookingPassedToProcessResponse = b);

            _supplierConnectorMock
                .Setup(x => x.Book(It.IsAny<BookingRequest>(), It.IsAny<string>()))
                .ReturnsAsync((BookingRequest br, string _)
                    => Result.Success<EdoContracts.Accommodations.Booking, ProblemDetails>(CreateBooking(br.ReferenceCode)));

            await service.Execute(request, default, booking, default, default);

            _responseProcessorMock
                .Verify(x => x.ProcessResponse(It.IsAny<EdoContracts.Accommodations.Booking>(), It.IsAny<ApiCaller>(), It.IsAny<BookingChangeEvents>()));
            Assert.Equal(referenceCode, bookingPassedToProcessResponse.ReferenceCode);
        }


        [Fact]
        public async Task When_failure_change_status_to_invalid_should_be_called()
        {
            InitializeMocks();

            var service = CreateBookingRequestExecutor();
            var booking = new Booking();
            var request = CreateAccommodationBookingRequest();

            var problemDetails = new ProblemDetails { Detail = "ErrorDetail" };
            problemDetails.Extensions["booking-failure-code"] = BookingFailureCodes.ConnectorValidationFailed;

            _supplierConnectorMock
                .Setup(x => x.Book(It.IsAny<BookingRequest>(), It.IsAny<string>()))
                .ReturnsAsync(Result.Failure<EdoContracts.Accommodations.Booking, ProblemDetails>(problemDetails));

            await service.Execute(request, default, booking, default, default);

            _bookingRecordsUpdaterMock
                .Verify(x => x.ChangeStatus(It.IsAny<Booking>(), BookingStatuses.Invalid, It.IsAny<DateTime>(),
                    It.IsAny<ApiCaller>(), It.IsAny<BookingChangeReason>()));
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


        private AccommodationBookingRequest CreateAccommodationBookingRequest()
            => new (default, default, default, default, default, default, default, default, string.Empty, default);


        private AccommodationBookingRequest CreateAccommodationBookingRequest(Guid roomContractSetId, bool rejectIfUnavailable)
            => new (default, default, default, default, default, default, default, roomContractSetId, string.Empty, rejectIfUnavailable);


        private EdoContracts.Accommodations.Booking CreateBooking(string referenceCode)
            => new (referenceCode, default, string.Empty, string.Empty, default, default, new List<SlimRoomOccupation>(0), default);


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
