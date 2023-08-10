//using EntityFrameworkCore.InMemory;

namespace TwitterClone.Data;

public class Tweet
{
    //[Key]
    public int Id { get; set; }
    //[Required]
    //[MaxLength(50)]
    public string Username { get; set; }
    //[Required]
    //[MaxLength(280)]
    public string TweetContent { get; set; }
    public DateTime CreatedAt { get; set; }

    public override string ToString()
    {
        // if(TweetContent == null)
        // {
        //     return $"TweetContent: null";
        // }
        //return $"Username: {Username}, TweetContent: {TweetContent}";
        return Id.ToString();
    }
}

// public enum TweetViewModelAction
// {
//     NewTweet,
//     ListTweets
// }

public class TweetViewModel
{
    //public TweetViewModelAction Action { get; set; }
    public IList<Tweet> Tweets { get; set; }
    public Tweet? NewTweet { get; set; }

    public override string ToString()
    {
        return $"Tweets: {Tweets}, NewTweet: {NewTweet}";
    }
}