using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using HappyTravel.Edo.Api.Infrastructure;
using HappyTravel.Edo.Api.Infrastructure.Options;
using HappyTravel.Edo.Api.Models.Company;
using HappyTravel.Edo.Api.Models.Payments.External.PaymentLinks;
using HappyTravel.Edo.Api.Services.Company;
using HappyTravel.Edo.Api.Services.Payments.External.PaymentLinks;
using HappyTravel.Edo.Common.Enums;
using HappyTravel.Edo.Data.Documents;
using HappyTravel.Edo.Data.PaymentLinks;
using HappyTravel.MailSender;
using HappyTravel.Money.Enums;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Moq;
using Xunit;

namespace HappyTravel.Edo.UnitTests.External.PaymentLinks.LinkManagement
{
    public class LinkSending
    {
        [Fact]
        public async Task Failed_to_send_link_should_fail()
        {
            var linkService = CreateService(mailSender: GetMailSenderWithFailResult());
            
            var (_, isFailure, _) = await linkService.Send(LinkCreationRequest);

            Assert.True(isFailure);


            MailSenderWithCompanyInfo GetMailSenderWithFailResult()
            {
                var mailSenderMock = new Mock<IMailSender>();
                mailSenderMock.Setup(m => m.Send(It.IsAny<string>(), It.IsAny<IEnumerable<string>>(), It.IsAny<object>()))
                    .ReturnsAsync(Result.Failure("Some error"));

                var companyService = GetCompanyService();
                return new MailSenderWithCompanyInfo(mailSenderMock.Object, companyService);
            }
        }


        [Fact]
        public async Task Send_link_should_send_link_by_email()
        {
            var mailSenderMock = CreateMailSenderMockWithCallback();
            var linkService = CreateService(mailSender: mailSenderMock);
            
            var (_, isFailure, _) = await linkService.Send(LinkCreationRequest);

            Assert.False(isFailure);
            Assert.Equal(LastSentMailData.addressee, LinkCreationRequest.Email);


            MailSenderWithCompanyInfo CreateMailSenderMockWithCallback()
            {
                var senderMock = new Mock<IMailSender>();
                senderMock.Setup(m => m.Send(It.IsAny<string>(), It.IsAny<IEnumerable<string>>(), It.IsAny<object>()))
                    .ReturnsAsync(Result.Ok)
                    .Callback<string, IEnumerable<string>, object>((templateId, addressee, mailData)
                        => LastSentMailData = (templateId, addressee.First(), mailData));

                var companyService = GetCompanyService();
                return new MailSenderWithCompanyInfo(senderMock.Object, companyService);
            }
        }


        [Fact]
        public async Task Send_link_should_register_link()
        {
            var storageMock = CreateStorageMock();
            var linkService = CreateService(mailSender: GetMailSenderWithOkResult(), storage:storageMock.Object);
            
            var (_, isFailure, _) = await linkService.Send(LinkCreationRequest);

            Assert.False(isFailure);
            storageMock.Verify(s => s.Register(It.IsAny<PaymentLinkCreationRequest>()), Times.Once);
        }
        
        
        [Fact]
        public async Task Send_link_should_create_invoice()
        {
            var documentServiceMock = CreateDocumentServiceMock();
            var linkService = CreateService(mailSender: GetMailSenderWithOkResult(), documentsService:documentServiceMock.Object);
            
            var (_, isFailure, _) = await linkService.Send(LinkCreationRequest);

            Assert.False(isFailure);
            documentServiceMock.Verify(s => s.GenerateInvoice(It.IsAny<PaymentLinkData>()), Times.Once);
        }
        
        
        [Fact]
        public async Task Generate_url_should_register_link()
        {
            var storageMock = CreateStorageMock();
            var linkService = CreateService(mailSender: GetMailSenderWithOkResult(), storage: storageMock.Object);
            
            var (_, isFailure, _) = await linkService.GenerateUri(LinkCreationRequest);

            Assert.False(isFailure);
            storageMock.Verify(s => s.Register(It.IsAny<PaymentLinkCreationRequest>()), Times.Once);
        }

        
        [Fact]
        public async Task Generate_url_should_create_invoice()
        {
            var documentServiceMock = CreateDocumentServiceMock();
            var linkService = CreateService(mailSender: GetMailSenderWithOkResult(), documentsService:documentServiceMock.Object);
            
            var (_, isFailure, _) = await linkService.GenerateUri(LinkCreationRequest);

            Assert.False(isFailure);
            documentServiceMock.Verify(s => s.GenerateInvoice(It.IsAny<PaymentLinkData>()), Times.Once);
        }
        

