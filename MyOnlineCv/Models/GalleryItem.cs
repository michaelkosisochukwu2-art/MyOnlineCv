using System.ComponentModel.DataAnnotations;

namespace MyOnlineCv.Models
{
    public class GalleryItem
    {
        [Key]
        public int Id { get; set; }
        public string Title { get; set; }
        public string ImageUrl { get; set; }
        public DateTime UploadedAt { get; set; } = DateTime.UtcNow;
    }
}