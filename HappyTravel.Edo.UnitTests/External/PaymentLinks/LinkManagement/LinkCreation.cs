using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using HappyTravel.Edo.Api.Infrastructure;
using HappyTravel.Edo.Api.Infrastructure.Options;
using HappyTravel.Edo.Api.Models.Payments.External.PaymentLinks;
using HappyTravel.Edo.Api.Services.CodeProcessors;
using HappyTravel.Edo.Api.Services.Payments.External.PaymentLinks;
using HappyTravel.Edo.Common.Enums;
using HappyTravel.Edo.Data;
using HappyTravel.Edo.Data.PaymentLinks;
using HappyTravel.Money.Enums;
using Microsoft.Extensions.Options;
using Moq;
using Xunit;

namespace HappyTravel.Edo.UnitTests.External.PaymentLinks.LinkManagement
{
    public class LinkCreation
    {
        public LinkCreation(Mock<EdoContext> edoContextMock)
        {
            _edoContextMock = edoContextMock;

            _edoContextMock
                .Setup(e => e.PaymentLinks.Add(It.IsAny<PaymentLink>()))
                .Callback<PaymentLink>(link => LastCreatedLink = link);
        }


        [Theory]
        [MemberData(nameof(ValidLinkDataList))]
        public async Task Valid_links_should_be_registered_successfully(CreatePaymentLinkRequest paymentLinkData)
        {
            var linksStorage = CreateService();
            var (_, isFailure, _, _) = await linksStorage.Register(paymentLinkData);

            Assert.False(isFailure);
            AssertLinkDataIsStored(paymentLinkData);
        }


        [Theory]
        [MemberData(nameof(LinkDataConflictingWithSettings))]
        public async Task Registering_link_should_validate_against_client_settings(CreatePaymentLinkRequest paymentLinkData)
        {
            var linksStorage = CreateService(GetOptions());

            var (_, isGenerateFailure, _, _) = await linksStorage.Register(paymentLinkData);
            Assert.True(isGenerateFailure);


            IOptions<PaymentLinkOptions> GetOptions()
                => Options.Create(new PaymentLinkOptions
                {
                    ClientSettings = new ClientSettings
                    {
                        Currencies = new List<Currencies> {Currencies.AED, Currencies.EUR},
                        ServiceTypes = new Dictionary<ServiceTypes, string>
                        {
                            {ServiceTypes.HTL, "Hotel booking"}
                        }
                    }
                });
        }


        [Theory]
        [MemberData(nameof(InvalidLinkDataList))]
        public async Task Register_link_should_fail_for_invalid_data(CreatePaymentLinkRequest paymentLinkData)
        {
            var linksStorage = CreateService();
            var (_, isSendFailure, _) = await linksStorage.Register(paymentLinkData);
            Assert.True(isSendFailure);
        }


        private PaymentLinksStorage CreateService(IOptions<PaymentLinkOptions> options = null,
            ITagProcessor tagProcessor = null)
        {
            options ??= GetValidOptions();
            tagProcessor ??= Mock.Of<ITagProcessor>();

            return new PaymentLinksStorage(_edoContextMock.Object,
                new DefaultDateTimeProvider(), 
                options,
                tagProcessor);


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


        private void AssertLinkDataIsStored(CreatePaymentLinkRequest paymentLinkData)
        {
            Assert.Equal(paymentLinkData.Amount, LastCreatedLink.Amount);
            Assert.Equal(paymentLinkData.Comment, LastCreatedLink.Comment);
            Assert.Equal(paymentLinkData.Currency, LastCreatedLink.Currency);
            Assert.Equal(paymentLinkData.Email, LastCreatedLink.Email);
            Assert.Equal(paymentLinkData.ServiceType, LastCreatedLink.ServiceType);
        }


        private readonly Mock<EdoContext> _edoContextMock;
        private PaymentLink LastCreatedLink { get; set; }

        public static readonly object[][] ValidLinkDataList =
        {
            new object[] {new CreatePaymentLinkRequest(121, "hit@yy.com", ServiceTypes.HTL, Currencies.EUR, "comment1")},
            new object[] {new CreatePaymentLinkRequest((decimal) 433.1, "antuan@xor.com", ServiceTypes.TRN, Currencies.AED, "comment2")},
            new object[] {new CreatePaymentLinkRequest(55000, "rokfeller@bank.com", ServiceTypes.HTL, Currencies.EUR, "comment3")},
            new object[] {new CreatePaymentLinkRequest((decimal) 77.77, "lucky@fortune.en", ServiceTypes.TRN, Currencies.AED, "comment4")},
            new object[] {new CreatePaymentLinkRequest((decimal) 0.01, "minimal@techno.com", ServiceTypes.HTL, Currencies.EUR, "comment5")}
        };

        public static readonly object[][] InvalidLinkDataList =
        {
            new object[] {new CreatePaymentLinkRequest(-121, "hit@yy.com", ServiceTypes.HTL, Currencies.EUR, "Invalid amount")},
            new object[] {new CreatePaymentLinkRequest((decimal) 433.1, "antuan.com", ServiceTypes.TRN, Currencies.AED, "Invalid email")},
            new object[] {new CreatePaymentLinkRequest(55000, "rokfeller@bank.com", ServiceTypes.HTL, Currencies.NotSpecified, "Unspecified currency")}
        };

        public static readonly object[][] LinkDataConflictingWithSettings =
        {
            new object[] {new CreatePaymentLinkRequest(121, "hit@yy.com", ServiceTypes.TRN, Currencies.AED, "Not allowed service type")},
            new object[] {new CreatePaymentLinkRequest((decimal) 433.1, "antuan@xor.com", ServiceTypes.TRN, Currencies.EUR, "Not allowed currency")}
        };
    }
}