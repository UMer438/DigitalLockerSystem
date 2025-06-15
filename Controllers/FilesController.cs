using DigitalLockerSystem.Models;
using DigitalLockerSystem.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Hosting;
using System.IO;
using System.Security.Claims;
using System.Threading.Tasks;
using System;

namespace DigitalLockerSystem.Controllers
{
    [Authorize]
    public class FilesController : Controller
    {
        private readonly IFileRepository _fileRepository;
        private readonly IWebHostEnvironment _environment;

        public FilesController(IFileRepository fileRepository, IWebHostEnvironment environment)
        {
            _fileRepository = fileRepository;
            _environment = environment;
        }

        public async Task<IActionResult> MyFiles(string search)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var files = await _fileRepository.GetFilesByUserIdAsync(userId);

            if (!string.IsNullOrWhiteSpace(search))
            {
                files = files.Where(f => f.OriginalFileName.Contains(search, StringComparison.OrdinalIgnoreCase)).ToList();
            }

            ViewBag.SearchQuery = search;
            return View(files);
        }

        [HttpGet]
        public async Task<IActionResult> Download(int id)
        {
            var file = await _fileRepository.GetFileByIdAsync(id);
            if (file == null)
                return NotFound();

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (file.UserId != userId)
                return Forbid();

            var filePath = Path.Combine(_environment.WebRootPath, "uploads", file.StoredFileName);
            if (!System.IO.File.Exists(filePath))
                return NotFound();

            var memory = new MemoryStream();
            using (var stream = new FileStream(filePath, FileMode.Open))
            {
                await stream.CopyToAsync(memory);
            }
            memory.Position = 0;

            return File(memory, file.ContentType, file.OriginalFileName);
        }

        [HttpGet]
        public IActionResult Upload()
        {
            return View();
        }

       /* [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Upload(Microsoft.AspNetCore.Http.IFormFile file)
        {
            if (file == null || file.Length == 0)
            {
                ModelState.AddModelError("", "Please select a file.");
                return View();
            }

            if (file.Length > 25L * 1024 * 1024 * 1024)
            {
                ModelState.AddModelError("", "File size cannot exceed 5 GB.");
                return View();
            }

            var ext = Path.GetExtension(file.FileName).ToLowerInvariant();

            if (string.IsNullOrEmpty(ext))
            {
                ModelState.AddModelError("", "File must have a valid extension.");
                return View();
            }

            var storedFileName = Guid.NewGuid().ToString() + ext;
            var uploadPath = Path.Combine(_environment.WebRootPath, "uploads");

            if (!Directory.Exists(uploadPath))
                Directory.CreateDirectory(uploadPath);

            var filePath = Path.Combine(uploadPath, storedFileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var fileRecord = new Models.File
            {
                UserId = userId,
                OriginalFileName = file.FileName,
                StoredFileName = storedFileName,
                ContentType = file.ContentType,
                UploadDate = DateTime.UtcNow
            };

            await _fileRepository.AddFileAsync(fileRecord);

            TempData["Message"] = "File uploaded successfully!";
            return RedirectToAction(nameof(MyFiles));
        }*/
       [HttpPost]
       [ValidateAntiForgeryToken]
       public async Task<IActionResult> Upload(List<Microsoft.AspNetCore.Http.IFormFile> files)
       {
           if (files == null || files.Count == 0)
           {
               ModelState.AddModelError("", "Please select at least one file.");
               return View();
           }

           var uploadPath = Path.Combine(_environment.WebRootPath, "uploads");
           if (!Directory.Exists(uploadPath))
               Directory.CreateDirectory(uploadPath);

           var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

           foreach (var file in files)
           {
               if (file == null || file.Length == 0)
                   continue;

               if (file.Length > 25L * 1024 * 1024 * 1024)
               {
                   ModelState.AddModelError("", $"File {file.FileName} exceeds the 5 GB limit.");
                   continue;
               }

               var ext = Path.GetExtension(file.FileName).ToLowerInvariant();
               if (string.IsNullOrEmpty(ext))
               {
                   ModelState.AddModelError("", $"File {file.FileName} has an invalid extension.");
                   continue;
               }

               var storedFileName = Guid.NewGuid().ToString() + ext;
               var filePath = Path.Combine(uploadPath, storedFileName);

               using (var stream = new FileStream(filePath, FileMode.Create))
               {
                   await file.CopyToAsync(stream);
               }

               var fileRecord = new Models.File
               {
                   UserId = userId,
                   OriginalFileName = file.FileName,
                   StoredFileName = storedFileName,
                   ContentType = file.ContentType,
                   UploadDate = DateTime.UtcNow
               };

               await _fileRepository.AddFileAsync(fileRecord);
           }

           TempData["Message"] = "Files uploaded successfully!";
           return RedirectToAction(nameof(MyFiles));
       }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var file = await _fileRepository.GetFileByIdAsync(id);
            if (file == null)
                return NotFound();

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (file.UserId != userId)
                return Forbid();

            var filePath = Path.Combine(_environment.WebRootPath, "uploads", file.StoredFileName);
            if (System.IO.File.Exists(filePath))
            {
                System.IO.File.Delete(filePath);
            }

            await _fileRepository.DeleteFileAsync(file);

            TempData["Message"] = "File deleted successfully.";
            return RedirectToAction(nameof(MyFiles));
        }
    }
}
