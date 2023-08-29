namespace TwitterClone.Data;

public interface ITweetRetrievalStrategy
{
    Task<IEnumerable<Tweet>> GetTweetsAsync(string? userId);
}