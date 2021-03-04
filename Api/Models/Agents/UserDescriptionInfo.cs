using System.ComponentModel.DataAnnotations;
using Newtonsoft.Json;

namespace HappyTravel.Edo.Api.Models.Agents
{
    public readonly struct UserDescriptionInfo
    {
        [JsonConstructor]
        public UserDescriptionInfo(string title, string firstName, string lastName,
            string position, string email)
        {
            Title = title;
            FirstName = firstName;
            LastName = lastName;
            Position = position;
            Email = email;
        }


        /// <summary>
        ///     Title, Mr, Mrs etc.
        /// </summary>
        [Required]
        public string Title { get; }

        /// <summary>
        ///     First name.
        /// </summary>
        [Required]
        public string FirstName { get; }

        /// <summary>
        ///     Last name.
        /// </summary>
        [Required]
        public string LastName { get; }

        /// <summary>
        ///     Position or designation.
        /// </summary>
        public string Position { get; }

        /// <summary>
        ///     Email
        /// </summary>
        public string Email { get; }
    }
}