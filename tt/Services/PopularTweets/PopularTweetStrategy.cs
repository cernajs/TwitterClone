using Microsoft.EntityFrameworkCore;

using TwitterClone.Models;

namespace TwitterClone.Data;

public class PopularTweetStrategy : IPopularTweetStrategy
{
    private readonly TwitterContext _tweetRepo;

    public PopularTweetStrategy(TwitterContext context)
    {
        _tweetRepo = context;
    }

    public async Task<IEnumerable<Tweet>> GetTweetsAsync()
    {
        return await _tweetRepo.Tweets
                .Include(t => t.User)
                .Include(t => t.Likes)
                .Include(t => t.Retweets)
                .Include(t => t.Replies)
                .OrderByDescending(t => t.Likes.Count + t.Retweets.Count + t.Replies.Count)
                .Take(10)
                .ToListAsync();
    } 
}