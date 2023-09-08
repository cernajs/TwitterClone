namespace TwitterClone.Models;

public class Hashtag
{
    public int Id { get; set; }
    public string Tag { get; set; }
    public virtual ICollection<TweetHashtag> TweetHashtags { get; set; }
}