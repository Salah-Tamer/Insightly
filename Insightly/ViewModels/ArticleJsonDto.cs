namespace Insightly.ViewModels
{
    public class ArticleJsonDto
    {
        public int ArticleId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public AuthorJsonDto Author { get; set; } = null!;
    }

    public class AuthorJsonDto
    {
        public string Name { get; set; } = string.Empty;
        public string Id { get; set; } = string.Empty;
    }
}

