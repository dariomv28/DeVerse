using AutoMapper;
using Microsoft.AspNetCore.Http;
using SocialNetwork.Core.Application.Dtos.Account;
using SocialNetwork.Core.Application.Helpers;
using SocialNetwork.Core.Application.Interfaces.Repositories;
using SocialNetwork.Core.Application.Interfaces.Services;
using SocialNetwork.Core.Application.ViewModels.Comment;
using SocialNetwork.Core.Domain.Entities;

namespace SocialNetwork.Core.Application.Services
{
    public class CommentService : GenericService<SaveCommentViewModel, CommentViewModel, Comment>, ICommentService
    {
        private readonly ICommentRepository _commentRepository;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly AuthenticationResponse userViewModel;
        private readonly IMapper _mapper;

        public CommentService(ICommentRepository commentRepository, IHttpContextAccessor httpContextAccessor, IMapper mapper) : base(commentRepository, mapper)
        {
            _commentRepository = commentRepository;
            _httpContextAccessor = httpContextAccessor;
            _mapper = mapper;
            userViewModel = _httpContextAccessor.HttpContext.Session.Get<AuthenticationResponse>("user");
        }

        public override async Task<SaveCommentViewModel> Add(SaveCommentViewModel vm)
        {
            vm.UserId = userViewModel.Id;
            return await base.Add(vm);
        }

        public override async Task Update(SaveCommentViewModel vm, int id)
        {
            vm.UserId = userViewModel.Id;
            await base.Update(vm, id);
        }
    }
}
