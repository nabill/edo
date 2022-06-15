using System;
using System.Linq;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using HappyTravel.Edo.Api.Infrastructure;
using HappyTravel.Edo.Api.Infrastructure.Logging;
using HappyTravel.Edo.Api.Models.Agents;
using HappyTravel.Edo.Api.Services.Accommodations.Availability.Steps.RoomSelection;
using HappyTravel.Edo.Api.Services.Accommodations.Availability.Steps.WideAvailabilitySearch;
using HappyTravel.Edo.Api.Services.Connectors;
using HappyTravel.Edo.Data.Bookings;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace HappyTravel.Edo.Api.Services.Accommodations.Availability
{
    public class DeadlineService : IDeadlineService
    {
        public DeadlineService(IWideAvailabilityStorage availabilityStorage, 
            IRoomSelectionStorage roomSelectionStorage,
            ISupplierConnectorManager supplierConnectorManager,
            IAccommodationBookingSettingsService accommodationBookingSettingsService, 
            ILogger<DeadlineService> logger)
        {
            _availabilityStorage = availabilityStorage;
            _roomSelectionStorage = roomSelectionStorage;
            _supplierConnectorManager = supplierConnectorManager;
            _accommodationBookingSettingsService = accommodationBookingSettingsService;
            _logger = logger;
        }


        public async Task<Result<Deadline, ProblemDetails>> GetDeadlineDetails(Guid searchId, string htId, Guid roomContractSetId, AgentContext agent,
            string languageCode)
        {
            Tags.AddSearchId(searchId);
            var searchSettings = await _accommodationBookingSettingsService.Get();
            var (_, isFailure, result, _) = await GetDeadlineByWideAvailabilitySearchStorage();
            // This request can be from first and second step, that is why we check two caches.
            return isFailure 
                ? await GetDeadlineByRoomSelectionStorage() 
                : result;


            async Task<Result<Deadline, ProblemDetails>> GetDeadlineByRoomSelectionStorage()
            {
                var selectedResult = await _roomSelectionStorage.GetResult(searchId, htId, searchSettings.EnabledConnectors);
                var selectedRoomSet = selectedResult
                    .SelectMany(r =>
                    {
                        return r.Result.RoomContractSets
                            .Select(rs => (SupplierCode: r.SupplierCode, RoomContractSetId: rs.Id, AvailabilityId: r.Result.AvailabilityId));
                    })
                    .SingleOrDefault(r => r.RoomContractSetId == roomContractSetId);

                if (selectedRoomSet.Equals(default))
                    return ProblemDetailsBuilder.Fail<Deadline>("Could not find selected room contract set");
                
                if (selectedRoomSet.RoomContractSetId.Equals(default))
                    return ProblemDetailsBuilder.Fail<Deadline>("Could not find RoomContractSetId for selected room set");

                var checkInDate = selectedResult.Select(s => s.Result.CheckInDate).FirstOrDefault();
                return await MakeSupplierRequest(selectedRoomSet.SupplierCode, selectedRoomSet.RoomContractSetId, selectedRoomSet.AvailabilityId)
                    .Bind(deadline => ProcessDeadline(deadline, checkInDate, agent));
            }


            async Task<Result<Deadline, ProblemDetails>> GetDeadlineByWideAvailabilitySearchStorage()
            {
                var selectedResults = (await _availabilityStorage.GetResults(searchId, searchSettings))
                    .SelectMany(r => r.AccommodationAvailabilities.Select(a => (r.SupplierCode, a)))
                    .Where(r => r.a.HtId == htId)
                    .ToList();
                
                foreach (var (SupplierKey, a) in selectedResults)
                {
                    var selectedRoom = a.RoomContractSets?.SingleOrDefault(r => r.Id == roomContractSetId);

                    if (selectedRoom is not null && !selectedRoom.Id.Equals(default))
                        return await MakeSupplierRequest(SupplierKey, selectedRoom.Id, a.AvailabilityId)
                            .Bind(d => ProcessDeadline(d, a.CheckInDate, agent));
                }

                return ProblemDetailsBuilder.Fail<Deadline>("Could not find selected room contract set");
            }


            Task<Result<EdoContracts.Accommodations.Deadline, ProblemDetails>> MakeSupplierRequest(string supplierCode, Guid roomSetId, string availabilityId)
                => _supplierConnectorManager.Get(supplierCode).GetDeadline(availabilityId, roomSetId, languageCode);
        }
        
        
        private async Task<Result<Deadline, ProblemDetails>> ProcessDeadline(EdoContracts.Accommodations.Deadline deadline, DateTimeOffset checkInDate, AgentContext agent)
        {
            var settings = await _accommodationBookingSettingsService.Get();
            return DeadlinePolicyProcessor.Process(deadline.ToDeadline(), checkInDate, settings.CancellationPolicyProcessSettings);
        }


        private readonly ISupplierConnectorManager _supplierConnectorManager;
        private readonly IAccommodationBookingSettingsService _accommodationBookingSettingsService;
        private readonly IWideAvailabilityStorage _availabilityStorage;
        private readonly IRoomSelectionStorage _roomSelectionStorage;
        private readonly ILogger<DeadlineService> _logger;
    }
}