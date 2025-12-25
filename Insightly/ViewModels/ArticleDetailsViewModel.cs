namespace Insightly.ViewModels
{
    public class ArticleDetailsViewModel
    {
        public int ArticleId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public string? ImagePath { get; set; }
        public string AuthorId { get; set; } = string.Empty;
        public string AuthorName { get; set; } = string.Empty;
        public string? AuthorProfilePicture { get; set; }
        public List<CommentViewModel> Comments { get; set; } = new List<CommentViewModel>();
    }
}

