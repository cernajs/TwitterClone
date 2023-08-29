namespace TwitterClone.Data;
using Microsoft.EntityFrameworkCore;

public class GetFollowedUsersTweets : ITweetRetrievalStrategy {
    private readonly TwitterContext _tweetRepo;

    public GetFollowedUsersTweets(TwitterContext twitterContext) {
        _tweetRepo = twitterContext;
    }

    public async Task<IEnumerable<Tweet>> GetTweetsAsync(string? userId) {
        
        var followedUsersTweets = await (from tweet in _tweetRepo.Tweets
                               join userFollower in _tweetRepo.UserFollowers on tweet.UserId equals userFollower.FollowingId
                               where userFollower.FollowerId == userId
                               orderby tweet.CreatedAt descending
                               select tweet)
                              .ToListAsync();

        return followedUsersTweets;
    }
}