namespace HappyTravel.Edo.Api.Models.Analytics;

public readonly struct WideSearchSupplierStartedEvent
{
    public WideSearchSupplierStartedEvent(string supplierCode, string supplierName)
    {
        SupplierCode = supplierCode;
        SupplierName = supplierName;
    }


    public string SupplierCode { get; }
    public string SupplierName { get; }
}