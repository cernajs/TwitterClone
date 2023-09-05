using System;
using Microsoft.AspNetCore.SignalR;
using Microsoft.AspNetCore.Identity;
using System.Security.Claims;

using TwitterClone.Data;
using System.Collections.Concurrent;

namespace TwitterClone.Hubs;

public class NotificationHub : Hub
{
    public static ConcurrentDictionary<string, string> Users = new ConcurrentDictionary<string, string>();

    private readonly UserManager<ApplicationUser> _userManager;
    public NotificationHub(UserManager<ApplicationUser> userManager)
    {
        _userManager = userManager;
    }

    public async Task SendMessage(string recipientId, string content)
    {
        await Clients.User(recipientId).SendAsync("ReceiveMessage", content, Context.User.Identity.Name);
    }

    public async Task SendToSpecificUser(string userId, string message)
    {
        await Clients.User(userId).SendAsync("ReceiveNotification", message);
    }

    public async Task SendTweet(string username, string content, string createdAt)
    {
        await Clients.All.SendAsync("ReceiveTweet", username, content, createdAt);
    }

    public override async Task OnDisconnectedAsync(Exception exception)
    {
        string userId;
        Users.TryRemove(Context.ConnectionId, out userId);
        await base.OnDisconnectedAsync(exception);
    }

    public override async Task OnConnectedAsync()
    {
        var userId = Context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        Users.TryAdd(Context.ConnectionId, userId);
        await base.OnConnectedAsync();

        if (userId != null)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, userId);
        }

        await base.OnConnectedAsync();
    }
}
