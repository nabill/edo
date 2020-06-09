using System;
using HappyTravel.Edo.Common.Enums;

namespace HappyTravel.Edo.Data.Documents
{
    public class Invoice
    {
        public int Id { get; set; }
        public ServiceTypes ServiceType { get; set; }
        public ServiceSource ServiceSource { get; set; }
        public int ParentId { get; set; }
        public string Data { get; set; }
        public DateTime Date { get; set; }
    }
}