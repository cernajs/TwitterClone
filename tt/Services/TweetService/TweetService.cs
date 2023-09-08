using Microsoft.EntityFrameworkCore;

using TwitterClone.Models;

namespace TwitterClone.Data;

public class TweetService : ITweetService
{
    private readonly TwitterContext _tweetRepo;

    public TweetService(TwitterContext db)
    {
        _tweetRepo = db;
    }

    /// <summary>
    ///     Create a new tweet and add it to the database
    /// </summary>
    /// <param name="user"></param>
    /// <param name="tweetContent"></param>
    /// <returns></returns>
    public async Task<Tweet> CreateTweetAsync(ApplicationUser user, string tweetContent)
    {
        var tweet = new TweetBuilder()
            .WithUser(user)
            .WithContent(tweetContent)
            .WithCreatedAt(DateTime.Now)
            .Build();

        _tweetRepo.Tweets.Add(tweet);
        await _tweetRepo.SaveChangesAsync();

        return tweet;
    }

    /// <summary>
    ///     Like a tweet by adding a TweetLike relationship to the database
    /// </summary>
    /// <param name="userId"></param>
    /// <param name="tweetId"></param>
    /// <returns></returns>
    public async Task<bool> LikeTweetAsync(string userId, int tweetId)
    {
        var existingLike = await _tweetRepo.TweetLikes
            .FirstOrDefaultAsync(l => l.TweetId == tweetId && l.UserId == userId);

        if (existingLike == null)
        {
            var like = new TweetLike
            {
                TweetId = tweetId,
                UserId = userId,
                LikedAt = DateTime.Now
            };
            _tweetRepo.TweetLikes.Add(like);
            await _tweetRepo.SaveChangesAsync();

            return true;
        }

        return false;
    }

    /// <summary>
    ///     delete a tweet if the user is the owner of the tweet
    /// </summary>
    /// <param name="currentUserId"></param>
    /// <param name="tweetId"></param>
    /// <returns></returns>
    public async Task<bool> DeleteAsync(string currentUserId, int tweetId)
    {
        var tweet = await _tweetRepo.Tweets.FirstOrDefaultAsync(t => t.Id == tweetId);
        if (tweet == null)
        {
            return false;
        }

        if (tweet.User.Id != currentUserId)
        {
            return false;
        }

        _tweetRepo.Tweets.Remove(tweet);
        await _tweetRepo.SaveChangesAsync();

        return true;
    }

    /// <summary>
    ///     Unlike a tweet by removing the TweetLike relationship from the database
    /// </summary>
    /// <param name="userId"></param>
    /// <param name="tweetId"></param>
    /// <returns></returns>
    public async Task<bool> UnlikeTweetAsync(string userId, int tweetId)
    {
        var existingLike = await _tweetRepo.TweetLikes
            .FirstOrDefaultAsync(l => l.TweetId == tweetId && l.UserId == userId);

        if (existingLike == null)
        {
            return false;
        }

        _tweetRepo.TweetLikes.Remove(existingLike);
        await _tweetRepo.SaveChangesAsync();

        return true;
    }

    /// <summary>
    ///     Retrieve all tweets from the database that are liked by the user with the given id
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    public Task<IEnumerable<ApplicationUser>> ShowLikesAsync(int id)
    {
        return _tweetRepo.Tweets
            .Where(t => t.Id == id)
            .SelectMany(t => t.Likes.Select(l => l.User))
            .ToListAsync()
            .ContinueWith(task => task.Result.AsEnumerable() as IEnumerable<ApplicationUser>);
    }


