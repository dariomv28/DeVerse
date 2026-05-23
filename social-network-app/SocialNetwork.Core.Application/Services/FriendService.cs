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
    // Người gửi lời mời là user đang đăng nhập
    vm.UserSenderId = userViewModel.Id;

    // Mặc định khi gửi lời mời là đang chờ chấp nhận
    vm.Status = "Pending";

    // Không cho tự kết bạn với chính mình
    if (vm.UserSenderId == vm.UserReceptorId)
    {
        vm.HasError = true;
        vm.Error = "You can't add yourself to your friends list";
        return vm;
    }

    // Lấy toàn bộ danh sách friend/request
    var friends = await _friendRepository.GetAllAsync();

    // Kiểm tra đã gửi lời mời hoặc đã là bạn chưa
    var existingFriend = friends.FirstOrDefault(f =>
        (f.UserSenderId == vm.UserSenderId && f.UserReceptorId == vm.UserReceptorId) ||
        (f.UserSenderId == vm.UserReceptorId && f.UserReceptorId == vm.UserSenderId)
    );

    if (existingFriend != null)
    {
        vm.HasError = true;

        if (existingFriend.Status == "Pending")
        {
            vm.Error = "Friend request already sent";
        }
        else if (existingFriend.Status == "Accepted")
        {
            vm.Error = "You are already friends";
        }

        return vm;
    }

    return await base.Add(vm);
}
        public async Task<List<FriendViewModel>> GetAllFriendViewModel()
        {
            var list = await _friendRepository.GetAllAsync();

            List<FriendViewModel> friendViews = new();
// Thêm chỉ lấy danh sách accepted
            var acceptedFriends = list.Where(f =>
             f.Status == "Accepted" &&
            (f.UserSenderId == userViewModel.Id || f.UserReceptorId == userViewModel.Id)
             );
// //Code cũ

//             foreach (var f in list.Where(f => f.UserSenderId == userViewModel.Id))
//             {
//                 var user = await _userService.GetByIdAsync(f.UserReceptorId);
//                 var friendViewModel = _mapper.Map<FriendViewModel>(user);
//                 friendViewModel.FriendshipId = f.Id;

//                 friendViews.Add(friendViewModel);
//             }

//             return friendViews;
    foreach (var f in acceptedFriends)
    {
        string friendUserId;

        if (f.UserSenderId == userViewModel.Id)
        {
            friendUserId = f.UserReceptorId;
        }
        else
        {
            friendUserId = f.UserSenderId;
        }

        var user = await _userService.GetByIdAsync(friendUserId);

        var friendViewModel = _mapper.Map<FriendViewModel>(user);

        friendViewModel.FriendshipId = f.Id;

        friendViews.Add(friendViewModel);
    }

    return friendViews;
        }

        public async Task<List<PostViewModel>> GetAllFriendPostsViewModel()
        {
            var friendList = await _friendRepository.GetAllAsync();

            // var friendReceptorIds = friendList.Where(f => f.UserSenderId == userViewModel.Id).Select(f => f.UserReceptorId).ToList();
             var friendIds = friendList
        .Where(f =>
            f.Status == "Accepted" &&
            (f.UserSenderId == userViewModel.Id || f.UserReceptorId == userViewModel.Id)
        )
        .Select(f =>
            f.UserSenderId == userViewModel.Id
                ? f.UserReceptorId
                : f.UserSenderId
        )
        .ToList();
            var allFriendPosts = await Task.WhenAll(friendIds.Select(userId => _postService.GetPostsByUserViewModelWithIncludes(userId)));

            var postVmList = allFriendPosts.SelectMany(posts => posts)
                                .OrderByDescending(post => post.Created)
                                .ToList();

            return postVmList;
        }
public async Task AcceptFriendRequest(int friendshipId)
{
    // Lấy lời mời kết bạn theo Id
    var friendRequest = await _friendRepository.GetByIdAsync(friendshipId);

    // Nếu không tìm thấy thì dừng
    if (friendRequest == null)
    {
        return;
    }

    // Chỉ người nhận lời mời mới được chấp nhận
    if (friendRequest.UserReceptorId != userViewModel.Id)
    {
        return;
    }

    // Đổi trạng thái từ Pending sang Accepted
    friendRequest.Status = "Accepted";

    // Cập nhật vào database
    await _friendRepository.UpdateAsync(friendRequest, friendshipId);
}
public async Task<List<FriendViewModel>> GetPendingFriendRequests()
{
    var list = await _friendRepository.GetAllAsync();

    List<FriendViewModel> pendingRequests = new();

    var requests = list.Where(f =>
        f.UserReceptorId == userViewModel.Id &&
        f.Status == "Pending"
    );

    foreach (var request in requests)
    {
        var user = await _userService.GetByIdAsync(request.UserSenderId);

        var requestViewModel = _mapper.Map<FriendViewModel>(user);

        requestViewModel.FriendshipId = request.Id;

        pendingRequests.Add(requestViewModel);
    }

    return pendingRequests;
}

    }
    

    
}
