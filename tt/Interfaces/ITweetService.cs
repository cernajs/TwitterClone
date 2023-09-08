using TwitterClone.Models;

namespace TwitterClone.Data;

public interface ITweetService
{
    Task<Tweet> CreateTweetAsync(ApplicationUser user, string tweetContent);
    Task<bool> LikeTweetAsync(string userId, int tweetId);
    Task<bool> DeleteAsync(string currentUserId, int tweetId);
    Task<bool> UnlikeTweetAsync(string userId, int tweetId);
    Task<IEnumerable<ApplicationUser>> ShowLikesAsync(int id);
    Task<Tweet> ReplyToTweetAsync(int parentTweetId, string content, ApplicationUser currentUser);
    Task<(Tweet? ParentTweet, IEnumerable<Tweet> Replies)> ViewRepliesAndParentAsync(int id);
    Task<bool> BookmarkTweetAsync(int tweetId, string currentUserId, bool isBookmarked);
    Task<bool> Retweet(int tweetId, string currentUserId, bool isRetweeted);
}