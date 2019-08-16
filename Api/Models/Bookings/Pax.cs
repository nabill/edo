using Newtonsoft.Json;

namespace HappyTravel.Edo.Api.Models.Bookings
{
    public readonly struct Pax
    {
        [JsonConstructor]
        public Pax(PassengerTitle title, string lastName, bool isLeader = false, string firstName = null, string initials = null, int? age = null)
        {
            Title = title;
            LastName = lastName;
            FirstName = firstName;
            Initials = initials;
            IsLeader = isLeader;
            Age = age;
        }
		
        public PassengerTitle Title { get; }
		
        public string LastName { get; }

        public bool IsLeader { get; }
		
        public string FirstName { get; }
		
        public string Initials { get; }

        public int? Age { get; }
    }
}