    /// <summary>
    ///     Create reply to tweet with the given parentTweetId and add it to the database
    /// </summary>
    /// <param name="parentTweetId"></param>
    /// <param name="content"></param>
    /// <param name="currentUser"></param>
    /// <returns></returns>
    public async Task<Tweet> ReplyToTweetAsync(int parentTweetId, string content, ApplicationUser currentUser)
    {
        var parentTweet = _tweetRepo.Tweets.FirstOrDefault(t => t.Id == parentTweetId);
        if (parentTweet == null)
        {
            return null;
        }

        var newTweet = new TweetBuilder()
            .WithUser(currentUser)
            .WithContent(content)
            .WithCreatedAt(DateTime.Now)
            .WithParentTweetId(parentTweetId)
            .WithParentTweet(parentTweet)
            .Build();

        _tweetRepo.Tweets.Add(newTweet);
        await _tweetRepo.SaveChangesAsync();

        return newTweet;
    }

    /// <summary>
    ///     Retrieve all tweets from the database that are replies to the tweet with the given id
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    public async Task<(Tweet? ParentTweet, IEnumerable<Tweet> Replies)> ViewRepliesAndParentAsync(int id)
    {
        // Query for the parent tweet
        var parentTweet = await _tweetRepo.Tweets
                                          .Include(t => t.User) // Include the user information
                                          .FirstOrDefaultAsync(t => t.Id == id);

        // Query for the replies
        var replies = await _tweetRepo.Tweets
                                      .Include(t => t.ParentTweet)
                                      .Include(t => t.User) // Include the user information
                                      .Where(t => t.ParentTweetId == id)
                                      .ToListAsync();

        return (parentTweet, replies);
    }


    /// <summary>
    ///     Bookmark a tweet by adding a TweetBookmark relationship to the database
    ///     or unbookmark a tweet by removing the TweetBookmark relationship from the database
    ///     depending on the value of isBookmarked
    /// </summary>
    /// <param name="tweetId"></param>
    /// <param name="currentUserId"></param>
    /// <param name="isBookmarked"></param>
    /// <returns></returns>
    public async Task<bool> BookmarkTweetAsync(int tweetId, string currentUserId, bool isBookmarked)
    {
        if(isBookmarked)
        {
            if(_tweetRepo.TweetBookmarks.Any(tb => tb.UserId == currentUserId && tb.TweetId == tweetId))
            {
                return false;
            }

            var tweetBookmark = new TweetBookmark
            {
                UserId = currentUserId,
                TweetId = tweetId
            };

            _tweetRepo.TweetBookmarks.Add(tweetBookmark);
        }
        else
        {
            var tweetBookmark = await _tweetRepo.TweetBookmarks
                .Where(tb => tb.UserId == currentUserId && tb.TweetId == tweetId)
                .FirstOrDefaultAsync();

            if(tweetBookmark == null)
            {
                return false;
            }

            if (tweetBookmark != null)
            {
                _tweetRepo.TweetBookmarks.Remove(tweetBookmark);
                
            }
        }
        await _tweetRepo.SaveChangesAsync();
        return true;
    }

    /// <summary>
    ///    Retweet a tweet by adding a Retweet relationship to the database
    ///    or unretweet a tweet by removing the Retweet relationship from the database
    ///    depending on the value of isRetweeted
    /// </summary>
    /// <param name="tweetId"></param>
    /// <param name="currentUserId"></param>
    /// <param name="isRetweeted"></param>
    /// <returns></returns>
    public async Task<bool> Retweet(int tweetId, string currentUserId, bool isRetweeted)
    {
        if(isRetweeted)
        {
            if(_tweetRepo.Retweets.Any(r => r.UserId == currentUserId && r.TweetId == tweetId))
            {
                return false;
            }
            var retweet = new Retweet {
                UserId = currentUserId,
                TweetId = tweetId,
                RetweetTime = DateTime.Now
            };

            _tweetRepo.Retweets.Add(retweet);
        }
        else
        {
            var retweet = await _tweetRepo.Retweets
                .Where(r => r.UserId == currentUserId && r.TweetId == tweetId)
                .FirstOrDefaultAsync();

            if(retweet == null)
            {
                return false;
            }

            if (retweet != null)
            {
                _tweetRepo.Retweets.Remove(retweet);
            }
        }
        await _tweetRepo.SaveChangesAsync();
        return true;
    }
}