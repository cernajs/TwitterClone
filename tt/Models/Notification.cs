namespace TwitterClone.Models;

public enum NotificationType
{
    Tweet,
    ChatMessage,
    Following,
    TweetLike,
    TweetReply
}

public class Notification
{
    public int Id { get; set; }
    public string UserId { get; set; }
    public ApplicationUser User { get; set; }
    public int? TweetId { get; set; }
    public Tweet Tweet { get; set; }
    public string? SenderId { get; set; }
    public int? TweetLikeId { get; set; }
    public TweetLike TweetLike { get; set; }
    public string? FollowerId { get; set; }
    public string Message { get; set; }
    public NotificationType Type { get; set; }
    public bool IsSeen { get; set; }
    public DateTime Timestamp { get; set; }
}