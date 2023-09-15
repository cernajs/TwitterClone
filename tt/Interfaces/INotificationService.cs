using TwitterClone.Models;

namespace TwitterClone.Data;

public interface INotificationService
{
    Task SendTweetNotificationAsync(int tweetId, string username, string content, DateTime createdAt, int likesCount, string userId);

    Task NotifyFollowersOfNewTweetAsync(string userId, string message, int tweetId);

    Task NotifyTweetOwner(string userId, int tweetId, NotificationType type);

    Task NotifyRecipientAsync(ChatMessageDto chatMessageDto, string senderId);

    Task NotifyUserOfNewFollow(string userId, string followerId);

    Task<int> GetNotificationCountAsync(string userId);

    Task<List<Notification>> ShowNotificationsAsync(string userId);
}