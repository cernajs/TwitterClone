namespace TwitterClone.Models;

public class TweetBuilder
{
    private readonly Tweet _tweet;

    public TweetBuilder()
    {
        _tweet = new Tweet();
    }

    public TweetBuilder WithUser(ApplicationUser user)
    {
        _tweet.UserId = user.Id;
        _tweet.Username = user.UserName;
        _tweet.User = user;
        return this;
    }

    public TweetBuilder WithContent(string content)
    {
        _tweet.TweetContent = content;
        return this;
    }

    public TweetBuilder WithCreatedAt(DateTime createdAt)
    {
        _tweet.CreatedAt = createdAt;
        return this;
    }

    public TweetBuilder WithParentTweetId(int parentTweetId)
    {
        _tweet.ParentTweetId = parentTweetId;
        return this;
    }

    public TweetBuilder WithParentTweet(Tweet parentTweet)
    {
        _tweet.ParentTweet = parentTweet;
        return this;
    }

    public Tweet Build()
    {
        // validate?
        return _tweet;
    }
}
