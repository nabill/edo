using System;
using HappyTravel.Edo.Common.Enums;

namespace HappyTravel.Edo.Data.Management
{
    public class BookingManagementAuditLogEntry : IEntity
    {
        public int Id { get; set; }
        public int BookingId { get; set; }
        public int AdministratorId { get; set; }
        public string Reason { get; set; }
        public DateTime? Date { get; set; }
        public DateTime Created { get; set; }
        public BookingManagementOperationTypes OperationType { get; set; }
    }
}