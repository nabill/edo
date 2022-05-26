namespace Api.Models.Management.Administrators
{
    public readonly struct SlimAccountManager
    {
        public SlimAccountManager(int id, string name)
        {
            Id = id;
            Name = name;
        }


        /// <summary>
        /// Id of account manager
        /// </summary>
        public int Id { get; }

        /// <summary>
        /// Full name
        /// </summary>
        public string Name { get; }
    }
}