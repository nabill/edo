using System.Threading.Tasks;
using HappyTravel.Edo.NotificationCenter.Models;

namespace HappyTravel.Edo.NotificationCenter.Services.Message
{
    public interface IMessageService
    {
        Task Add(NotificationInfo request);
        Task MarkAsRead(int messageId);
        Task GetMessages();
    }
}