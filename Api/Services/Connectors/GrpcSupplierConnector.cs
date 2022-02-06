using System;
using System.IO;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using HappyTravel.Edo.Api.Infrastructure;
using HappyTravel.EdoContracts.Accommodations;
using Microsoft.AspNetCore.Mvc;

namespace HappyTravel.Edo.Api.Services.Connectors;

public class GrpcSupplierConnector : ISupplierConnector
{
    public async Task<Result<Availability, ProblemDetails>> GetAvailability(AvailabilityRequest availabilityRequest, string languageCode) 
        => throw new NotImplementedException();


    public async Task<Result<AccommodationAvailability, ProblemDetails>> GetAvailability(string availabilityId, string accommodationId, string languageCode) 
        => throw new NotImplementedException();


    public async Task<Result<RoomContractSetAvailability?, ProblemDetails>> GetExactAvailability(string availabilityId, Guid roomContractSetId, string languageCode) 
        => throw new NotImplementedException();


    public async Task<Result<Deadline, ProblemDetails>> GetDeadline(string availabilityId, Guid roomContractSetId, string languageCode) 
        => throw new NotImplementedException();


    public async Task<Result<Booking, ProblemDetails>> Book(BookingRequest request, string languageCode) 
        => throw new NotImplementedException();


    public async Task<Result<Unit, ProblemDetails>> CancelBooking(string referenceCode) 
        => throw new NotImplementedException();


    public async Task<Result<Booking, ProblemDetails>> GetBookingDetails(string referenceCode, string languageCode) 
        => throw new NotImplementedException();


    public async Task<Result<Booking, ProblemDetails>> ProcessAsyncResponse(Stream stream) 
        => throw new NotImplementedException();
}