using HappyTravel.Edo.Common.Enums;

namespace HappyTravel.Edo.Api.Infrastructure.Users
{
    public readonly struct UserInfo
    {
        public UserInfo(int id, UserType type)
        {
            Id = id;
            Type = type;
        }
        public int Id { get; }
        public UserType Type { get; }
    }
}