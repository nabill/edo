namespace HappyTravel.Edo.Data.Infrastructure
{
    public class EntityLock
    {
        public string EntityDescriptor { get; set; } = string.Empty;
        public string LockerInfo { get; set; } = string.Empty;
        public string Token { get; set; } = string.Empty;
    }
}