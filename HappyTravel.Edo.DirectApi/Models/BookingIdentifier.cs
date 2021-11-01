namespace HappyTravel.Edo.DirectApi.Models
{
    public readonly struct BookingIdentifier
    {
        public BookingIdentifier(string clientReferenceCode, string supplierReferenceCode)
        {
            ClientReferenceCode = clientReferenceCode;
            SupplierReferenceCode = supplierReferenceCode;
        }


        public string? ClientReferenceCode { get; }
        public string? SupplierReferenceCode { get; }
    }
}