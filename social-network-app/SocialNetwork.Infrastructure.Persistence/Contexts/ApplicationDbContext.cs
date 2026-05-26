using Microsoft.EntityFrameworkCore;
using SocialNetwork.Core.Domain.Common;
using SocialNetwork.Core.Domain.Entities;

namespace SocialNetwork.Infrastructure.Persistence.Contexts
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

        public DbSet<Post> Posts { get; set; }
        public DbSet<Friend> Friends { get; set; }
        public DbSet<Comment> Comments { get; set; }
        public DbSet<Reply> Replies { get; set; }
        public DbSet<Message> Messages { get; set; }
        public DbSet<PostLike> PostLikes { get; set; }

        public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = new CancellationToken())
        {
            foreach (var entry in ChangeTracker.Entries<AuditableBaseEntity>())
            {
                switch (entry.State)
                {
                    case EntityState.Added:
                        entry.Entity.Created = DateTime.Now;
                        entry.Entity.CreatedBy = "DefaultAppUser";
                        break;
                    case EntityState.Modified:
                        entry.Entity.LastModified = DateTime.Now;
                        entry.Entity.LastModifiedBy = "DefaultAppUser";
                        break;
                }
            }
            return base.SaveChangesAsync(cancellationToken);
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            //FLUENT API

            #region tables
            modelBuilder.Entity<Post>().ToTable("Posts");
            modelBuilder.Entity<Friend>().ToTable("Friends");
            modelBuilder.Entity<Comment>().ToTable("Comments");
            modelBuilder.Entity<Reply>().ToTable("Replies");
            modelBuilder.Entity<Message>().ToTable("Messages");
            modelBuilder.Entity<PostLike>().ToTable("PostLikes");
            #endregion

            #region "primary keys"
            modelBuilder.Entity<Post>().HasKey(p => p.Id);
            modelBuilder.Entity<Friend>().HasKey(f => f.Id);
            modelBuilder.Entity<Comment>().HasKey(c => c.Id);
            modelBuilder.Entity<Reply>().HasKey(r => r.Id);
            modelBuilder.Entity<Message>().HasKey(m => m.Id);
            modelBuilder.Entity<PostLike>().HasKey(pl => pl.Id);

            #endregion

            #region relationships
            modelBuilder.Entity<Post>()
                .HasMany<Comment>(p => p.Comments)
                .WithOne(c => c.Post)
                .HasForeignKey(c => c.PostId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Comment>()
                .HasMany<Reply>(c => c.Replies)
                .WithOne(r => r.Comments)
                .HasForeignKey(r => r.CommentId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Post>()
                .HasMany<PostLike>(p => p.Likes)
                .WithOne(l => l.Post)
                .HasForeignKey(l => l.PostId)
                .OnDelete(DeleteBehavior.Cascade);
            #endregion

            #region "property configuration"

            #region posts
                modelBuilder.Entity<Post>().
                Property(p => p.UserId)
                .IsRequired();
            
            modelBuilder.Entity<Post>().
                Property(p => p.Content)
                .IsRequired();

            modelBuilder.Entity<Post>()
                .Property(p => p.LikeCount)
                .HasDefaultValue(0);
            #endregion

            #region friends
            modelBuilder.Entity<Friend>().
            Property(f => f.UserSenderId)
            .IsRequired();

            modelBuilder.Entity<Friend>().
                Property(f => f.UserReceptorId)
                .IsRequired();
            #endregion
            #region messages
modelBuilder.Entity<Message>()
    .Property(m => m.SenderId)
    .IsRequired();

modelBuilder.Entity<Message>()
    .Property(m => m.ReceiverId)
    .IsRequired();

modelBuilder.Entity<Message>()
    .Property(m => m.Content)
    .IsRequired();
    #endregion

            #region likes
            modelBuilder.Entity<PostLike>()
                .Property(pl => pl.UserId)
                .IsRequired();

            modelBuilder.Entity<PostLike>()
                .HasIndex(pl => new { pl.PostId, pl.UserId })
                .IsUnique();
            #endregion
            #region comments
            modelBuilder.Entity<Comment>().
            Property(c => c.UserId)
            .IsRequired();

            modelBuilder.Entity<Comment>().
                Property(c => c.Content)
                .IsRequired();
            #endregion

            #region replies
            modelBuilder.Entity<Reply>().
            Property(c => c.UserId)
            .IsRequired();

            modelBuilder.Entity<Reply>().
                Property(c => c.Content)
                .IsRequired();
            #endregion

            #endregion

        }
    }
}
