namespace TwitterClone.Data;
using Microsoft.EntityFrameworkCore;

using TwitterClone.Models;

public class UsernameSearch : ISearchStrategy
{
    public async Task<IEnumerable<Tweet>> SearchAsync(string query, TwitterContext context)
    {
        return await context.Tweets
                      .Include(t => t.User)
                      .Where(t => t.Username.Contains(query) || t.TweetContent.Contains(query))
                      .ToListAsync();
    }
}