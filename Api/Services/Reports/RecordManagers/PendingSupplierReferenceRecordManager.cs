using System;
using System.Linq;
using HappyTravel.Edo.Api.Models.Reports;
using HappyTravel.Edo.Data;

namespace HappyTravel.Edo.Api.Services.Reports.RecordManagers
{
    public class PendingSupplierReferenceRecordManager : IRecordManager<PendingSupplierReference>
    {
        public PendingSupplierReferenceRecordManager(EdoContext context)
        {
            _context = context;
        }


        public IQueryable<PendingSupplierReference> Get(DateTime fromDate, DateTime endDate)
        {
            return from booking in _context.Bookings
                where
                    booking.SupplierReferenceCode == null
                select new PendingSupplierReference
                {
                    ReferenceCode = booking.ReferenceCode,
                    Date = booking.Created
                };
        }
        
        
        private readonly EdoContext _context;
    }
}