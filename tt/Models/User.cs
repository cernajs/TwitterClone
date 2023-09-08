using System.Collections.Generic;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace TwitterClone.Models;

public class ApplicationUser : IdentityUser
{
    public string Bio { get; set; } = "This user has not set a bio yet.";
    public string ProfilePicture { get; set; } = "";


    public virtual ICollection<Tweet> Tweets { get; set; } = new List<Tweet>();

    public virtual ICollection<UserFollower> Followers { get; set; } = new List<UserFollower>();
    public virtual ICollection<UserFollower> Following { get; set; } = new List<UserFollower>();


    //likes
    public virtual ICollection<TweetLike> LikedTweets { get; set; } = new List<TweetLike>();

    //messages
    public virtual ICollection<ChatMessage> SentMessages { get; set; } = new List<ChatMessage>();
    public virtual ICollection<ChatMessage> ReceivedMessages { get; set; } = new List<ChatMessage>();

    //bookmarks
    public virtual ICollection<TweetBookmark> BookmarkedTweets { get; set; } = new List<TweetBookmark>();

    //retweets
    public virtual ICollection<Retweet> Retweets { get; set; } = new List<Retweet>();
}
