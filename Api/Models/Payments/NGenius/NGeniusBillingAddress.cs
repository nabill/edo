namespace HappyTravel.Edo.Api.Models.Payments.NGenius
{
    public readonly struct NGeniusBillingAddress
    {
        public string FirstName { get; init; }
        public string LastName { get; init; }
        public string Address1 { get; init; }
        public string City { get; init; }
        public string CountryCode { get; init; }
    }
}