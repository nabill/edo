using System.Collections.Generic;
using HappyTravel.Edo.Api.Models.Locations.Google;
using HappyTravel.Edo.Common.Enums;

namespace HappyTravel.Edo.Api.Models.Locations
{
    public readonly struct Prediction
    {
        public Prediction(string id, string countryCode, PredictionSources source, List<Match> matches, LocationTypes type, string value)
        {
            Id = id;
            CountryCode = countryCode;
            Matches = matches;
            Source = source;
            Type = type;
            Value = value;
        }


        /// <summary>
        ///     The ID of a prediction.
        /// </summary>
        public string Id { get; }

        /// <summary>
        ///     The country code of a prediction.
        /// </summary>
        public string CountryCode { get; }

        /// <summary>
        ///     The list of query occurrences in the prediction value.
        /// </summary>
        public List<Match> Matches { get; }

        /// <summary>
        ///     The source of a prediction. <see cref="PredictionSources" />
        /// </summary>
        public PredictionSources Source { get; }

        /// <summary>
        ///     Type of a predicted location. <see cref="LocationTypes" />
        /// </summary>
        public LocationTypes Type { get; }

        /// <summary>
        ///     The combined value from prediction country, locality, and name values to display for a user.
        /// </summary>
        public string Value { get; }
    }
}