using Microsoft.AspNetCore.SignalR;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

using TwitterClone.Hubs;

namespace TwitterClone.Data;

public class NotificationService : INotificationService
{
    private readonly TwitterContext _tweetRepo;
    private readonly IHubContext<NotificationHub> _hubContext;

    public NotificationService(TwitterContext db, UserManager<ApplicationUser> userManager, IHubContext<NotificationHub> hubContext)
    {
        _tweetRepo = db;
        _hubContext = hubContext;
    }

    /// <summary>
    ///     Send a notification to all connected clients
    ///     that will generate new tweet using javascript
    /// </summary>
    /// <param name="tweetId"></param>
    /// <param name="username"></param>
    /// <param name="content"></param>
    /// <param name="createdAt"></param>
    /// <param name="likesCount"></param>
    /// <param name="userId"></param>
    /// <returns></returns>
    public async Task SendTweetNotificationAsync(int tweetId, string username, string content, DateTime createdAt, int likesCount, string userId)
    {
        await _hubContext.Clients.All.SendAsync("ReceiveTweet", tweetId, username, content, createdAt.ToString(), likesCount, userId, false );
    }

    /// <summary>
    ///    Send a notification about new tweet to followers
    ///    of a user with given userId
    ///    and add a notification to database
    /// </summary>
    /// <param name="userId"></param>
    /// <param name="message"></param>
    /// <param name="tweetId"></param>
    /// <returns></returns>
    public async Task NotifyFollowersOfNewTweetAsync(string userId, string message, int tweetId)
    {
        var followers = await _tweetRepo.UserFollowers
            .Where(uf => uf.FollowingId == userId)
            .Select(uf => uf.FollowerId)
            .ToListAsync();

        foreach(var follower in followers)
        {

            var notification = new Notification
            {
                UserId = follower,
                Message = message,
                TweetId = tweetId,
                Timestamp = DateTime.Now
            };

            _tweetRepo.Notifications.Add(notification);
            await _tweetRepo.SaveChangesAsync();

            await _hubContext.Clients.User(follower).SendAsync("ReceiveNotification", message);
        }
    }
}