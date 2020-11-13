namespace HappyTravel.Edo.Data.Agents
{
    public class UserInvitation : InvitationBase
    {
        public UserInvitationData Data { get; set; }


        public class UserInvitationData : InvitationDataBase
        {
            public UserRegistrationInfo RegistrationInfo { get; set; }
            public int AgencyId { get; set; }
        }


        public class UserRegistrationInfo
        {
            public string Title { get; set; }
            public string FirstName { get; set; }
            public string LastName { get; set; }
            public string Position { get; set; }
        }
    }
}