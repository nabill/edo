using System.Threading.Tasks;

namespace HappyTravel.Edo.NotificationCenter.Services.Hub
{
    public interface INotificationClient
    {
        Task ReceiveMessage(int messageId, string message);
    }
}