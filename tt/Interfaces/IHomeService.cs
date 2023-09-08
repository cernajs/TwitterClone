using TwitterClone.Models;

namespace TwitterClone.Data;

public interface IHomeService
{
    Task<List<string>> GetTrendingTopicsAsync();

    Task<List<ApplicationUser>> GetFollowSuggestAsync(string userId);
}

