using Microsoft.EntityFrameworkCore;

using TwitterClone.Models;

namespace TwitterClone.Data;

public class PopularTweetStrategy : IPopularTweetStrategy
{
    // private readonly IRepository<Tweet> _tweetRepository;
    // private readonly IRepository<Retweet> _retweetRepository;
    // private readonly IRepository<RetweetedTweet> _retweetedTweetRepository;
    private readonly TwitterContext _tweetRepo;

    // public PopularTweetStrategy(IRepository<Tweet> tweetRepository, IRepository<Retweet> retweetRepository, IRepository<RetweetedTweet> retweetedTweetRepository)
    // {
    //     _tweetRepository = tweetRepository;
    //     _retweetRepository = retweetRepository;
    //     _retweetedTweetRepository = retweetedTweetRepository;
    // }

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

    // public async Task<IEnumerable<Tweet>> GetTweetsAsync()
    // {
    //     var tweets = await _tweetRepository.ListAllAsync();
    //     var retweets = await _retweetRepository.ListAllAsync();
    //     var retweetedTweets = await _retweetedTweetRepository.ListAllAsync();

    //     var popularTweets = tweets
    //         .Where(t => t.Likes.Count + t.Bookmarks.Count + t.Replies.Count + retweets.Count(r => r.TweetId == t.Id) + retweetedTweets.Count(rt => rt.Id == t.Id) > 1)
    //         .OrderByDescending(t => t.Likes.Count + t.Bookmarks.Count + t.Replies.Count + retweets.Count(r => r.TweetId == t.Id) + retweetedTweets.Count(rt => rt.Id == t.Id))
    //         .Take(10);

    //     return popularTweets;
    // }
}