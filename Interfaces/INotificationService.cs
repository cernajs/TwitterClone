using TwitterClone.Models;

namespace TwitterClone.Data;

public interface INotificationService
{
    Task SendTweetNotificationAsync(int tweetId, string username, string content, DateTime createdAt, int likesCount, string userId);

    Task NotifyFollowersOfNewTweetAsync(string userId, string message, int tweetId);

    Task<int> GetNotificationCountAsync(string userId);

    Task<List<Notification>> ShowNotificationsAsync(string userId);
}