namespace HappyTravel.Edo.Api.Models.Payments.NGenius
{
    public struct Secure3dOptions
    {
        public Secure3dOptions(string acsUrl, string acsPaReq, string acsMd)
        {
            AcsUrl = acsUrl;
            AcsPaReq = acsPaReq;
            AcsMd = acsMd;
        }


        public string AcsUrl { get; }
        public string AcsPaReq { get; }
        public string AcsMd { get; } 
    }
}