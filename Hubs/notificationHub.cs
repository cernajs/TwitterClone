using Microsoft.AspNetCore.SignalR;
using Microsoft.AspNetCore.Identity;
using System.Security.Claims;

using TwitterClone.Data;

namespace TwitterClone.Hubs;

public class NotificationHub : Hub
{
    private readonly UserManager<ApplicationUser> _userManager;
    public NotificationHub(UserManager<ApplicationUser> userManager)
    {
        _userManager = userManager;
    }


    //???
    public async Task SendToSpecificUser(string userId, string message)
    {
        await Clients.User(userId).SendAsync("ReceiveNotification", message);
    }

    public async Task SendTweet(string username, string content, string createdAt)
    {
        await Clients.All.SendAsync("ReceiveTweet", username, content, createdAt);
    }

    public override async Task OnConnectedAsync()
    {
        var userId = Context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        if (userId != null)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, userId);
        }

        await base.OnConnectedAsync();
    }
}
