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
        private static readonly List<string> PostIncludes = new() { "Comments", "Likes", "SharedPost" };

        private readonly IPostRepository _postRepository;
        private readonly IUserService _userService;
        private readonly AuthenticationResponse userViewModel;

        public PostService(IPostRepository postRepository, IUserService userService, IHttpContextAccessor httpContextAccessor, IMapper mapper) : base(postRepository, mapper)
        {
            _postRepository = postRepository;
            _userService = userService;
            userViewModel = httpContextAccessor.HttpContext.Session.Get<AuthenticationResponse>("user");
        }

        public override async Task<SavePostViewModel> Add(SavePostViewModel vm)
        {
            vm.UserId = userViewModel.Id;
            return await base.Add(vm);
        }

        public override async Task Update(SavePostViewModel vm, int id)
        {
            Post? post = await _postRepository.GetByIdAsync(id);

            if (post == null)
            {
                return;
            }

            post.UserId = userViewModel.Id;
            post.Content = vm.Content;
            post.Attachment = vm.Attachment;
            post.SharedPostId = vm.SharedPostId;

            await _postRepository.UpdateAsync(post, id);
        }

        public async Task<List<PostViewModel>> GetAllViewModelWithInclude()
        {
            var posts = await _postRepository.GetAllWithIncludeAsync(PostIncludes);
            var filteredList = new List<PostViewModel>();

            foreach (Post post in posts.Where(p => p.UserId == userViewModel.Id).OrderByDescending(p => p.Created))
            {
                filteredList.Add(await BuildPostViewModel(post));
            }

            return filteredList;
        }

        public async Task<PostViewModel> GetByIdViewModelWithInclude(int id)
        {
            var posts = await _postRepository.GetAllWithIncludeAsync(PostIncludes);
            Post? post = posts.FirstOrDefault(p => p.Id == id);

            if (post == null)
            {
                throw new InvalidOperationException("Post not found");
            }

            return await BuildPostViewModel(post);
        }

        public async Task<List<PostViewModel>> GetPostsByUserViewModelWithIncludes(string userId)
        {
            var posts = await _postRepository.GetAllWithIncludeAsync(PostIncludes);
            SaveUserViewModel user = await _userService.GetByIdAsync(userId);
            var filteredList = new List<PostViewModel>();

            foreach (Post post in posts.Where(p => p.UserId == userId).OrderByDescending(p => p.Created))
            {
                filteredList.Add(await BuildPostViewModel(post, user));
            }

            return filteredList;
        }

        public async Task<(bool IsLiked, int LikeCount)> ToggleLike(int postId)
        {
            return await _postRepository.ToggleLikeAsync(postId, userViewModel.Id);
        }

        public override async Task Delete(int id)
        {
            var posts = await _postRepository.GetAllAsync();

            foreach (Post sharedPost in posts.Where(p => p.SharedPostId == id))
            {
                sharedPost.SharedPostId = null;
                await _postRepository.UpdateAsync(sharedPost, sharedPost.Id);
            }

            await base.Delete(id);
        }

        public async Task<SavePostViewModel> SharePost(int postId, string? content)
        {
            var posts = await _postRepository.GetAllWithIncludeAsync(new List<string> { "SharedPost" });
            Post? sourcePost = posts.FirstOrDefault(p => p.Id == postId);

            if (sourcePost == null)
            {
                throw new InvalidOperationException("Post not found");
            }

            Post rootPost = await ResolveRootSharedPost(sourcePost);

            SavePostViewModel shareVm = new()
            {
                Content = content?.Trim() ?? string.Empty,
                Attachment = rootPost.Attachment,
                SharedPostId = rootPost.Id
            };

            return await Add(shareVm);
        }

        private async Task<PostViewModel> BuildPostViewModel(Post post, SaveUserViewModel? postUser = null)
        {
            postUser ??= await _userService.GetByIdAsync(post.UserId);

            var postViewModel = new PostViewModel
            {
                Id = post.Id,
                UserId = post.UserId,
                UserName = postUser.Username ?? string.Empty,
                UserProfilePicture = postUser.ProfilePicture ?? string.Empty,
                Content = post.Content,
                Attachment = post.Attachment,
                LikeCount = post.LikeCount,
                IsLikedByCurrentUser = post.Likes != null && post.Likes.Any(l => l.UserId == userViewModel.Id),
                Created = post.Created,
                SharedPostId = post.SharedPostId,
                Comments = new List<CommentViewModel>()
            };

            if (post.SharedPostId.HasValue)
            {
                Post? sharedPost = post.SharedPost ?? await _postRepository.GetByIdAsync(post.SharedPostId.Value);

                if (sharedPost != null)
                {
                    SaveUserViewModel sharedPostUser = await _userService.GetByIdAsync(sharedPost.UserId);

                    postViewModel.SharedPostUserId = sharedPost.UserId;
                    postViewModel.SharedPostUserName = sharedPostUser.Username;
                    postViewModel.SharedPostUserProfilePicture = sharedPostUser.ProfilePicture;
                    postViewModel.SharedPostCreated = sharedPost.Created;
                }
            }

            foreach (Comment comment in post.Comments ?? Enumerable.Empty<Comment>())
            {
                SaveUserViewModel commentUser = await _userService.GetByIdAsync(comment.UserId);

                postViewModel.Comments.Add(new CommentViewModel
                {
                    Id = comment.Id,
                    UserId = comment.UserId,
                    UserName = commentUser.Username,
                    UserProfilePicture = commentUser.ProfilePicture,
                    Content = comment.Content,
                    Created = comment.Created
                });
            }

            return postViewModel;
        }

        private async Task<Post> ResolveRootSharedPost(Post post)
        {
            Post rootPost = post;

            while (rootPost.SharedPostId.HasValue)
            {
                Post? parentPost = rootPost.SharedPost ?? await _postRepository.GetByIdAsync(rootPost.SharedPostId.Value);

                if (parentPost == null)
                {
                    throw new InvalidOperationException("Shared post not found");
                }

                rootPost = parentPost;
            }

            return rootPost;
        }
    }
}
