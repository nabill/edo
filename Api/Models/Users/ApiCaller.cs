using HappyTravel.Edo.Common.Enums;

namespace HappyTravel.Edo.Api.Models.Users
{
    public readonly struct ApiCaller
    {
        public ApiCaller(string id, ApiCallerTypes type)
        {
            Id = id;
            Type = type;
        }


        public string Id { get; }
        public ApiCallerTypes Type { get; }
        
        public static ApiCaller InternalServiceAccount 
            => new("0", ApiCallerTypes.InternalServiceAccount);
        
        public static ApiCaller FromSupplier(string supplierCode) 
            => new(supplierCode, ApiCallerTypes.Supplier);
    }
}