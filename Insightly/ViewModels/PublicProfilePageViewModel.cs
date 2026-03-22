namespace Insightly.ViewModels
{
    /// <summary>Read-only profile page: /profile/{id}</summary>
    public class PublicProfilePageViewModel
    {
        public ProfileViewModel Profile { get; set; } = null!;
        public List<ArticleListItemViewModel> Articles { get; set; } = new();
        public int FollowersCount { get; set; }
        public int FollowingCount { get; set; }
        public bool IsFollowing { get; set; }
        /// <summary>True when a signed-in viewer should see Follow/Unfollow (not viewing own profile).</summary>
        public bool ShowFollowButton { get; set; }
    }
}
