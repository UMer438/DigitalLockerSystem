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

        public async Task<IActionResult> MyFiles()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var files = await _fileRepository.GetFilesByUserIdAsync(userId);
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

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Upload(Microsoft.AspNetCore.Http.IFormFile file)
        {
            if (file == null || file.Length == 0)
            {
                ModelState.AddModelError("", "Please select a file.");
                return View();
            }

           
            if (file.Length > 5L * 1024 * 1024 * 1024)
            {
                ModelState.AddModelError("", "File size cannot exceed 5 GB.");
                return View();
            }

            var permittedExtensions = new[] { ".pdf", ".jpg", ".jpeg", ".png", ".gif", ".mp4" };
            var ext = Path.GetExtension(file.FileName).ToLowerInvariant();

            if (string.IsNullOrEmpty(ext) || Array.IndexOf(permittedExtensions, ext) < 0)
            {
                ModelState.AddModelError("", "Unsupported file type.");
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
