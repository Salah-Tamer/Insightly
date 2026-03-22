namespace Insightly.ViewModels
{
    /// <summary>Owner profile page: /profile/me</summary>
    public class MyProfilePageViewModel
    {
        public ProfileViewModel Profile { get; set; } = null!;
        public List<ArticleListItemViewModel> Articles { get; set; } = new();
        public int FollowersCount { get; set; }
        public int FollowingCount { get; set; }
    }
}
