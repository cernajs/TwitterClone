using Microsoft.AspNetCore.SignalR;
using Microsoft.AspNetCore.Identity;
using System.Security.Claims;

using TwitterClone.Data;

namespace TwitterClone.Hubs;
//IHubContext<NotificationHub>

public interface IExtendedHub : IHubContext<NotificationHub>
{
    Task SendNotification(string userId, string message);
    Task SendTweet(string username, string content, string createdAt);

}