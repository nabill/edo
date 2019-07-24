using System.Collections.Generic;

namespace HappyTravel.Edo.Data.Customers
{
    public class Customer
    {
        public int Id { get; set; }
        public string Login { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Title { get; set; }
        public string Position { get; set; }
        public string Email { get; set; }
        public ICollection<CustomerCompanyRelation> CompanyRelations { get; set; }
    }
}