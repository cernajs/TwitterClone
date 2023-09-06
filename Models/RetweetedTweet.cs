namespace TwitterClone.Models;

public class RetweetedTweet : Tweet {
    public RetweetedTweet(Tweet tweet) {
        Id = tweet.Id;
        UserId = tweet.UserId;
        User = tweet.User;
        TweetContent = tweet.TweetContent;
        CreatedAt = tweet.CreatedAt;
        Likes = tweet.Likes;
        ParentTweetId = tweet.ParentTweetId;
        ParentTweet = tweet.ParentTweet;
        Replies = tweet.Replies;
        TweetHashtags = tweet.TweetHashtags;
        Bookmarks = tweet.Bookmarks;
        Retweets = tweet.Retweets;
        IsRetweet = true;
    }
    bool IsRetweet { get; set; }
}