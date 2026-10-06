using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace MyOnlineCv.Pages
{
    public class GalleryModel : PageModel
    {
        private readonly IWebHostEnvironment _environment;

        public GalleryModel(IWebHostEnvironment environment)
        {
            _environment = environment;
        }

        [BindProperty]
        public IFormFile ImageUpload { get; set; }

        public List<string> ImageUrls { get; set; } = new List<string>();

        public void OnGet()
        {
            LoadImages();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (ImageUpload != null && ImageUpload.Length > 0)
            {
                var uploadsFolder = Path.Combine(_environment.WebRootPath, "uploads");
                if (!Directory.Exists(uploadsFolder))
                {
                    Directory.CreateDirectory(uploadsFolder);
                }

                var uniqueFileName = Guid.NewGuid().ToString() + "_" + Path.GetFileName(ImageUpload.FileName);
                var filePath = Path.Combine(uploadsFolder, uniqueFileName);

                using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    await ImageUpload.CopyToAsync(fileStream);
                }
            }

            LoadImages();
            return Page();
        }

        private void LoadImages()
        {
            var uploadsFolder = Path.Combine(_environment.WebRootPath, "uploads");
            if (Directory.Exists(uploadsFolder))
            {
                var files = Directory.GetFiles(uploadsFolder);
                foreach (var file in files)
                {
                    ImageUrls.Add("/uploads/" + Path.GetFileName(file));
                }
            }
        }
    }
}