using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Insightly.Models
{
    public class User
    {
        public int Id { get; set; }
        
        [Required]
        [StringLength(100)]
        public string Name { get; set; } = string.Empty;
        
        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;
        
        [Required]
        public string Password { get; set; } = string.Empty;
        
        [Required]
        [StringLength(10)]
        public string Gender { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; } = DateTime.Now;
        
        public virtual List<Article> Articles { get; set; } = new List<Article>();
        public virtual List<ArticleRead> ReadArticles { get; set; } = new List<ArticleRead>();
    }
}
