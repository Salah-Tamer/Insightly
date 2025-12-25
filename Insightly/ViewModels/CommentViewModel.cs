namespace Insightly.ViewModels
{
    public class CommentViewModel
    {
        public int CommentId { get; set; }
        public string Content { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public string AuthorId { get; set; } = string.Empty;
        public string AuthorName { get; set; } = string.Empty;
        public string? AuthorProfilePicture { get; set; }
        public int ArticleId { get; set; }
    }
}

