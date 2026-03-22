namespace Insightly.ViewModels
{
    /// <summary>Outcome of resolving /profile/{id} for a viewer.</summary>
    public sealed class PublicProfileRequestResult
    {
        public bool UserNotFound { get; init; }
        /// <summary>Viewer opened their own id on the public URL; redirect to /profile/me.</summary>
        public bool ViewerIsOwner { get; init; }
        public PublicProfilePageViewModel? Page { get; init; }
    }
}
