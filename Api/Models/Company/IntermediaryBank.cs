namespace HappyTravel.Edo.Api.Models.Company
{
    public record IntermediaryBank
    {
        public string BankName { get; init; }
        public string AccountNumber { get; init; }
        public string SwiftCode { get; init; }
        public string AbaNo { get; init; }
    }
}
