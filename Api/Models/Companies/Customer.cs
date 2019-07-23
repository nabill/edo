using System.ComponentModel.DataAnnotations;
using Newtonsoft.Json;

namespace HappyTravel.Edo.Api.Models.Companies
{
    public readonly struct Customer
    {
        [JsonConstructor]
        public Customer(string title, string firstName, string lastName, string login, string email, string password)
        {
            Title = title;
            FirstName = firstName;
            LastName = lastName;
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