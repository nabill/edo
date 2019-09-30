namespace HappyTravel.Edo.Api.Models.Customers
{
    public readonly struct CustomerCompanyInfo
    {
        public CustomerCompanyInfo(int id, string name)
        {
            Id = id;
            Name = name;
        }


        /// <summary>
        ///     Id of the company.
        /// </summary>
        public int Id { get; }

        /// <summary>
        ///     Name of the company.
        /// </summary>
        public string Name { get; }
    }
}