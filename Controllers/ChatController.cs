using CRM.Models;
using CRM.Services;
using CRM.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace CRM.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class ChatController : ControllerBase
    {
        private readonly IChatServices _chatServices;
        public ChatController(IChatServices chatServices)
        {
            _chatServices = chatServices;
        }

        [HttpPost("Ask")]
        public async Task<Chat> Ask([FromBody]Chat chat)
        {
            return await _chatServices.Ask(chat);
        }
        

        [HttpGet("ListAvailableModels")]
        public async Task<Chat> ListAvailableModels()
        {
            return await _chatServices.ListAvailableModels();
        }
        
    }
}
