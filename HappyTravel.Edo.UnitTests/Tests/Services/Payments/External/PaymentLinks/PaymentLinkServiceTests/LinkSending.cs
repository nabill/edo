using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using HappyTravel.Edo.Api.Infrastructure.Options;
using HappyTravel.Edo.Api.Models.Company;
using HappyTravel.Edo.Api.Models.Payments.External.PaymentLinks;
using HappyTravel.Edo.Api.Services.Company;
using HappyTravel.Edo.Api.Services.Payments.External.PaymentLinks;
using HappyTravel.Edo.Common.Enums;
using HappyTravel.Edo.Data.PaymentLinks;
using HappyTravel.Money.Enums;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Moq;
using Xunit;

namespace HappyTravel.Edo.UnitTests.Tests.Services.Payments.External.PaymentLinks.PaymentLinkServiceTests
{
    public class LinkSending
    {
        [Fact]
        public async Task Failed_to_send_link_should_fail()
        {
            var linkService = CreateService(notificationService: GetNotificationServiceWithFailResult());
            
            var (_, isFailure, _) = await linkService.Send(LinkCreationRequest);

            Assert.True(isFailure);


            IPaymentLinkNotificationService GetNotificationServiceWithFailResult()
            {
                var notificationServiceMock = new Mock<IPaymentLinkNotificationService>();
                notificationServiceMock.Setup(m => m.SendLink(It.IsAny<PaymentLinkData>(), It.IsAny<string>()))
                    .ReturnsAsync(Result.Failure("Some error"));

                return notificationServiceMock.Object;
            }
        }


        [Fact]
        public async Task Send_link_should_send_link_by_email()
        {
            var notificationMock = new Mock<IPaymentLinkNotificationService>();
            var linkService = CreateService(notificationService: notificationMock.Object);
            
            var (_, isFailure, _) = await linkService.Send(LinkCreationRequest);

            Assert.False(isFailure);
            notificationMock
                .Verify(n=>n.SendLink(It.IsAny<PaymentLinkData>(), It.IsAny<string>()), Times.Once);
        }


        [Fact]
        public async Task Send_link_should_register_link()
        {
            var storageMock = CreateStorageMock();
            var linkService = CreateService(notificationService: GetNotificationServiceWithOkResult(), storage:storageMock.Object);
            
            var (_, isFailure, _) = await linkService.Send(LinkCreationRequest);

            Assert.False(isFailure);
            storageMock.Verify(s => s.Register(It.IsAny<PaymentLinkCreationRequest>()), Times.Once);
        }
        
        
        
        [Fact]
        public async Task Generate_url_should_register_link()
        {
            var storageMock = CreateStorageMock();
            var linkService = CreateService(notificationService: GetNotificationServiceWithOkResult(), storage: storageMock.Object);
            
            var (_, isFailure, _) = await linkService.GenerateUri(LinkCreationRequest);

            Assert.False(isFailure);
            storageMock.Verify(s => s.Register(It.IsAny<PaymentLinkCreationRequest>()), Times.Once);
        }
        

        private static Mock<IPaymentLinksStorage> CreateStorageMock()
        {
            var mock = new Mock<IPaymentLinksStorage>();
            mock
                .Setup(s => s.Register(It.IsAny<PaymentLinkCreationRequest>()))
                .Returns(Task.FromResult(Result.Success(new PaymentLink())));
            return mock;
        }
        
 
        private static IPaymentLinkNotificationService GetNotificationServiceWithOkResult()
        {
            var notificationServiceMock = new Mock<IPaymentLinkNotificationService>();
            notificationServiceMock.Setup(m => m.SendLink(It.IsAny<PaymentLinkData>(), It.IsAny<string>()))
                .ReturnsAsync(Result.Success);

            return notificationServiceMock.Object;
        }


        private PaymentLinkService CreateService(IOptions<PaymentLinkOptions>? options = null,
            IPaymentLinkNotificationService? notificationService = null,
            IPaymentLinksStorage? storage = null)
        {
            options ??= GetValidOptions();
            notificationService ??= GetNotificationServiceWithOkResult();
            storage ??= CreateStorageMock().Object;

            return new PaymentLinkService(options,
                notificationService,
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
                    PaymentUrlPrefix = "https://test/prefix"
                });
        }


        private static readonly PaymentLinkCreationRequest LinkCreationRequest =
            new PaymentLinkCreationRequest(121, "hit@yy.com", ServiceTypes.HTL, Currencies.EUR, "comment1");
    }
}