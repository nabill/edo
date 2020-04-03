using System.Collections.Generic;

namespace HappyTravel.Edo.Api.Models.Customers
{
    public readonly struct CustomerDescription
    {
        public CustomerDescription(string email, string lastName, string firstName, string title, string position, List<CustomerCounterpartyInfo> counterparties)
        {
            Email = email;
            LastName = lastName;
            FirstName = firstName;
            Title = title;
            Position = position;
            Counterparties = counterparties;
        }


        /// <summary>
        ///     Customer e-mail.
        /// </summary>
        public string Email { get; }

        /// <summary>
        ///     Last name.
        /// </summary>
        public string LastName { get; }

        /// <summary>
        ///     First name.
        /// </summary>
        public string FirstName { get; }

        /// <summary>
        ///     Title (Mr., Mrs etc).
        /// </summary>
        public string Title { get; }

        /// <summary>
        ///     Customer position in counterparty.
        /// </summary>
        public string Position { get; }

        /// <summary>
        ///     List of counterparties, associated with customer.
        /// </summary>
        public List<CustomerCounterpartyInfo> Counterparties { get; }
    }
}