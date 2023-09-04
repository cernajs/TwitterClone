namespace TwitterClone.Data;

public interface IHashtagService
{
    (List<string>, string) ParseHashtags(string tweetContent);
    public Task CreateHashtagsAsync(List<string> hashtags, int tweetId);
}