using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using HappyTravel.Edo.Api.Models.Reports;
using HappyTravel.Edo.Data;
using Microsoft.EntityFrameworkCore;

namespace HappyTravel.Edo.Api.Services.Reports.RecordManagers;

public class PaymentLinkReportRecordManager : IRecordManager<PaymentLinkReportData>
{
    public PaymentLinkReportRecordManager(EdoContext context)
    {
        _context = context;
    }
    
    
    public async Task<IEnumerable<PaymentLinkReportData>> Get(DateTime fromDate, DateTime endDate)
    {
        return await (from paymentLink in _context.PaymentLinks
                join admin in _context.Administrators on paymentLink.AdministratorId equals admin.Id into adminGroup
                from admin in adminGroup.DefaultIfEmpty()
                where paymentLink.Created >= fromDate && paymentLink.Created < endDate
                select new PaymentLinkReportData
                {
                    Amount = paymentLink.Amount,
                    PaymentDate = paymentLink.LastPaymentDate,
                    Currency = paymentLink.Currency,
                    InvoiceNumber = paymentLink.InvoiceNumber,
                    Administrator = admin,
                    PaymentProcessor = paymentLink.PaymentProcessor,
                    PaymentResponse = paymentLink.LastPaymentResponse,
                    ServiceType = paymentLink.ServiceType
                }
            ).ToListAsync();
    }
    
    
    private readonly EdoContext _context;
}