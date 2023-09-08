namespace TwitterClone.Models;

public class Notification
{
    public int Id { get; set; }
    public string UserId { get; set; }
    public ApplicationUser User { get; set; }
    public int TweetId { get; set; }
    public Tweet Tweet { get; set; }
    public string Message { get; set; }
    public bool IsSeen { get; set; }
    public DateTime Timestamp { get; set; }
}