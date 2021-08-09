namespace HappyTravel.Edo.Api.Services.Reports.Helpers
{
    public static class VatHelper
    {
        public static decimal VatAmount(decimal totalAmount) 
            => totalAmount * Vat / (100 + Vat);


        public static decimal AmountExcludedVat(decimal totalAmount) 
            => totalAmount / (1m + Vat / 100m);
        
        private const int Vat = 5;
    }
}