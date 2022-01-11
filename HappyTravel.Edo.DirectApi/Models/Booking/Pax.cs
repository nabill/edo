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
        ///     The passenger age. <b>Required for children</b>.
        /// </summary>
        public int? Age { get; }

        /// <summary>
        ///     The passenger first name.
        /// </summary>
        [Required]
        public string FirstName { get; }

        /// <summary>
        ///     Indicates the passenger as a group leader within a booking.
        /// </summary>
        [Required]
        public bool IsLeader { get; }

        /// <summary>
        ///     The passenger last name.
        /// </summary>
        [Required]
        public string LastName { get; }

        /// <summary>
        ///     The passenger title.
        /// </summary>
        [Required]
        public PassengerTitles Title { get; }
    }
}