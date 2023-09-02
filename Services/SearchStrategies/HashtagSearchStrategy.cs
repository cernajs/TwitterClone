using Internal;

using System;
using Microsoft.EntityFrameworkCore;

namespace TwitterClone.Data;

public class HashtagSearch : ISearchStrategy
{
    public async Task<IEnumerable<Tweet>> SearchAsync(string query, TwitterContext context)
    {
        return await context.TweetHashtags
                      .Include(th => th.Hashtag)
                      .Include(th => th.Tweet)
                      .ThenInclude(t => t.User)
                      .Where(th => th.Hashtag.Tag.Equals(query.Substring(1), StringComparison.OrdinalIgnoreCase))
                      .Select(th => th.Tweet)
                      .ToListAsync();
    }
}