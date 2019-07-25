using System.ComponentModel.DataAnnotations;
using Newtonsoft.Json;

namespace HappyTravel.Edo.Api.Models.Customers
{
    public readonly struct CustomerRegistrationInfo
    {
        [JsonConstructor]
        public CustomerRegistrationInfo(string title, string firstName, string lastName,
            string position, string email, string userToken)
        {
            Title = title;
            FirstName = firstName;
            LastName = lastName;
            Position = position;
            Email = email;
            UserToken = userToken;
        }

        /// <summary>
        ///     Customer title, Mr, Mrs etc.
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
        ///     Customer position or designation.
        /// </summary>
        public string Position { get; }

        /// <summary>
        ///     E-mail address.
        /// </summary>
        [Required]
        [DataType(DataType.EmailAddress)]
        public string Email { get; }

        /// <summary>
        ///     Unique user token, given by external Identity service.
        /// </summary>
        [Required]
        public string UserToken { get; }
    }
}