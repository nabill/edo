namespace HappyTravel.Edo.DirectApi.Models
{
    public readonly struct BookingFinalizationRequest
    {
        public BookingFinalizationRequest(string referenceCode, string supplierReferenceCode)
        {
            ReferenceCode = referenceCode;
            SupplierReferenceCode = supplierReferenceCode;
        }


        public string ReferenceCode { get; }
        public string SupplierReferenceCode { get; }
    }
}