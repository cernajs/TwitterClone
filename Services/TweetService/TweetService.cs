namespace TwitterClone.Data;

public class TweetService : ITweetService
{
    private readonly TwitterContext _tweetRepo;

    public TweetService(TwitterContext db)
    {
        _tweetRepo = db;
    }

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
}