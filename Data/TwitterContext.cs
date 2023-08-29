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
        public DbSet<TweetLike> TweetLikes { get; set; }
        public DbSet<Hashtag> Hashtags { get; set; }
        public DbSet<TweetHashtag> TweetHashtags { get; set; }
        public DbSet<ChatMessage> ChatMessages { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            //followers
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


            //likes
            builder.Entity<TweetLike>()
                .HasKey(tl => new { tl.UserId, tl.TweetId });

            builder.Entity<TweetLike>()
                .HasOne(tl => tl.User)
                .WithMany(u => u.LikedTweets)
                .HasForeignKey(tl => tl.UserId);

            builder.Entity<TweetLike>()
                .HasOne(tl => tl.Tweet)
                .WithMany(t => t.Likes)
                .HasForeignKey(tl => tl.TweetId);


            //comments
            builder.Entity<Tweet>()
                .HasMany(t => t.Replies)
                .WithOne(t => t.ParentTweet)
                .HasForeignKey(t => t.ParentTweetId)
                .IsRequired(false);

            //hashtags
            builder.Entity<TweetHashtag>()
                .HasKey(th => new { th.TweetId, th.HashtagId });

            builder.Entity<TweetHashtag>()
                .HasOne(th => th.Tweet)
                .WithMany(t => t.TweetHashtags)
                .HasForeignKey(th => th.TweetId);

            builder.Entity<TweetHashtag>()
                .HasOne(th => th.Hashtag)
                .WithMany(h => h.TweetHashtags)
                .HasForeignKey(th => th.HashtagId);

            //chat
            builder.Entity<ChatMessage>()
                .HasOne(m => m.Sender)
                .WithMany(u => u.SentMessages)
                .HasForeignKey(m => m.SenderId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<ChatMessage>()
                .HasOne(m => m.Recipient)
                .WithMany(u => u.ReceivedMessages)
                .HasForeignKey(m => m.RecipientId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}