using System;
using System.Collections.Generic;
using CSharpFunctionalExtensions;
using HappyTravel.Edo.Api.Models.Accommodations;
using HappyTravel.Edo.Api.Models.Bookings;
using HappyTravel.Edo.Api.Services.Connectors;
using HappyTravel.EdoContracts.Accommodations.Enums;
using HappyTravel.EdoContracts.Accommodations.Internals;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Moq.Language.Flow;
using BookingRequest = HappyTravel.EdoContracts.Accommodations.BookingRequest;

namespace HappyTravel.Edo.UnitTests.Tests.Services.Agents.BookingRequestExecutorTests
{
    public static class Utility
    {
        public static AccommodationBookingRequest CreateAccommodationBookingRequest()
            => new(default, default, default, default, default, default, default, string.Empty, default);


        public static AccommodationBookingRequest CreateAccommodationBookingRequest(Guid roomContractSetId, bool rejectIfUnavailable)
            => new(default, default, default, default, default, default, roomContractSetId, string.Empty, default, rejectIfUnavailable);

        public static BookingAvailabilityInfo CreateAvailabilityInfo(string availabilityId)
            => new(default, default, default, default, default, default, default, default, default, default, default, default,
                default, default, default, default, default, default, availabilityId, default, default, default,
                default, default, default, default);

        public static EdoContracts.Accommodations.Booking CreateBooking(string referenceCode)
            => new(referenceCode, default, string.Empty, string.Empty, default, default, new List<SlimRoomOccupation>(0), default);


        public static ProblemDetails CreateProblemDetailsWithFailureCode(bool isErrorDefined, BookingFailureCodes failureCode = default)
        {
            var problemDetails = new ProblemDetails { Detail = "ErrorDetail" };
            if (isErrorDefined)
                problemDetails.Extensions["booking-failure-code"] = failureCode;

            return problemDetails;
        }


        public static IReturnsResult<ISupplierConnector> SetupConnectorBookSuccess(Mock<ISupplierConnector> supplierConnectorMock)
            => supplierConnectorMock
                .Setup(x => x.Book(It.IsAny<BookingRequest>(), It.IsAny<string>()))
                .ReturnsAsync((BookingRequest br, string _)
                    => Result.Success<EdoContracts.Accommodations.Booking, ProblemDetails>(CreateBooking(br.ReferenceCode)));


        public static IReturnsResult<ISupplierConnector> SetupConnectorBookFailure(Mock<ISupplierConnector> supplierConnectorMock, ProblemDetails problemDetails)
            => supplierConnectorMock
                .Setup(x => x.Book(It.IsAny<BookingRequest>(), It.IsAny<string>()))
                .ReturnsAsync(Result.Failure<EdoContracts.Accommodations.Booking, ProblemDetails>(problemDetails));


        public static IReturnsResult<ISupplierConnector> SetupConnectorBookThrowsException(Mock<ISupplierConnector> supplierConnectorMock)
            => supplierConnectorMock
                .Setup(x => x.Book(It.IsAny<BookingRequest>(), It.IsAny<string>()))
                .ThrowsAsync(new Exception());
    }
}
