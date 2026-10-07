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

            var webRoot = GetWebRootPath();

            var uploadsFolder = Path.Combine(webRoot, "gallery_uploads");

            // If a file unexpectedly exists with this name, delete it
            if (System.IO.File.Exists(uploadsFolder))
            {
                System.IO.File.Delete(uploadsFolder);
            }

            // Safely create the uploads directory inside wwwroot
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
                var webRoot = GetWebRootPath();
                var filePath = Path.Combine(webRoot, item.ImageUrl.TrimStart('/'));
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
        /// Resolves the static web root path.
        /// </summary>
        private string GetWebRootPath()
        {
            var webRoot = _environment.WebRootPath;

            if (string.IsNullOrEmpty(webRoot))
            {
                webRoot = Path.Combine(_environment.ContentRootPath, "wwwroot");
            }

            if (!Directory.Exists(webRoot))
            {
                Directory.CreateDirectory(webRoot);
            }

            return webRoot;
        }
    }
}