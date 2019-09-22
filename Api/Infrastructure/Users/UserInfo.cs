using HappyTravel.Edo.Common.Enums;

namespace HappyTravel.Edo.Api.Infrastructure.Users
{
    public readonly struct UserInfo
    {
        public UserInfo(int id, UserTypes type)
        {
            Id = id;
            Type = type;
        }
        public int Id { get; }
        public UserTypes Type { get; }
    }
}