namespace TwitterClone.Data;

public class Retweet
{
    public int Id { get; set; }
    public string UserId { get; set; }
    public ApplicationUser User { get; set; }
    public int TweetId { get; set; }
    public Tweet Tweet { get; set; }
    public DateTime RetweetTime { get; set; }
}
