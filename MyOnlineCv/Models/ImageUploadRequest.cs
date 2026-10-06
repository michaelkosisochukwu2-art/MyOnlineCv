using Microsoft.AspNetCore.Http;

namespace MyOnlineCv.Models
{
    public class ImageUploadRequest
    {
        public string? Title { get; set; }
        public IFormFile File { get; set; }
    }
}