using HappyTravel.Edo.Api.NotificationCenter.Hubs;
using Microsoft.AspNetCore.SignalR;
using Moq;
using System;
using System.Threading.Tasks;
using Xunit;

namespace HappyTravel.Edo.UnitTests.Tests.NotificationCenter
{
    public class NotificationCenterTests : IDisposable
    {
        [Fact]
        [Trait("NotificationHub", "ReceiveMessage")]
        public async Task The_message_must_be_sent_via_the_hub()
        {
            // Arrange
            var notificationHub = new Mock<IHubContext<NotificationHub, INotificationClient>>();
            var mockClientProxy = new Mock<INotificationClient>();
            var mockClients = new Mock<IHubClients<INotificationClient>>();
            mockClients.Setup(clients => clients.User(It.IsAny<string>())).Returns(mockClientProxy.Object);
            notificationHub.Setup(x => x.Clients).Returns(() => mockClients.Object);

            var userId = "agent-1";
            var messageId = 1;
            var message = "Test message";

            // Act
            await notificationHub.Object.Clients.User(userId).ReceiveMessage(messageId, message);

            // Assert
            mockClientProxy.Verify(x => x.ReceiveMessage(messageId, message), Times.Once);
        }


        public void Dispose() { }
    }
}
