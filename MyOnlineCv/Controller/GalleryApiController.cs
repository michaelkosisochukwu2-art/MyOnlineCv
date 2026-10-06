using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MyOnlineCv.Data;
using MyOnlineCv.Models;

namespace MyOnlineCv.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class GalleryApiController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _environment;

        public GalleryApiController(ApplicationDbContext context, IWebHostEnvironment environment)
        {
            _context = context;
            _environment = environment;
        }

        [HttpGet("items")]
        public async Task<IActionResult> GetGalleryItems()
        {
            var items = await _context.GalleryItems
                .OrderByDescending(g => g.UploadedAt)
                .ToListAsync();

            return Ok(items);
        }

        [HttpPost("upload")]
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> UploadImage([FromForm] ImageUploadRequest request)
        {
            if (request?.File == null || request.File.Length == 0)
                return BadRequest("No file provided.");

            var uploadsFolder = Path.Combine(_environment.WebRootPath, "uploads");
            if (!Directory.Exists(uploadsFolder))
            {
                Directory.CreateDirectory(uploadsFolder);
            }

            var uniqueFileName = $"{Guid.NewGuid()}_{Path.GetFileName(request.File.FileName)}";
            var filePath = Path.Combine(uploadsFolder, uniqueFileName);

            using (var fileStream = new FileStream(filePath, FileMode.Create))
            {
                await request.File.CopyToAsync(fileStream);
            }

            var imageUrl = $"{Request.Scheme}://{Request.Host}/uploads/{uniqueFileName}";
            var galleryItem = new GalleryItem
            {
                Title = string.IsNullOrWhiteSpace(request.Title) ? request.File.FileName : request.Title,
                ImageUrl = imageUrl,
                UploadedAt = DateTime.UtcNow
            };

            _context.GalleryItems.Add(galleryItem);
            await _context.SaveChangesAsync();

            return Ok(galleryItem);
        }
    }
}