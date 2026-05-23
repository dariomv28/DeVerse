using AutoMapper;
using Microsoft.AspNetCore.Http;
using SocialNetwork.Core.Application.Dtos.Account;
using SocialNetwork.Core.Application.Helpers;
using SocialNetwork.Core.Application.Interfaces.Repositories;
using SocialNetwork.Core.Application.Interfaces.Services;
using SocialNetwork.Core.Application.ViewModels.Friend;
using SocialNetwork.Core.Application.ViewModels.Post;
using SocialNetwork.Core.Domain.Entities;

namespace SocialNetwork.Core.Application.Services
{
    public class FriendService : GenericService<SaveFriendViewModel, FriendViewModel, Friend>, IFriendService
    {
        private readonly IFriendRepository _friendRepository;
        private readonly IUserService _userService;
        private readonly IPostService _postService;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly AuthenticationResponse userViewModel;
        private readonly IMapper _mapper;

        public FriendService(IFriendRepository friendRepository, IUserService userService, IPostService postService, IHttpContextAccessor httpContextAccessor, IMapper mapper) : base(friendRepository, mapper)
        {
            _friendRepository = friendRepository;
            _userService = userService;
            _postService = postService;
            _httpContextAccessor = httpContextAccessor;
            _mapper = mapper;
            userViewModel = _httpContextAccessor.HttpContext.Session.Get<AuthenticationResponse>("user");
        }

        public override async Task<SaveFriendViewModel> Add(SaveFriendViewModel vm)
        {
            vm.UserSenderId = userViewModel.Id;
            if (vm.UserSenderId == vm.UserReceptorId)
            {
                vm.HasError = true;
                vm.Error = "you can't add yourself to your friends list";
                return vm;
            }

            return await base.Add(vm);
        }

        public async Task<List<FriendViewModel>> GetAllFriendViewModel()
        {
            var list = await _friendRepository.GetAllAsync();

            List<FriendViewModel> friendViews = new();

            foreach (var f in list.Where(f => f.UserSenderId == userViewModel.Id))
            {
                var user = await _userService.GetByIdAsync(f.UserReceptorId);
                var friendViewModel = _mapper.Map<FriendViewModel>(user);
                friendViewModel.FriendshipId = f.Id;

                friendViews.Add(friendViewModel);
            }

            return friendViews;
        }

        public async Task<List<PostViewModel>> GetAllFriendPostsViewModel()
        {
            var friendList = await _friendRepository.GetAllAsync();

            var friendReceptorIds = friendList.Where(f => f.UserSenderId == userViewModel.Id).Select(f => f.UserReceptorId).ToList();

            var allFriendPosts = await Task.WhenAll(friendReceptorIds.Select(userId => _postService.GetPostsByUserViewModelWithIncludes(userId)));

            var postVmList = allFriendPosts.SelectMany(posts => posts)
                                .OrderByDescending(post => post.Created)
                                .ToList();

            return postVmList;
        }
    }
}
