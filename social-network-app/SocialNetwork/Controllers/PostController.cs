using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SocialNetwork.Core.Application.Helpers;
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
            UploadFilesHelper.DeleteFile(id, "Posts");

            return RedirectToRoute(new { controller = "Home", action = "Index" });
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
        public async Task<IActionResult> AddComment(SaveCommentViewModel vm)
        {
            if (!_validateUserSession.HasUser())
            {
                return RedirectToRoute(new { controller = "User", action = "Login" });
            }

            if (ModelState.IsValid)
            {
                return View(vm);
            }

            SaveCommentViewModel commentVm = await _commentService.Add(vm);

            return RedirectToRoute(new { controller = "Friend", action = "Index" });
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
