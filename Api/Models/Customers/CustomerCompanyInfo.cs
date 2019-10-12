namespace HappyTravel.Edo.Api.Models.Customers
{
    public readonly struct CustomerCompanyInfo
    {
        public CustomerCompanyInfo(int id, string name, bool isMaster)
        {
            Id = id;
            Name = name;
            IsMaster = isMaster;
        }


        /// <summary>
        ///     Id of the company.
        /// </summary>
        public int Id { get; }

        /// <summary>
        ///     Name of the company.
        /// </summary>
        public string Name { get; }

        /// <summary>
        ///     Flag indicating that customer is master in this company.
        /// </summary>
        public bool IsMaster { get; }
    }
}