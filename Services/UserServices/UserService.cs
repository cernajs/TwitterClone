using System.Security.Claims;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication;

using TwitterClone.Models;


namespace TwitterClone.Data;

public class UserService : IUserService
{
    private readonly TwitterContext _tweetRepo;
    private readonly UserManager<ApplicationUser> _userManager;

    public UserService(TwitterContext db,
                        UserManager<ApplicationUser> userManager)
    {
        _tweetRepo = db;
        _userManager = userManager;
    }

    /// <summary>
    ///     Get the current user from the database
    /// </summary>
    /// <param name="claimsPrincipal"></param>
    /// <returns></returns>
    public async Task<ApplicationUser?> GetUserAsync(ClaimsPrincipal claimsPrincipal)
    {
        return await _userManager.GetUserAsync(claimsPrincipal);
    }

    /// <summary>
    ///    Get the current user from the database with related data
    ///    (Followers, Following, Retweets, Tweets, Likes, LikedTweets)
    ///    This is used to display the user's profile page
    /// </summary>
    /// <param name="userId"></param>
    /// <returns></returns>
    public Task<ApplicationUser?> GetUserRelatedDataAsync(string userId)
    {
        return _tweetRepo.Users
                .Include(u => u.Followers)
                .Include(u => u.Following)
                .Include(u => u.Retweets)
                    .ThenInclude(r => r.Tweet)
                        .ThenInclude(t => t.User)
                .Include(u => u.Tweets)
                    .ThenInclude(t => t.Likes)
                .Include(u => u.LikedTweets).FirstOrDefaultAsync(u => u.Id == userId);
    }

    /// <summary>
    ///    Follow a user by adding a UserFollower relationship to the database
    /// </summary>
    /// <param name="followerId"></param>
    /// <param name="userIdToFollow"></param>
    /// <returns></returns>
    public async Task<bool> FollowUserAsync(string followerId, string userIdToFollow)
    {
        if (_tweetRepo.UserFollowers.Any(uf => uf.FollowerId == followerId && uf.FollowingId == userIdToFollow))
        {
            return false;
        }

        var userFollower = new UserFollower
        {
            FollowerId = followerId,
            FollowingId = userIdToFollow
        };

        _tweetRepo.UserFollowers.Add(userFollower);
        await _tweetRepo.SaveChangesAsync();

        return true;
    }

    /// <summary>
    ///     remove a UserFollower relationship from the database
    /// </summary>
    /// <param name="followerId"></param>
    /// <param name="userIdToUnfollow"></param>
    /// <returns></returns>
    public async Task<bool> UnfollowUserAsync(string followerId, string userIdToUnfollow)
    {
        var followingRelationship = await _tweetRepo.UserFollowers
            .FirstOrDefaultAsync(uf => uf.FollowerId == followerId && uf.FollowingId == userIdToUnfollow);

        if (followingRelationship == null)
        {
            return false;
        }

        _tweetRepo.UserFollowers.Remove(followingRelationship);
        await _tweetRepo.SaveChangesAsync();
        return true;
    }

    /// <summary>
    ///     Get a list of users that the current user is following or followers of the current user
    /// </summary>
    /// <param name="userId"></param>
    /// <param name="type"></param>
    /// <returns></returns>
    public Task<List<ApplicationUser>>? ShowUsersAsync(string userId, string type)
    {
        if(type == "followers")
        {
            return _tweetRepo.Users
                                .Where(u => u.Id == userId)
                                .SelectMany(u => u.Followers.Select(f => f.Follower))
                                .ToListAsync();

        }
        else if(type == "followings")
        {
            return _tweetRepo.Users
                                .Where(u => u.Id == userId)
                                .SelectMany(u => u.Following.Select(f => f.Following))
                                .ToListAsync();
            
        }
        else
        {
            return null;
        }
    }

    /// <summary>
    ///     Edit the current user's profile
    /// </summary>
    /// <param name="model"></param>
    /// <returns></returns>
    public async Task<EditProfileResult> EditUserProfileAsync(EditProfileViewModel model)
    {
        var result = new EditProfileResult();

        var user = await _userManager.FindByIdAsync(model.Id);
        if (user == null)
        {
            result.Errors.Add("User not found");
            return result;
        }

        var existingUser = await _userManager.FindByEmailAsync(model.Email);
        if (existingUser != null && existingUser.Id != model.Id)
        {
            result.Errors.Add("Email already exists.");
            return result;
        }

        var tweets = await _tweetRepo.Tweets.Where(t => t.UserId == model.Id).ToListAsync();
        foreach (var tweet in tweets)
        {
            tweet.Username = model.UserName;
        }

        user.UserName = model.UserName;
        user.Email = model.Email;

        await _tweetRepo.SaveChangesAsync();

        var updateResult = await _userManager.UpdateAsync(user);
        if (updateResult.Succeeded)
        {
            result.Success = true;
            return result;
        }
        else
        {
            result.Errors = updateResult.Errors.Select(e => e.Description).ToList();
            return result;
        }
    }

    /// <summary>
    ///     Get a list of tweets that the current user has bookmarked
    /// </summary>
    /// <param name="userId"></param>
    /// <returns></returns>
    public Task<List<Tweet>> GetBookmarksAsync(string userId)
    {
        return _tweetRepo.TweetBookmarks
            .Where(b => b.UserId == userId)
            .Include(b => b.Tweet)
            .ThenInclude(t => t.User)
            .Include(b => b.Tweet.Likes)
            .Include(b => b.Tweet.Bookmarks)
            .Include(b => b.Tweet.Retweets)
            .Include(b => b.Tweet.Replies)
            .Select(b => b.Tweet)
            .ToListAsync();
    }
}