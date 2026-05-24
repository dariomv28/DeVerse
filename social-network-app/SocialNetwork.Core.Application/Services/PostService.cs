using AutoMapper;
using Microsoft.AspNetCore.Http;
using SocialNetwork.Core.Application.Dtos.Account;
using SocialNetwork.Core.Application.Helpers;
using SocialNetwork.Core.Application.Interfaces.Repositories;
using SocialNetwork.Core.Application.Interfaces.Services;
using SocialNetwork.Core.Application.ViewModels.Comment;
using SocialNetwork.Core.Application.ViewModels.Post;
using SocialNetwork.Core.Application.ViewModels.User;
using SocialNetwork.Core.Domain.Entities;

namespace SocialNetwork.Core.Application.Services
{
    public class PostService : GenericService<SavePostViewModel, PostViewModel, Post>, IPostService
    {
        private readonly IPostRepository _postRepository;
        private readonly IUserService _userService;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly AuthenticationResponse userViewModel;
        private readonly IMapper _mapper;

        public PostService(IPostRepository postRepository,  IUserService userService,IHttpContextAccessor httpContextAccessor, IMapper mapper) : base(postRepository, mapper)
        {
            _postRepository = postRepository;
            _userService = userService;
            _httpContextAccessor = httpContextAccessor;
            _mapper = mapper;
            userViewModel = _httpContextAccessor.HttpContext.Session.Get<AuthenticationResponse>("user");
        }

        public override async Task<SavePostViewModel> Add(SavePostViewModel vm)
        {
            vm.UserId = userViewModel.Id;
            return await base.Add(vm);
        }

        public override async Task Update(SavePostViewModel vm, int id)
        {
            vm.UserId = userViewModel.Id;
            await base.Update(vm, id);
        }

        public async Task<List<PostViewModel>> GetAllViewModelWithInclude()
        {
            var list = await _postRepository.GetAllWithIncludeAsync(new List<string> { "Comments" });

            var filteredList = new List<PostViewModel>();

            foreach (var p in list.Where(p => p.UserId == userViewModel.Id).OrderByDescending(p => p.Created))
            {
                var postViewModel = new PostViewModel
                {
                    Id = p.Id,
                    UserId = p.UserId,
                    UserName = userViewModel.Username,
                    UserProfilePicture = userViewModel.ProfilePicture,
                    Content = p.Content,
                    Attachment = p.Attachment,
                    Created = p.Created,
                    Comments = new List<CommentViewModel>()
                };

                foreach (var c in p.Comments)
                {
                    var user = await _userService.GetByIdAsync(c.UserId);
                    var commentViewModel = new CommentViewModel
                    {
                        Id = c.Id,
                        UserId = c.UserId,
                        UserName = user.Username,
                        UserProfilePicture = user.ProfilePicture,
                        Content = c.Content,
                        Created = c.Created
                    };
                    postViewModel.Comments.Add(commentViewModel);
                }

                filteredList.Add(postViewModel);
            }

            return filteredList;
        }


        public async Task<PostViewModel> GetByIdViewModelWithInclude(int id)
        {
            var list = await _postRepository.GetAllWithIncludeAsync(new List<string> { "Comments" });
            var post = list.Where(p => p.Id == id).FirstOrDefault();

            SaveUserViewModel user = await _userService.GetByIdAsync(post.UserId);
            var postVm = new PostViewModel
            {
                Id = post.Id,
                UserId = post.UserId,
                UserName = user.Username,
                UserProfilePicture = user.ProfilePicture,
                Content = post.Content,
                Attachment = post.Attachment,
                Created = post.Created,
                Comments = post.Comments.Select(c => new CommentViewModel
                {
                    Id = c.Id,
                    UserId = c.UserId,
                    UserName = _userService.GetByIdAsync(c.UserId).Result.Username,
                    UserProfilePicture = _userService.GetByIdAsync(c.UserId).Result.ProfilePicture,
                    Content = c.Content,
                    Created = c.Created
                }).ToList()
            };

            return postVm;
        }

        public async Task<List<PostViewModel>> GetPostsByUserViewModelWithIncludes(string userId)
        {
            var posts = await _postRepository.GetAllWithIncludeAsync(new List<string> { "Comments" });

            SaveUserViewModel user = await _userService.GetByIdAsync(userId);

            var filteredList = posts
                .Where(p => p.UserId == userId)
                .OrderByDescending(p => p.Created)
                .Select(p => new PostViewModel
                {
                    Id = p.Id,
                    UserId = p.UserId,
                    UserName = user.Username,
                    UserProfilePicture = user.ProfilePicture,
                    Content = p.Content,
                    Attachment = p.Attachment,
                    Created = p.Created,
                    Comments = p.Comments.Select(c => new CommentViewModel
                    {
                        Id = c.Id,
                        UserId = c.UserId,
                        UserName = _userService.GetByIdAsync(c.UserId).Result.Username,
                        UserProfilePicture = _userService.GetByIdAsync(c.UserId).Result.ProfilePicture,
                        Content = c.Content,
                        Created = c.Created
                    }).ToList()
                })
                .ToList();

            return filteredList;
        }

    }
}
