namespace HappyTravel.Edo.Api.Services.Payments.NGenius
{
    public class StateTypes
    {
        public const string Started = "STARTED";
        public const string Await3Ds = "AWAIT_3DS";
        public const string Authorized = "AUTHORISED";
        public const string PartiallyCaptured = "PARTIALLY_CAPTURED";
        public const string Captured = "CAPTURED";
        public const string Failed = "FAILED";
        public const string Reversed = "REVERSED";
    }
}