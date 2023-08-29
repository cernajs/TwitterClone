namespace TwitterClone.Data;

public class TweetHashtag
{
    public int TweetId { get; set; }
    public Tweet Tweet { get; set; }
    public int HashtagId { get; set; }
    public Hashtag Hashtag { get; set; }
}