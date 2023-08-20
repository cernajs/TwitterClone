using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace TwitterClone.Data
{
    public class TwitterContext : IdentityDbContext<ApplicationUser>
    {
        public TwitterContext(DbContextOptions<TwitterContext> options) : base(options)
        {
        }

        // public DbSet<Tweet> Tweets { get; set; }
        public DbSet<Tweet> Tweets { get; set; }
        public DbSet<UserFollower> UserFollowers { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.Entity<UserFollower>()
                .HasKey(uf => new { uf.FollowerId, uf.FollowingId });

            builder.Entity<UserFollower>()
                .HasOne(uf => uf.Follower)
                .WithMany(u => u.Following)
                .HasForeignKey(uf => uf.FollowerId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<UserFollower>()
                .HasOne(uf => uf.Following)
                .WithMany(u => u.Followers)
                .HasForeignKey(uf => uf.FollowingId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}