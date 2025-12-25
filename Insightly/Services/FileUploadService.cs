using Microsoft.AspNetCore.Http;
using System.Linq;

namespace Insightly.Services
{
    public class FileUploadService : IFileUploadService
    {
        public async Task<(bool IsValid, string? ErrorMessage)> ValidateImageAsync(IFormFile? file, long maxBytes = 5 * 1024 * 1024)
        {
            if (file == null || file.Length == 0)
            {
                return (true, null);
            }

            if (file.Length > maxBytes)
            {
                return (false, "Photo must be 5 MB or smaller.");
            }

            var permitted = new[] { "image/jpeg", "image/png", "image/gif" };
            if (!permitted.Contains(file.ContentType))
            {
                return (false, "Only JPG, PNG, or GIF images are allowed.");
            }

            return (true, null);
        }

        public async Task<string?> UploadArticleImageAsync(IFormFile? file)
        {
            if (file == null || file.Length == 0)
            {
                return null;
            }

            var uploadsRoot = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", "articles");
            if (!Directory.Exists(uploadsRoot))
            {
                Directory.CreateDirectory(uploadsRoot);
            }

            var fileExtension = Path.GetExtension(file.FileName);
            var safeFileName = $"article_{Guid.NewGuid():N}{fileExtension}";
            var filePath = Path.Combine(uploadsRoot, safeFileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            return $"/uploads/articles/{safeFileName}";
        }
    }
}

