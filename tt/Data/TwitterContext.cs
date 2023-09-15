using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

using TwitterClone.Models;

namespace TwitterClone.Data
{
    public class TwitterContext : IdentityDbContext<ApplicationUser>
    {
        public TwitterContext(DbContextOptions<TwitterContext> options) : base(options)
        {
        }

        public DbSet<Tweet> Tweets { get; set; }
        public DbSet<UserFollower> UserFollowers { get; set; }
        public DbSet<TweetLike> TweetLikes { get; set; }
        public DbSet<Hashtag> Hashtags { get; set; }
        public DbSet<TweetHashtag> TweetHashtags { get; set; }
        public DbSet<ChatMessage> ChatMessages { get; set; }
        public DbSet<TweetBookmark> TweetBookmarks { get; set; }
        public DbSet<Retweet> Retweets { get; set; }
        public DbSet<Notification> Notifications { get; set; }

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
                .HasKey(tl => tl.Id);

            builder.Entity<TweetLike>()
                .HasAlternateKey(tl => new { tl.UserId, tl.TweetId });

            builder.Entity<TweetLike>()
                .HasOne(tl => tl.User)
                .WithMany(u => u.LikedTweets)
                .HasForeignKey(tl => tl.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<TweetLike>()
                .HasOne(tl => tl.Tweet)
                .WithMany(t => t.Likes)
                .HasForeignKey(tl => tl.TweetId)
                .OnDelete(DeleteBehavior.Cascade);


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

            //bookmarks
            builder.Entity<TweetBookmark>()
                .HasKey(tb => new { tb.UserId, tb.TweetId });

            builder.Entity<TweetBookmark>()
                .HasOne(tb => tb.User)
                .WithMany(u => u.BookmarkedTweets)
                .HasForeignKey(tb => tb.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<TweetBookmark>()
                .HasOne(tb => tb.Tweet)
                .WithMany(t => t.Bookmarks)
                .HasForeignKey(tb => tb.TweetId)
                .OnDelete(DeleteBehavior.Cascade);

            //retweets
            builder.Entity<Retweet>()
                .HasKey(r => new { r.UserId, r.TweetId });

            builder.Entity<Retweet>()
                .HasOne(r => r.User)
                .WithMany(u => u.Retweets)
                .HasForeignKey(r => r.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<Retweet>()
                .HasOne(r => r.Tweet)
                .WithMany(t => t.Retweets)
                .HasForeignKey(r => r.TweetId)
                .OnDelete(DeleteBehavior.Cascade);

            //notifications
            builder.Entity<Notification>()
                .HasKey(n => n.Id);

            builder.Entity<Notification>()
                .HasOne(n => n.User)
                .WithMany()
                .HasForeignKey(n => n.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<Notification>()
                .HasOne(n => n.Tweet)
                .WithMany()
                .HasForeignKey(n => n.TweetId)
                .IsRequired(false)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<Notification>()
                .HasOne(n => n.TweetLike)
                .WithMany()
                .HasForeignKey(n => n.TweetLikeId)
                .IsRequired(false)
                .OnDelete(DeleteBehavior.Restrict);


        }
    }
}