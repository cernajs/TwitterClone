using Internal;
using System;
using Microsoft.EntityFrameworkCore;

using TwitterClone.Models;

namespace TwitterClone.Data;

public class HashtagSearch : ISearchStrategy
{
    public async Task<IEnumerable<Tweet>> SearchAsync(string query, TwitterContext context)
    {
        return await context.TweetHashtags
                      .Include(th => th.Hashtag)
                      .Include(th => th.Tweet)
                      .ThenInclude(t => t.User)
                      .Where(th => th.Hashtag.Tag.ToLower() == query.Substring(1).ToLower())
                      .Select(th => th.Tweet)
                      .ToListAsync();
    }
}