namespace HappyTravel.Edo.Api.Infrastructure.Options
{
    public class UrlGenerationOptions
    {
        public byte[] AesKey { get; set; } = new byte[32];

        public byte[] AesIV { get; set; } = new byte[16];
    }
}
