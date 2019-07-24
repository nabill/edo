using System.ComponentModel.DataAnnotations;
using Newtonsoft.Json;

namespace HappyTravel.Edo.Api.Models.Companies
{
    public readonly struct CustomerRegistrationInfo
    {
        [JsonConstructor]
        public CustomerRegistrationInfo(string title, string firstName, string lastName, string position, string login, string email, string password)
        {
            Title = title;
            FirstName = firstName;
            LastName = lastName;
            Position = position;
            Login = login;
            Email = email;
            Password = password;
        }
        
        [Required]
        public string Title { get; }
        
        [Required]
        public string FirstName { get; }
        
        [Required]
        public string LastName { get; }

        public string Position { get; }

        [Required]
        public string Login { get; }
        
        [Required]
        [DataType(DataType.EmailAddress)]
        public string Email { get; }
        
        [Required]
        [DataType(DataType.Password)]
        public string Password { get; }
    }
}