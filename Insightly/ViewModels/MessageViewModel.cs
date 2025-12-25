namespace Insightly.ViewModels
{
    public class MessageViewModel
    {
        public int Id { get; set; }
        public string SenderId { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public DateTime SentAt { get; set; }
        public bool IsRead { get; set; }
        public int ChatId { get; set; }
    }
}
