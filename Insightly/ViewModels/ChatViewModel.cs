namespace Insightly.ViewModels
{
    public class ChatViewModel
    {
        public int Id { get; set; }
        public string UserId { get; set; } = string.Empty;
        public string OtherUserId { get; set; } = string.Empty;
        public string UserName { get; set; } = string.Empty;
        public string? UserProfilePicture { get; set; }
        public string OtherUserName { get; set; } = string.Empty;
        public string? OtherUserProfilePicture { get; set; }
        public UserListItemViewModel User { get; set; } = null!;
        public UserListItemViewModel OtherUser { get; set; } = null!;
        public List<MessageViewModel> Messages { get; set; } = new List<MessageViewModel>();
    }
}

