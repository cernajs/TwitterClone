using Microsoft.EntityFrameworkCore;

namespace TwitterClone.Data;

public class HomeService : IHomeService
{
    private readonly TwitterContext _tweetRepo;

    public HomeService(TwitterContext db)
    {
        _tweetRepo = db;
    }

    /// <summary>
    ///     retrieve the top 3 trending topics
    /// </summary>
    /// <returns></returns>
    public Task<List<string>> GetTrendingTopicsAsync()
    {
        return _tweetRepo.TweetHashtags
            .Include(th => th.Hashtag)
            .GroupBy(th => th.Hashtag.Tag)
            .Select(group => new
            {
                Tag = group.Key,
                Count = group.Count()
            })
            .OrderByDescending(g => g.Count)
            .Take(3)
            .Select(g => g.Tag)
            .ToListAsync();
    }

    /// <summary>
    ///     retrieve 3 random users that are not the current user
    /// </summary>
    /// <param name="userId"></param>
    /// <returns></returns>
    public Task<List<ApplicationUser>> GetFollowSuggestAsync(string userId)
    {
        return _tweetRepo.Users
            .Where(u => u.Id != userId)
            .OrderBy(u => Guid.NewGuid())
            .Take(3)
            .ToListAsync();

    }
}

