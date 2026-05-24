using AutoMapper;
using Microsoft.AspNetCore.Http;
using SocialNetwork.Core.Application.Dtos.Account;
using SocialNetwork.Core.Application.Helpers;
using SocialNetwork.Core.Application.Interfaces.Repositories;
using SocialNetwork.Core.Application.Interfaces.Services;
using SocialNetwork.Core.Application.ViewModels.Message;
using SocialNetwork.Core.Domain.Entities;

namespace SocialNetwork.Core.Application.Services
{
    public class MessageService : GenericService<SaveMessageViewModel, MessageViewModel, Message>, IMessageService
    {
        private readonly IMessageRepository _messageRepository;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly AuthenticationResponse userViewModel;

        public MessageService(
            IMessageRepository messageRepository,
            IHttpContextAccessor httpContextAccessor,
            IMapper mapper
        ) : base(messageRepository, mapper)
        {
            _messageRepository = messageRepository;
            _httpContextAccessor = httpContextAccessor;
            userViewModel = _httpContextAccessor.HttpContext.Session.Get<AuthenticationResponse>("user");
        }

        public async Task<SaveMessageViewModel> SendMessage(SaveMessageViewModel vm)
        {
            vm.SenderId = userViewModel.Id;

            if (string.IsNullOrWhiteSpace(vm.Content))
            {
                vm.HasError = true;
                vm.Error = "Message cannot be empty";
                return vm;
            }

            return await base.Add(vm);
        }

        public async Task<List<MessageViewModel>> GetConversationWithUser(string friendId)
        {
            var messages = await _messageRepository.GetAllAsync();

            return messages
                .Where(m =>
                    (m.SenderId == userViewModel.Id && m.ReceiverId == friendId) ||
                    (m.SenderId == friendId && m.ReceiverId == userViewModel.Id)
                )
                .OrderBy(m => m.Created)
                .Select(m => new MessageViewModel
                {
                    Id = m.Id,
                    SenderId = m.SenderId,
                    ReceiverId = m.ReceiverId,
                    Content = m.Content,
                    Created = m.Created ?? DateTime.Now,
                    IsMine = m.SenderId == userViewModel.Id
                })
                .ToList();
        }
    }
}