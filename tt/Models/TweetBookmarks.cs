namespace TwitterClone.Models;

public class TweetBookmark
{
    public int Id { get; set; }
    public string UserId { get; set; }
    public ApplicationUser User { get; set; }
    public int TweetId { get; set; }
    public Tweet Tweet { get; set; }
}
