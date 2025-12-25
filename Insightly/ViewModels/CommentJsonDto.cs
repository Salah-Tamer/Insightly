namespace Insightly.ViewModels
{
    public class CommentJsonDto
    {
        public int Id { get; set; }
        public string Content { get; set; } = string.Empty;
        public string Author { get; set; } = string.Empty;
        public string AuthorId { get; set; } = string.Empty;
        public string? AuthorProfilePicture { get; set; }
        public string CreatedAt { get; set; } = string.Empty;
        public string? UpdatedAt { get; set; }
        public bool IsUpdated { get; set; }
    }
}

