using Microsoft.AspNetCore.SignalR;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

using TwitterClone.Models;
using TwitterClone.Hubs;

namespace TwitterClone.Data;

public class NotificationService : INotificationService
{
    private readonly TwitterContext _tweetRepo;
    private readonly IHubContext<NotificationHub> _hubContext;
    private readonly UserManager<ApplicationUser> _userManager;

    public NotificationService(TwitterContext db,
                                UserManager<ApplicationUser> userManager,
                                IHubContext<NotificationHub> hubContext)
    {
        _tweetRepo = db;
        _hubContext = hubContext;
        _userManager = userManager;
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

        foreach (var connectionId in NotificationHub.Users.Keys)
        {
            if (NotificationHub.Users.TryGetValue(connectionId, out string connectedUserId))
            {
                bool isCurrentUser = connectedUserId == userId;
                await _hubContext.Clients.Client(connectionId).SendAsync("ReceiveTweet", tweetId, username, content, createdAt.ToString(), likesCount, userId, isCurrentUser);
            }
        }
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
                Type = NotificationType.Tweet,
                Timestamp = DateTime.Now
            };

            _tweetRepo.Notifications.Add(notification);
            await _tweetRepo.SaveChangesAsync();

            await _hubContext.Clients.User(follower).SendAsync("ReceiveNotification", message);
        }
    }

    public async Task NotifyTweetOwner(string userId, int tweetId, string message, NotificationType type)
    {
        var tweet = await _tweetRepo.Tweets
            .Include(t => t.User)
            .FirstOrDefaultAsync(t => t.Id == tweetId);

        if (tweet == null) return;

        var user = await _userManager.FindByIdAsync(userId);

        if (user == null) return;

        // Don't notify the owner of the tweet
        if (tweet.User.Id == user.Id) return;

        Notification notification;

        notification = type switch
        {
            NotificationType.TweetLike => new Notification
            {
                UserId = tweet.User.Id,
                Message = message,
                TweetId = tweetId,
                Type = type,
                Timestamp = DateTime.Now
            },
            NotificationType.TweetReply => new Notification
            {
                UserId = tweet.User.Id,
                Message = message,
                TweetId = tweetId,
                Type = type,
                Timestamp = DateTime.Now
            },
            _ => throw new NotImplementedException(),
        };

        _tweetRepo.Notifications.Add(notification);
        await _tweetRepo.SaveChangesAsync();

        await _hubContext.Clients.User(tweet.User.Id).SendAsync("ReceiveNotification", notification.Message);
    }

    public async Task NotifyRecipientAsync(ChatMessageDto chatMessageDto, string message, string senderId)
    {
        var notification = new Notification
        {
            UserId = chatMessageDto.RecipientId,
            Message = message,
            SenderId = senderId,
            Type = NotificationType.ChatMessage,
            Timestamp = DateTime.Now
        };

        _tweetRepo.Notifications.Add(notification);
        await _tweetRepo.SaveChangesAsync();

        await _hubContext.Clients.User(chatMessageDto.RecipientId).SendAsync("ReceiveNotification", notification.Message);
    }

    public async Task NotifyUserOfNewFollow(string userId, string followerId)
    {
        var follower = await _userManager.FindByIdAsync(followerId);

        var notification = new Notification
        {
            UserId = userId,
            Message = $"{follower.UserName} started following you",
            FollowerId = followerId,
            Type = NotificationType.Following,
            Timestamp = DateTime.Now
        };

        _tweetRepo.Notifications.Add(notification);
        await _tweetRepo.SaveChangesAsync();

        await _hubContext.Clients.User(userId).SendAsync("ReceiveNotification", notification.Message);
    }


    /// <summary>
    ///     Return the number of notifications for a given user
    /// </summary>
    /// <param name="userId"></param>
    /// <returns></returns>
    public Task<int> GetNotificationCountAsync(string userId)
    {
        return _tweetRepo.Notifications
            .Where(n => n.UserId == userId && !n.IsSeen)
            .CountAsync();
    }

    /// <summary>
    ///     Retrieve all notifications for a given user,
    ///     mark them as seen, and delete notifications
    ///     that has been viewed
    /// </summary>
    /// <param name = "userId" ></ param >
    /// <returns></returns>
    public async Task<List<Notification>> ShowNotificationsAsync(string userId) {
        
        var notifications = await _tweetRepo.Notifications
            .Include(n => n.Tweet)
            .Include(n => n.Tweet.User)
            .Include(n => n.TweetLike)
            .Where(n => n.UserId == userId)
            .OrderByDescending(n => n.Timestamp)
            .ToListAsync();

        var NotificationsAlreadySeen = new List<Notification>();

        foreach (var notification in notifications)
        {
            if (!notification.IsSeen)
            {
                notification.IsSeen = true;
            }
            else
            {
                NotificationsAlreadySeen.Add(notification);
            }
        }

        foreach (var notification in NotificationsAlreadySeen)
        {
            if (notification != null)
            {
                _tweetRepo.Notifications.Remove(notification);
            }
        }

        await _tweetRepo.SaveChangesAsync();

        var filteredNotification = notifications.Where(n => !NotificationsAlreadySeen.Contains(n)).ToList();

        return filteredNotification;
    }

}