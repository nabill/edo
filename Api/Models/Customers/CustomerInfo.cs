namespace HappyTravel.Edo.Api.Models.Customers
{
    public readonly struct CustomerInfo
    {
        public CustomerInfo(string email, string lastName, string firstName, string title, string position)
        {
            Email = email;
            LastName = lastName;
            FirstName = firstName;
            Title = title;
            Position = position;
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
        ///     Customer position in company.
        /// </summary>
        public string Position { get; }
    }
}