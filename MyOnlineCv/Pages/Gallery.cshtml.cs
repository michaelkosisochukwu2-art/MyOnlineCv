using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using MyOnlineCv.Data;
using MyOnlineCv.Models;

namespace MyOnlineCv.Pages
{
    public class GalleryModel : PageModel
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _environment;

        public GalleryModel(ApplicationDbContext context, IWebHostEnvironment environment)
        {
            _context = context;
            _environment = environment;
        }

        public List<GalleryItem> GalleryItems { get; set; } = new();

        [BindProperty]
        public string Title { get; set; } = string.Empty;

        [BindProperty]
        public IFormFile? UploadedImage { get; set; }

        public async Task OnGetAsync()
        {
            GalleryItems = await _context.GalleryItems
                .OrderByDescending(g => g.UploadedAt)
                .ToListAsync();
        }

        public async Task<IActionResult> OnPostUploadAsync()
        {
            if (UploadedImage == null || UploadedImage.Length == 0)
            {
                ModelState.AddModelError(string.Empty, "Please select an image file to upload.");
                await OnGetAsync();
                return Page();
            }

            var uploadsFolder = GetUploadsFolderPath();

            // Ensure the persistent uploads directory exists
            if (!Directory.Exists(uploadsFolder))
            {
                Directory.CreateDirectory(uploadsFolder);
            }

            // Generate a unique filename
            var fileName = Guid.NewGuid().ToString() + Path.GetExtension(UploadedImage.FileName);
            var filePath = Path.Combine(uploadsFolder, fileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await UploadedImage.CopyToAsync(stream);
            }

            var item = new GalleryItem
            {
                Title = string.IsNullOrWhiteSpace(Title) ? "Untitled" : Title,
                ImageUrl = $"/gallery_uploads/{fileName}",
                UploadedAt = DateTime.UtcNow
            };

            _context.GalleryItems.Add(item);
            await _context.SaveChangesAsync();

            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostDeleteAsync(int id)
        {
            var item = await _context.GalleryItems.FindAsync(id);
            if (item != null)
            {
                var uploadsFolder = GetUploadsFolderPath();
                var fileName = Path.GetFileName(item.ImageUrl);
                var filePath = Path.Combine(uploadsFolder, fileName);

                if (System.IO.File.Exists(filePath))
                {
                    System.IO.File.Delete(filePath);
                }

                _context.GalleryItems.Remove(item);
                await _context.SaveChangesAsync();
            }

            return RedirectToPage();
        }

        /// <summary>
        /// Resolves the persistent uploads directory outside wwwroot (C:\home\site\gallery_uploads on Azure).
        /// This folder remains writable when WEBSITE_RUN_FROM_PACKAGE = 1 and survives deployments.
        /// </summary>
        private string GetUploadsFolderPath()
        {
            var uploadsPath = Path.GetFullPath(Path.Combine(_environment.ContentRootPath, "..", "gallery_uploads"));
            if (!Directory.Exists(uploadsPath))
            {
                Directory.CreateDirectory(uploadsPath);
            }
            return uploadsPath;
        }
    }
}