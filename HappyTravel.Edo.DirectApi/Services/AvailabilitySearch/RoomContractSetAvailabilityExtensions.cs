﻿using System;
using HappyTravel.Edo.DirectApi.Models;
using HappyTravel.Edo.DirectApi.Models.Search;

namespace HappyTravel.Edo.DirectApi.Services.AvailabilitySearch
{
    public static class RoomContractSetAvailabilityExtensions
    {
        public static RoomContractSetAvailability MapFromEdoModels(this Api.Models.Accommodations.RoomContractSetAvailability availability, Guid searchId, string accommodationId)
        {
            return new RoomContractSetAvailability(searchId: searchId,
                accommodationId: accommodationId,
                evaluationToken: availability.EvaluationToken,
                roomContractSet: availability.RoomContractSet.MapFromEdoModel());
        }
    }
}