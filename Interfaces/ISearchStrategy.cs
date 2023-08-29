namespace TwitterClone.Data;

public interface ISearchStrategy
{
    public Task<IEnumerable<Tweet>> SearchAsync(string query, TwitterContext context);
}