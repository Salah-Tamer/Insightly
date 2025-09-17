using Insightly.Models;
using Microsoft.EntityFrameworkCore.Storage;

namespace Insightly.Repositories
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly AppDbContext _context;
        private IDbContextTransaction? _transaction;

        public UnitOfWork(AppDbContext context)
        {
            _context = context;
            Articles = new ArticleRepository(_context);
            Comments = new CommentRepository(_context);
            Votes = new VoteRepository(_context);
            CommentVotes = new CommentVoteRepository(_context);
            Follows = new FollowRepository(_context);
            ArticleReads = new ArticleReadRepository(_context);
        }

        public IArticleRepository Articles { get; }
        public ICommentRepository Comments { get; }
        public IVoteRepository Votes { get; }
        public ICommentVoteRepository CommentVotes { get; }
        public IFollowRepository Follows { get; }
        public IArticleReadRepository ArticleReads { get; }

        public async Task<int> SaveChangesAsync()
        {
            return await _context.SaveChangesAsync();
        }

        public async Task BeginTransactionAsync()
        {
            _transaction = await _context.Database.BeginTransactionAsync();
        }

        public async Task CommitTransactionAsync()
        {
            if (_transaction != null)
            {
                await _transaction.CommitAsync();
                await _transaction.DisposeAsync();
                _transaction = null;
            }
        }

        public async Task RollbackTransactionAsync()
        {
            if (_transaction != null)
            {
                await _transaction.RollbackAsync();
                await _transaction.DisposeAsync();
                _transaction = null;
            }
        }

        public void Dispose()
        {
            _transaction?.Dispose();
            _context.Dispose();
        }
    }
}
