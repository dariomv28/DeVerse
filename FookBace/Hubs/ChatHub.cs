using Microsoft.AspNetCore.SignalR;
using SocialNetwork.Core.Application.Dtos.Account;
using SocialNetwork.Core.Application.Helpers;
using SocialNetwork.Core.Application.Interfaces.Services;
using SocialNetwork.Core.Application.ViewModels.Message;

namespace SocialNetwork.Hubs
{
    public class ChatHub : Hub
    {
        private readonly IMessageService _messageService;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public ChatHub(
            IMessageService messageService,
            IHttpContextAccessor httpContextAccessor
        )
        {
            _messageService = messageService;
            _httpContextAccessor = httpContextAccessor;
        }

        public override async Task OnConnectedAsync()
        {
            var user = _httpContextAccessor.HttpContext.Session.Get<AuthenticationResponse>("user");

            if (user != null)
            {
                await Groups.AddToGroupAsync(Context.ConnectionId, user.Id);
            }

            await base.OnConnectedAsync();
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

            var messageData = new
            {
                senderId = savedMessage.SenderId,
                receiverId = savedMessage.ReceiverId,
                content = savedMessage.Content,
                created = DateTime.Now
            };

            await Clients.Group(receiverId).SendAsync("ReceiveMessage", messageData);

            await Clients.Caller.SendAsync("ReceiveMessage", messageData);
        }
    }
}