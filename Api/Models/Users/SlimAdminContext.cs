namespace HappyTravel.Edo.Api.Models.Users
{
    public readonly struct SlimAdminContext
    {
        public SlimAdminContext(int adminId)
        {
            AdminId = adminId;
        }


        public int AdminId { get; }
    }
}
