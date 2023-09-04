namespace TwitterClone.Data;

public interface INotificationService
{
    Task SendTweetNotificationAsync(int tweetId, string username, string content, DateTime createdAt, int likesCount, string userId);

    Task NotifyFollowersOfNewTweetAsync(string userId, string message, int tweetId);
}