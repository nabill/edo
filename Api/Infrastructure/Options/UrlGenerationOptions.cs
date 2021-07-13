namespace HappyTravel.Edo.Api.Infrastructure.Options
{
    public class UrlGenerationOptions
    {
        public string ConfirmationPageUrl { get; set; }
        public byte[] AesKey { get; set; } = new byte[32];
        public byte[] AesIV { get; set; } = new byte[16];
    }
}
