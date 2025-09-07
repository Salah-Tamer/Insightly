using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Insightly.Models
{
    public class AppDbContext: IdentityDbContext<ApplicationUser>
    {
        public AppDbContext(DbContextOptions<AppDbContext> options): base(options) { }
        public DbSet<Article> Articles { get; set; }
        public DbSet<Comment> Comments { get; set; }
        public DbSet<ArticleRead> ArticleReads { get; set; }
        public DbSet<Vote> Votes { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // User
            modelBuilder.Entity<ApplicationUser>(entity =>
            {
                entity.HasKey(u => u.Id);
                entity.Property(u => u.Name).IsRequired().HasMaxLength(100);
                entity.Property(u => u.Email).IsRequired();
                entity.Property(u => u.PasswordHash).IsRequired();
                entity.Property(u => u.Gender).IsRequired().HasMaxLength(10);
                entity.Property(u => u.CreatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");
            });

            // Article
            modelBuilder.Entity<Article>(entity =>
            {
                entity.HasKey(a => a.ArticleId);
                entity.Property(a => a.Title).IsRequired().HasMaxLength(100);
                entity.Property(a => a.Content).IsRequired();
                entity.Property(a => a.CreatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");
                entity.HasOne(a => a.Author)
                    .WithMany(u => u.Articles)
                    .HasForeignKey(a => a.AuthorId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

           
            modelBuilder.Entity<Comment>(entity =>
            {
                entity.HasKey(c => c.CommentId);
                entity.Property(c => c.Content).IsRequired();
                entity.Property(c => c.CreatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");
                entity.Property(c => c.UpdatedAt).IsRequired(false);

                entity.HasOne(c => c.Author)
                    .WithMany(u => u.Comments)
                    .HasForeignKey(c => c.AuthorId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(c => c.Article)
                    .WithMany(a => a.Comments)
                    .HasForeignKey(c => c.ArticleId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // ArticleRead
            modelBuilder.Entity<ArticleRead>(entity =>
            {
                entity.HasKey(ar => ar.Id);
                entity.Property(ar => ar.ReadAt).HasDefaultValueSql("CURRENT_TIMESTAMP");

                entity.HasOne(ar => ar.Article)
                    .WithMany()
                    .HasForeignKey(ar => ar.ArticleId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(ar => ar.User)
                    .WithMany(u => u.ReadArticles)
                    .HasForeignKey(ar => ar.UserId)
                    .OnDelete(DeleteBehavior.Restrict);
            });
            modelBuilder.Entity<Vote>(entity =>
            {
                entity.HasKey(v => v.VoteId);
                entity.Property(v => v.VoteId).IsRequired();

                entity.HasIndex(v => new { v.UserId, v.ArticleId })
                    .IsUnique()
                    .HasDatabaseName("IX_Votes_UserId_ArticleId");

                entity.HasOne(v => v.User)
                    .WithMany(u => u.Votes)
                    .HasForeignKey(v => v.UserId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(v => v.Article)
                    .WithMany(a => a.Votes)
                    .HasForeignKey(v => v.ArticleId)
                    .OnDelete(DeleteBehavior.Cascade);
            });
        }
    }
}