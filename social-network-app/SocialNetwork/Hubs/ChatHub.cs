using Microsoft.AspNetCore.SignalR;
using SocialNetwork.Core.Application.Interfaces.Services;
using SocialNetwork.Core.Application.ViewModels.Message;

namespace SocialNetwork.Hubs
{
    public class ChatHub : Hub
    {
        private readonly IMessageService _messageService;

        public ChatHub(IMessageService messageService)
        {
            _messageService = messageService;
        }

        public async Task SendMessage(string receiverId, string content)
        {
            if (string.IsNullOrWhiteSpace(content))
            {
                return;
            }

            SaveMessageViewModel vm = new()
            {
                ReceiverId = receiverId,
                Content = content
            };

            var savedMessage = await _messageService.SendMessage(vm);

            await Clients.User(receiverId).SendAsync("ReceiveMessage", new
            {
                senderId = savedMessage.SenderId,
                receiverId = savedMessage.ReceiverId,
                content = savedMessage.Content,
                created = DateTime.Now.ToString("dd/MM/yyyy HH:mm")
            });

            await Clients.Caller.SendAsync("ReceiveMessage", new
            {
                senderId = savedMessage.SenderId,
                receiverId = savedMessage.ReceiverId,
                content = savedMessage.Content,
                created = DateTime.Now.ToString("dd/MM/yyyy HH:mm")
            });
        }
    }
}