using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using HappyTravel.EdoContracts.General.Enums;

namespace HappyTravel.Edo.DirectApi.Models.Booking
{
    public readonly struct Pax
    {
        [JsonConstructor]
        public Pax(PassengerTitles title, string lastName, string firstName, bool isLeader = false, int? age = null)
        {
            Age = age;
            FirstName = firstName;
            IsLeader = isLeader;
            LastName = lastName;
            Title = title;
        }


        /// <summary>
        ///     Passenger's age (<b>required for children</b>)
        /// </summary>
        public int? Age { get; }

        /// <summary>
        ///     Passenger's first name
        /// </summary>
        [Required]
        public string FirstName { get; }

        /// <summary>
        ///     Indicates if the passenger is a group leader for the booking. The flag affects voucher appearance and other booking-related details.
        /// </summary>
        [Required]
        public bool IsLeader { get; }

        /// <summary>
        ///     Passenger's last name
        /// </summary>
        [Required]
        public string LastName { get; }

        /// <summary>
        ///     Passenger's title
        /// </summary>
        [Required]
        public PassengerTitles Title { get; }
    }
}