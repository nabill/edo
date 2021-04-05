using HappyTravel.Edo.Common.Enums;

namespace HappyTravel.Edo.Api.Models.Users
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
        
        public static UserInfo InternalServiceAccount 
            => new UserInfo(0, UserTypes.InternalServiceAccount);
        
        public static UserInfo AnonymousSupplier 
            => new UserInfo(0, UserTypes.Supplier);
    }
}