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
    public class MessageService :
        GenericService<SaveMessageViewModel, MessageViewModel, Message>,
        IMessageService
    {
        private readonly IMessageRepository _messageRepository;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public MessageService(
            IMessageRepository messageRepository,
            IHttpContextAccessor httpContextAccessor,
            IMapper mapper
        ) : base(messageRepository, mapper)
        {
            _messageRepository = messageRepository;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task<SaveMessageViewModel> SendMessage(
            SaveMessageViewModel vm
        )
        {
            var user =
                _httpContextAccessor
                .HttpContext?
                .Session
                .Get<AuthenticationResponse>("user");

            if (user == null)
            {
                vm.HasError = true;
                vm.Error = "User session expired";
                return vm;
            }

            vm.SenderId = user.Id;

            if (string.IsNullOrWhiteSpace(vm.Content))
            {
                vm.HasError = true;
                vm.Error = "Message cannot be empty";
                return vm;
            }

            return await base.Add(vm);
        }

        public async Task<List<MessageViewModel>>
            GetConversationWithUser(string friendId)
        {
            var user =
                _httpContextAccessor
                .HttpContext?
                .Session
                .Get<AuthenticationResponse>("user");

            if (user == null)
            {
                return new List<MessageViewModel>();
            }

            var messages =
                await _messageRepository.GetAllAsync();

            return messages
                .Where(m =>
                    (m.SenderId == user.Id &&
                     m.ReceiverId == friendId)
                    ||
                    (m.SenderId == friendId &&
                     m.ReceiverId == user.Id)
                )
                .OrderBy(m => m.Created)
                .Select(m => new MessageViewModel
                {
                    Id = m.Id,
                    SenderId = m.SenderId,
                    ReceiverId = m.ReceiverId,
                    Content = m.Content,
                    Created = m.Created ?? DateTime.Now,
                    IsMine = m.SenderId == user.Id
                })
                .ToList();
        }
    }
}