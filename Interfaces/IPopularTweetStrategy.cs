namespace TwitterClone.Data;

public interface IPopularTweetStrategy
{
    Task<IEnumerable<Tweet>> GetTweetsAsync();
}