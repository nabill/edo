using Newtonsoft.Json;

namespace HappyTravel.Edo.Api.Models.Bookings
{
    public readonly struct Pax
    {
        [JsonConstructor]
        public Pax(PassengerTitle title, string surname, bool isLeader = false, string name = null, string initial = null, int? age = null)
        {
            Title = title;
            Surname = surname;
            Name = name;
            Initial = initial;
            IsLeader = isLeader;
            Age = age;
        }
		
        public PassengerTitle Title { get; }
		
        public string Surname { get; }

        public bool IsLeader { get; }
		
        public string Name { get; }
		
        public string Initial { get; }

        public int? Age { get; }
    }
}
