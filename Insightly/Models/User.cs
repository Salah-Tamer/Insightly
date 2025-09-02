using System.ComponentModel.DataAnnotations;

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
        public string Password { get; set; }
        [Required]
        public string Gender { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public List<Article> Articles { get; set; } = new List<Article>();
        public List<Comment> Comments { get; set; } = new List<Comment>();
    }
}