        private static Mock<IPaymentLinksStorage> CreateStorageMock()
        {
            var mock = new Mock<IPaymentLinksStorage>();
            mock
                .Setup(s => s.Register(It.IsAny<PaymentLinkCreationRequest>()))
                .Returns(Task.FromResult(Result.Ok(new PaymentLink())));
            return mock;
        }
        
        
        private static Mock<IPaymentLinksDocumentsService> CreateDocumentServiceMock()
        {
            var mock = new Mock<IPaymentLinksDocumentsService>();
            mock
                .Setup(s => s.GenerateInvoice(It.IsAny<PaymentLinkData>()))
                .Returns(Task.CompletedTask);
            return mock;
        }


        private static MailSenderWithCompanyInfo GetMailSenderWithOkResult()
        {
            var mailSenderMock = new Mock<IMailSender>();
            mailSenderMock.Setup(m => m.Send(It.IsAny<string>(), It.IsAny<IEnumerable<string>>(), It.IsAny<object>()))
                .ReturnsAsync(Result.Ok);
            var companyService = GetCompanyService();
            return new MailSenderWithCompanyInfo(mailSenderMock.Object, companyService);
        }


        private static IPaymentLinksDocumentsService GetDocumentsService()
        {
            var mock = new Mock<IPaymentLinksDocumentsService>();
            mock
                .Setup(i => i.GenerateInvoice(It.IsAny<PaymentLinkData>()))
                .Returns(Task.FromResult(new List<(DocumentRegistrationInfo Metadata, PaymentLinkInvoiceData Data)> {(default, default)}));

            return mock.Object;
        }


        private PaymentLinkService CreateService(IOptions<PaymentLinkOptions> options = null,
            MailSenderWithCompanyInfo mailSender = null, IPaymentLinksDocumentsService documentsService = null,
            IPaymentLinksStorage storage = null)
        {
            var companyService = GetCompanyService();
            options ??= GetValidOptions();
            mailSender ??= new MailSenderWithCompanyInfo(Mock.Of<IMailSender>(), companyService);
            documentsService ??= GetDocumentsService();
            storage ??= CreateStorageMock().Object;

            return new PaymentLinkService(options,
                mailSender,
                documentsService,
                storage,
                new NullLogger<PaymentLinkService>());


            IOptions<PaymentLinkOptions> GetValidOptions()
                => Options.Create(new PaymentLinkOptions
                {
                    ClientSettings = new ClientSettings
                    {
                        Currencies = new List<Currencies> {Currencies.AED, Currencies.EUR},
                        ServiceTypes = new Dictionary<ServiceTypes, string>
                        {
                            {ServiceTypes.HTL, "Hotel booking"},
                            {ServiceTypes.TRN, "Airport transfer"}
                        }
                    },
                    SupportedVersions = new List<Version> {new Version(0, 2)},
                    MailTemplateId = "templateId_fkIfu423_-e",
                    PaymentUrlPrefix = new Uri("https://test/prefix")
                });
        }


        private static ICompanyService GetCompanyService()
        {
            var companyServiceMock = new Mock<ICompanyService>();
            companyServiceMock.Setup(c => c.Get())
                .Returns(new ValueTask<Result<CompanyInfo>>(Result.Ok(new CompanyInfo())));
            return companyServiceMock.Object;
        }


        private static readonly PaymentLinkCreationRequest LinkCreationRequest =
            new PaymentLinkCreationRequest(121, "hit@yy.com", ServiceTypes.HTL, Currencies.EUR, "comment1");

        private (string templateId, string addressee, object linkData) LastSentMailData { get; set; }
    }
}