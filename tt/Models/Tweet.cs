using System.ComponentModel.DataAnnotations;

namespace TwitterClone.Models;

public class Tweet
{
    [Key]
    [Required]
    public int Id { get; set; }
    [Required]
    //[MaxLength(50)]
    public string Username { get; set; }
    [Required]
    //[MaxLength(280)]
    public string TweetContent { get; set; }
    public DateTime CreatedAt { get; set; }

    bool retweet = false;

    public override string ToString()
    {
        return $"Username: {Username}, TweetContent: {TweetContent}";
    }

    // User reference
    public string UserId { get; set; }
    public virtual ApplicationUser User { get; set; }


    public virtual ICollection<TweetLike> Likes { get; set; } = new List<TweetLike>();


    // replies structure
    // ParentTweetId == null for a top-level tweet
    public int? ParentTweetId { get; set; }
    public virtual Tweet ParentTweet { get; set; }

    public virtual ICollection<Tweet> Replies { get; set; } = new List<Tweet>();

    public virtual ICollection<TweetHashtag> TweetHashtags { get; set; } = new List<TweetHashtag>();

    public virtual ICollection<TweetBookmark> Bookmarks { get; set; } = new List<TweetBookmark>();

    public virtual ICollection<Retweet> Retweets { get; set; } = new List<Retweet>();

}