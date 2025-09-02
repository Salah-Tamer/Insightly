using System.ComponentModel.DataAnnotations;

namespace Insightly.Models
{
    public class Article
    {
        public int ArticleId { get; set; }

        [Required]
        [StringLength(100)]
        public string Title { get; set; } = string.Empty;
        [Required]
        public string Content { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public DateTime UpdatedAt { get; set; } = DateTime.Now;
        public int AuthorId { get; set; }
        public List<Comment> Comments { get; set; } = new List<Comment>();
        public Article() { }
        /*public List<string> Tags { get; set; }
        public bool IsPublished { get; set; }
        public int ViewCount { get; set; }
        public int Likes { get; set; }
        */

    }
}
