using Insightly.Models;

namespace Insightly.Repositories
{
    public interface IUnitOfWork : IDisposable
    {
        IArticleRepository Articles { get; }
        ICommentRepository Comments { get; }
        IVoteRepository Votes { get; }
        ICommentVoteRepository CommentVotes { get; }
        IFollowRepository Follows { get; }
        IArticleReadRepository ArticleReads { get; }
        IChatRepository Chats { get; }
        
        Task<int> SaveChangesAsync();
        Task BeginTransactionAsync();
        Task CommitTransactionAsync();
        Task RollbackTransactionAsync();
    }
}
