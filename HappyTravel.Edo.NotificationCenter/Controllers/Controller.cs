using System.Threading.Tasks;
using HappyTravel.Edo.NotificationCenter.Models;
using HappyTravel.Edo.NotificationCenter.Services.Message;
using Microsoft.AspNetCore.Mvc;

namespace HappyTravel.Edo.NotificationCenter.Controllers
{
    [ApiController]
    public class Controller : ControllerBase
    {
        public Controller(IMessageService messageService)
        {
            _messageService = messageService;
        }


        [HttpGet]
        public async Task<IActionResult> GetMessages()
        {
            return Ok();
        }
        
        
        [HttpPost("add")]
        public async Task<IActionResult> AddMessage(Request request)
        {
            await _messageService.Add(request);
            return Ok();
        }


        [HttpPost("{messageId}/mark-as-read")]
        public async Task<IActionResult> MarkAsRead(int messageId)
        {
            await _messageService.MarkAsRead(messageId);
            return Ok();
        }


        private readonly IMessageService _messageService;
    }
}