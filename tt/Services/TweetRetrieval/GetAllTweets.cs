namespace TwitterClone.Data;
using Microsoft.EntityFrameworkCore;

using TwitterClone.Models;

public class GetAllTweets : ITweetRetrievalStrategy {
    private readonly TwitterContext _tweetRepo;

    public GetAllTweets(TwitterContext twitterContext) {
        _tweetRepo = twitterContext;
    }

    public async Task<IEnumerable<Tweet>> GetTweetsAsync(string? userId) {
        var tweets = await _tweetRepo.Tweets
            .Include(t => t.User)
            .Include(t => t.Likes)
            .Include(t => t.Bookmarks)
            .Include(t => t.Replies)
            .Include(t => t.Retweets)
            .Include(t => t.Replies).ToListAsync();

        return tweets as IEnumerable<Tweet>;
    }
}