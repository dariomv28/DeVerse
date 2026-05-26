using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SocialNetwork.Core.Application.Interfaces.Services;
using SocialNetwork.Core.Application.ViewModels.Comment;
using SocialNetwork.Core.Application.ViewModels.Post;
using SocialNetwork.Middlewares;

namespace SocialNetwork.Controllers
{
    [Authorize]
    public class PostController : Controller
    {
        private readonly IPostService _postService;
        private readonly ICommentService _commentService;
        private readonly ValidateUserSession _validateUserSession;

        public PostController(IPostService postService, ICommentService commentService, ValidateUserSession validateUserSession)
        {
            _postService = postService;
            _commentService = commentService;
            _validateUserSession = validateUserSession;
        }

        [HttpPost]
        public async Task<IActionResult> CreatePost(SavePostViewModel vm)
        {
            if (!_validateUserSession.HasUser())
            {
                return RedirectToRoute(new { controller = "User", action = "Login" });
            }

            if (!ModelState.IsValid)
            {
                return View("Index", vm);
            }

            SavePostViewModel postVm = await _postService.Add(vm);

            if (postVm.Id != 0 && postVm != null)
            {
                if (vm.File != null)
                {
                    postVm.Attachment = UploadFile(vm.File, postVm.Id);
                }

                await _postService.Update(postVm, postVm.Id);
            }

            return RedirectToRoute(new { controller = "Home", action = "Index" });
        }

        public async Task<IActionResult> Edit(int id)
        {
            if (!_validateUserSession.HasUser())
            {
                return RedirectToRoute(new { controller = "User", action = "Login" });
            }

            var post = await _postService.GetByIdSaveViewModel(id);
            return RedirectToAction("Index", "Home", new { id = post.Id });
        }

        [HttpPost]
        public async Task<IActionResult> Edit(SavePostViewModel vm)
        {
            if (!_validateUserSession.HasUser())
            {
                return RedirectToRoute(new { controller = "User", action = "Login" });
            }

            if (!ModelState.IsValid)
            {
                return View();
            }

            SavePostViewModel postVm = await _postService.GetByIdSaveViewModel(vm.Id);
            vm.Attachment = UploadFile(vm.File, vm.Id, true, postVm.Attachment);
            vm.SharedPostId = postVm.SharedPostId;

            await _postService.Update(vm, vm.Id);
            return RedirectToRoute(new { controller = "Home", action = "Index" });
        }

        public async Task<IActionResult> Delete(int id)
        {
            if (!_validateUserSession.HasUser())
            {
                return RedirectToRoute(new { controller = "User", action = "Login" });
            }

            var post = await _postService.GetByIdSaveViewModel(id);
            return View("Delete", post);
        }

        [HttpPost]
        public async Task<IActionResult> DeletePost(int id)
        {
            if (!_validateUserSession.HasUser())
            {
                return RedirectToRoute(new { controller = "User", action = "Login" });
            }

            await _postService.Delete(id);

            return RedirectToRoute(new { controller = "Home", action = "Index" });
        }

        [HttpPost]
        public async Task<IActionResult> ToggleLike(int postId)
        {
            if (!_validateUserSession.HasUser())
            {
                return Unauthorized(new { success = false, message = "User is not authenticated." });
            }

            if (postId <= 0)
            {
                return BadRequest(new { success = false, message = "Invalid post id." });
            }

            try
            {
                var result = await _postService.ToggleLike(postId);

                return Json(new
                {
                    success = true,
                    isLiked = result.IsLiked,
                    likeCount = result.LikeCount
                });
            }
            catch (InvalidOperationException)
            {
                return NotFound(new { success = false, message = "Post not found." });
            }
        }

        [HttpPost]
        public async Task<IActionResult> Share(int postId, string? content, string? returnUrl)
        {
            if (!_validateUserSession.HasUser())
            {
                return RedirectToRoute(new { controller = "User", action = "Login" });
            }

            if (postId <= 0)
            {
                return RedirectToSafeReturnUrl(returnUrl);
            }

            try
            {
                SavePostViewModel sharedPost = await _postService.SharePost(postId, content);

                if (sharedPost.Id != 0 && !string.IsNullOrEmpty(sharedPost.Attachment))
                {
                    sharedPost.Attachment = CopySharedAttachment(sharedPost.Attachment, sharedPost.Id);
                    await _postService.Update(sharedPost, sharedPost.Id);
                }
            }
            catch (InvalidOperationException)
            {
                return RedirectToSafeReturnUrl(returnUrl);
            }

            return RedirectToSafeReturnUrl(returnUrl);
        }

        public async Task<IActionResult> AddComment(int postId)
        {
            if (!_validateUserSession.HasUser())
            {
                return RedirectToRoute(new { controller = "User", action = "Login" });
            }

            ViewBag.Post = await _postService.GetByIdViewModelWithInclude(postId);
            return View("AddComment");
        }

        [HttpPost]
        public async Task<IActionResult> AddComment(int postId, string? content, string? returnUrl)
        {
            if (!_validateUserSession.HasUser())
            {
                return RedirectToRoute(new { controller = "User", action = "Login" });
            }

            if (postId <= 0 || string.IsNullOrWhiteSpace(content))
            {
                return RedirectToSafeReturnUrl(returnUrl);
            }

            SaveCommentViewModel vm = new()
            {
                PostId = postId,
                Content = content.Trim()
            };

            await _commentService.Add(vm);

            return RedirectToSafeReturnUrl(returnUrl);
        }

        private IActionResult RedirectToSafeReturnUrl(string? returnUrl)
        {
            if (!string.IsNullOrWhiteSpace(returnUrl) && Url.IsLocalUrl(returnUrl))
            {
                return LocalRedirect(returnUrl);
            }

            return RedirectToRoute(new { controller = "Home", action = "Index" });
        }

        private static string CopySharedAttachment(string sourceImagePath, int id)
        {
            string relativeSourcePath = sourceImagePath.TrimStart('/', '\\').Replace('/', Path.DirectorySeparatorChar);
            string sourcePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", relativeSourcePath);

            if (!System.IO.File.Exists(sourcePath))
            {
                return sourceImagePath;
            }

            string basePath = $"/Images/Posts/{id}";
            string path = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "Images", "Posts", id.ToString());

            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }

            string filename = $"{Guid.NewGuid()}{Path.GetExtension(sourcePath)}";
            string fileNameWithPath = Path.Combine(path, filename);
            System.IO.File.Copy(sourcePath, fileNameWithPath, true);

            return $"{basePath}/{filename}";
        }

        public static string UploadFile(IFormFile file, int id, bool isEditMode = false, string imagePath = "")
        {
            if (isEditMode && file == null)
            {
                return imagePath;
            }

            //get directory path
            string basePath = $"/Images/Posts/{id}";
            string path = Path.Combine(Directory.GetCurrentDirectory(), $"wwwroot{basePath}");

            //create folder if no exist
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }

            //get file path
            Guid guid = Guid.NewGuid();
            FileInfo fileInfo = new(file.FileName);
            string filename = guid + fileInfo.Extension;

            string fileNameWithPath = Path.Combine(path, filename);

            using (var stream = new FileStream(fileNameWithPath, FileMode.Create))
            {
                file.CopyTo(stream);
            }

            if (isEditMode)
            {
                string[] oldImagePart = imagePath.Split("/");
                string oldImageName = oldImagePart[^1];
                string completeImageOldPath = Path.Combine(path, oldImageName);

                if (System.IO.File.Exists(completeImageOldPath))
                {
                    System.IO.File.Delete(completeImageOldPath);
                }
            }

            return $"{basePath}/{filename}";
        }
    }
}
