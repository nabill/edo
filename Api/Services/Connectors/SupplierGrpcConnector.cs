﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using Grpc.Core;
using HappyTravel.Edo.Api.Infrastructure;
using HappyTravel.Edo.Api.Infrastructure.Logging;
using HappyTravel.Edo.Api.Infrastructure.Metrics;
using HappyTravel.EdoContracts.Accommodations;
using HappyTravel.EdoContracts.Grpc.Models;
using HappyTravel.EdoContracts.Grpc.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Prometheus;
using ProtoBuf.Grpc;

namespace HappyTravel.Edo.Api.Services.Connectors;

public class SupplierGrpcConnector : ISupplierConnector
{
    public SupplierGrpcConnector(string supplierName, IConnectorGrpcService connectorClient, ILogger<SupplierGrpcConnector> logger, Dictionary<string, string> customHeaders)
    {
        _supplierName = supplierName;
        _connectorClient = connectorClient;
        _logger = logger;
        _customHeaders = customHeaders;
    }
    
    
    public Task<Result<Availability, ProblemDetails>> GetAvailability(AvailabilityRequest availabilityRequest, string languageCode)
    {
        return ExecuteWithLogging(Counters.WideAvailabilitySearch, async () =>
        {
            var result = await _connectorClient.GetWideAvailability(availabilityRequest, GetCallOptions());
            return result.Result.IsFailure
                ? Result.Failure<Availability, ProblemDetails>(ProblemDetailsBuilder.Build(result.Result.Error))
                : result.Result.Value;
        });
    }


    public Task<Result<AccommodationAvailability, ProblemDetails>> GetAvailability(string availabilityId, string accommodationId, string languageCode)
    {
        return ExecuteWithLogging(Counters.RoomSelection, async () =>
        {
            var result = await _connectorClient.GetAccommodationAvailability(new AccommodationAvailabilityRequest
            {
                AvailabilityId = availabilityId,
                AccommodationId = accommodationId
            }, GetCallOptions());
            
            return result.Result.IsFailure
                ? Result.Failure<AccommodationAvailability, ProblemDetails>(result.Result.Error)
                : result.Result.Value;
        });
    }


    public Task<Result<RoomContractSetAvailability?, ProblemDetails>> GetExactAvailability(string availabilityId, Guid roomContractSetId, string languageCode)
    {
        return ExecuteWithLogging(Counters.Evaluation, async () =>
        {
            var result = await _connectorClient.GetExactAvailability(new ExactAvailabilityRequest
            {
                AvailabilityId = availabilityId,
                RoomContractSetId = roomContractSetId
            }, GetCallOptions());
            
            return result.Result.IsFailure
                ? Result.Failure<RoomContractSetAvailability?, ProblemDetails>(ProblemDetailsBuilder.Build(result.Result.Error))
                : result.Result.Value;
        });
    }


    public Task<Result<Deadline, ProblemDetails>> GetDeadline(string availabilityId, Guid roomContractSetId, string languageCode)
    {
        return ExecuteWithLogging(Counters.BookingDeadline, async () =>
        {
            var result = await _connectorClient.GetDeadline(new DeadlineRequest
            {
                AvailabilityId = availabilityId,
                RoomContractSetId = roomContractSetId
            }, GetCallOptions());
            
            return result.Result.IsFailure
                ? Result.Failure<Deadline, ProblemDetails>(ProblemDetailsBuilder.Build(result.Result.Error))
                : result.Result.Value;
        });
    }


    public Task<Result<Booking, ProblemDetails>> Book(BookingRequest request, string languageCode)
    {
        return ExecuteWithLogging(Counters.Booking, async () =>
        {
            var result = await _connectorClient.Book(request, GetCallOptions());
            
            return result.Result.IsFailure
                ? Result.Failure<Booking, ProblemDetails>(result.Result.Error)
                : result.Result.Value;
        });
    }


    public Task<Result<Unit, ProblemDetails>> CancelBooking(string referenceCode)
    {
        return ExecuteWithLogging(Counters.Cancellation, async () =>
        {
            var result = await _connectorClient.CancelBooking(new CancelBookingRequest
            {
                ReferenceCode = referenceCode
            }, GetCallOptions());
            
            return result.Result.IsFailure
                ? Result.Failure<Unit, ProblemDetails>(ProblemDetailsBuilder.Build(result.Result.Error))
                : Unit.Instance;
        });
    }


    public Task<Result<Booking, ProblemDetails>> GetBookingDetails(string referenceCode, string languageCode)
    {
        return ExecuteWithLogging(Counters.BookingInformation, async () =>
        {
            var result = await _connectorClient.GetBooking(new BookingInfoRequest
            {
                ReferenceCode = referenceCode
            }, GetCallOptions());
            
            return result.Result.IsFailure
                ? Result.Failure<Booking, ProblemDetails>(ProblemDetailsBuilder.Build(result.Result.Error))
                : result.Result.Value;
        });
    }


    public Task<Result<Booking, ProblemDetails>> ProcessAsyncResponse(Stream stream) 
        => throw new NotImplementedException();
    
    
    private async Task<Result<TResult, ProblemDetails>> ExecuteWithLogging<TResult>(string step, Func<Task<Result<TResult, ProblemDetails>>> funcToExecute)
    {
        _logger.LogSupplierConnectorRequestStarted(string.Empty, step);
            
        using var timer = Counters.SupplierRequestHistogram.WithLabels(step, _supplierName).NewTimer();
        var result = await funcToExecute();
        timer.Dispose();
            
        Counters.SupplierRequestCounter
            .WithLabels(step,
                _supplierName,
                result.IsFailure ? result.Error.Status.ToString() : string.Empty)
            .Inc();

        LoggerUtils.WriteLogByResult(result,
            () => _logger.LogSupplierConnectorRequestSuccess(string.Empty, step),
            () => _logger.LogSupplierConnectorRequestError(string.Empty, result.Error.Detail, step, result.Error.Status));

        return result;
    }


    private CallOptions GetCallOptions()
    {
        Metadata metadata = null;

        if (_customHeaders is not null)
        {
            metadata = new Metadata();
            foreach (var (key, value) in _customHeaders)
                metadata.Add(key, value);
        }

        return new CallOptions(headers: metadata);
    }
    
    
    private readonly string _supplierName;
    private readonly IConnectorGrpcService _connectorClient;
    private readonly ILogger<SupplierGrpcConnector> _logger;
    private readonly Dictionary<string, string> _customHeaders;
}