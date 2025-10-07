using CRM.Models;

namespace CRM.Services.Interfaces
{
    public interface IChatServices 
    {
        public Task<Chat> Ask(Chat chat);
        public Task<Chat> ListAvailableModels();
    }
}
