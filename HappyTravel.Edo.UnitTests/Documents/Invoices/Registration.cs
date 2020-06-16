using System;
using System.Globalization;
using System.Threading.Tasks;
using HappyTravel.Edo.Api.Infrastructure.Converters;
using HappyTravel.Edo.Api.Services.Documents;
using HappyTravel.Edo.Common.Enums;
using HappyTravel.Edo.Data.Documents;
using HappyTravel.Edo.UnitTests.Infrastructure;
using Moq;
using Xunit;

namespace HappyTravel.Edo.UnitTests.Documents.Invoices
{
    // TODO: Add other tests
    public class Registration
    {
        private Invoice CreatedInvoice { get; set; }
        
        [Theory]
        [InlineData("2021-12-11")]
        [InlineData("2030-01-02")]
        public async Task Should_register_with_current_date(string date)
        {
            var regDate = DateTime.Parse(date, CultureInfo.InvariantCulture);
            var invoiceService = CreateInvoiceServiceWithCurrentDate(regDate);

            var registrationInfo = await invoiceService.Register(ServiceTypes.CMS, ServiceSource.Internal, "refCode", new FakeInvoice());
            
            Assert.Equal(regDate, registrationInfo.Date);
            Assert.Equal(regDate, CreatedInvoice.Date);


            IInvoiceService CreateInvoiceServiceWithCurrentDate(DateTime date)
            {
                var context = MockEdoContext.Create();
            
                context
                    .Setup(c => c.Invoices.Add(It.IsAny<Invoice>()))
                    .Callback<Invoice>((i) => CreatedInvoice = i);
                
                return new InvoiceService(context.Object, new NewtonsoftJsonSerializer(), new DateTimeProviderMock(date));
            }
        }
        
        public class FakeInvoice
        {
            
        }
    }
}