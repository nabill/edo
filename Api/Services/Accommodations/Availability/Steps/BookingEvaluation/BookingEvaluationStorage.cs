using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using FloxDc.CacheFlow;
using FloxDc.CacheFlow.Extensions;
using HappyTravel.Edo.Api.Models.Accommodations;
using HappyTravel.Edo.Api.Models.Markups;
using HappyTravel.Edo.Common.Enums;
using HappyTravel.EdoContracts.Accommodations;
using HappyTravel.SuppliersCatalog;
using AccommodationInfo = HappyTravel.Edo.Api.Models.Accommodations.AccommodationInfo;
using RoomContractSetAvailability = HappyTravel.EdoContracts.Accommodations.RoomContractSetAvailability;

namespace HappyTravel.Edo.Api.Services.Accommodations.Availability.Steps.BookingEvaluation
{
    public class BookingEvaluationStorage : IBookingEvaluationStorage
    {
        public BookingEvaluationStorage(IDoubleFlow doubleFlow)
        {
            _doubleFlow = doubleFlow;
        }


        public Task Set(Guid searchId, Guid roomContractSetId, DataWithMarkup<RoomContractSetAvailability> availability,
            Suppliers supplier, List<PaymentTypes> availablePaymentTypes, string htId, SlimAccommodation accommodation, Deadline supplierDeadline)
        {
            var key = BuildKey(searchId, htId, roomContractSetId);
            var result = SupplierData.Create(supplier, availability);
            var roomSetAvailability = availability.Data;
            
            var location = accommodation.Location;
            var roomContractSet = roomSetAvailability.RoomContractSet.ToRoomContractSet(result.Source,
                roomSetAvailability.RoomContractSet.IsDirectContract);
            
            var dataWithMarkup = result.Data;
            
            var bookingAvailabilityInfo = new BookingAvailabilityInfo(
                accommodationId: roomSetAvailability.AccommodationId,
                accommodationName: accommodation.Name,
                accommodationInfo: new AccommodationInfo(accommodation.Photo),
                roomContractSet: roomContractSet,
                zoneName: location.LocalityZone,
                localityName: location.Locality,
                countryName: location.Country,
                countryCode: location.CountryCode,
                address: location.Address,
                coordinates: location.Coordinates,
                checkInDate: roomSetAvailability.CheckInDate,
                checkOutDate: roomSetAvailability.CheckOutDate,
                numberOfNights: roomSetAvailability.NumberOfNights,
                supplier: result.Source,
                appliedMarkups: dataWithMarkup.AppliedMarkups,
                convertedSupplierPrice: dataWithMarkup.ConvertedSupplierPrice,
                originalSupplierPrice: dataWithMarkup.OriginalSupplierPrice,
                availabilityId: roomSetAvailability.AvailabilityId,
                htId: htId,
                availablePaymentTypes: availablePaymentTypes,
                isDirectContract: roomSetAvailability.RoomContractSet.IsDirectContract,
                supplierDeadline: supplierDeadline.ToDeadline());
            
            return _doubleFlow.SetAsync(key, bookingAvailabilityInfo, CacheExpirationTime);
        }


        public async Task<Result<BookingAvailabilityInfo>> Get(Guid searchId, string htId, Guid roomContractSetId)
        {
            var key = BuildKey(searchId, htId, roomContractSetId);
            
            var result = await _doubleFlow.GetAsync<BookingAvailabilityInfo>(key, CacheExpirationTime);
            return result.Equals(default) 
                ? Result.Failure<BookingAvailabilityInfo>("Could not find evaluation result") 
                : result;
        }

        
        private string BuildKey(Guid searchId, string htId, Guid roomContractSetId) 
            => _doubleFlow.BuildKey(searchId.ToString(), htId, roomContractSetId.ToString());
        
        
        private static readonly TimeSpan CacheExpirationTime = TimeSpan.FromMinutes(15);
        private readonly IDoubleFlow _doubleFlow;
    }
}