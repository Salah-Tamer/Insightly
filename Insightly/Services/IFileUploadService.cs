namespace Insightly.Services
{
    public interface IFileUploadService
    {
        Task<(bool IsValid, string? ErrorMessage)> ValidateImageAsync(IFormFile? file, long maxBytes = 5 * 1024 * 1024);
        Task<string?> UploadArticleImageAsync(IFormFile? file);
    }
}

