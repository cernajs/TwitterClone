using System.Text.RegularExpressions;

namespace TwitterClone.Data;

public class HashtagService : IHashtagService
{
    private readonly TwitterContext _tweetRepo;

    public HashtagService(TwitterContext db)
    {
        _tweetRepo = db;
    }

    public (List<string>, string) ParseHashtags(string tweetText)
    {
        var hashtags = new List<string>();
        var words = tweetText.Split(' ');
        for (int i = 0; i < words.Length; ++i)
        {
            if (words[i].StartsWith("#"))
            {
                var hashtag = words[i].Substring(1);
                hashtags.Add(hashtag);
                words[i] = $"<a href=\"/Home/Search?searchQuery=%23{hashtag}\">{words[i]}</a>";
            }
        }
        return (hashtags, string.Join(' ', words));
    }

    public async Task CreateHashtagsAsync(List<string> hashtags, int tweetId)
    {   
        foreach(var hashtag in hashtags)
        {
            var newHashtag = new Hashtag
            {
                Tag = hashtag
            };
            _tweetRepo.Hashtags.Add(newHashtag);


            var newTweetHashtag = new TweetHashtag
            {
                TweetId = tweetId,
                HashtagId = newHashtag.Id
            };
            _tweetRepo.TweetHashtags.Add(newTweetHashtag);
        }
        await _tweetRepo.SaveChangesAsync();
    }  
}