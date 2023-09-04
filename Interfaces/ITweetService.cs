namespace TwitterClone.Data;

public interface ITweetService
{
    Task<Tweet> CreateTweetAsync(ApplicationUser user, string tweetContent);
}