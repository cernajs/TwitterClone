using System.Security.AccessControl;
using Internal;
using System;
using Microsoft.AspNetCore.Identity;
using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.SignalR;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Authorization;

using TwitterClone.Data;
using TwitterClone.Models;
using TwitterClone.Hubs;
using TwitterClone.SD;


namespace TwitterClone.Controllers;

[Authorize]
public class ChatController : Controller
{
    private readonly ILogger<ChatController> _logger;
    private readonly TwitterContext _tweetRepo;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IHubContext<NotificationHub> _hubContext;
    private readonly ITweetRetrievalStrategy _viewStrategy;

    public ChatController(ILogger<ChatController> logger, TwitterContext db,
                         UserManager<ApplicationUser> userManager, IHubContext<NotificationHub> hubContext,
                         ITweetRetrievalStrategy viewStrategy)
    {
        _logger = logger;
        _tweetRepo = db;
        _userManager = userManager;
        _hubContext = hubContext;
        _viewStrategy = viewStrategy;
    }

    public async Task<IActionResult> Index(string id)
    {
        var currentUser = await _userManager.GetUserAsync(User);
        ViewBag.CurrentUserId = currentUser.Id;
        ViewBag.OtherUserId = id;
        return View();
    }

    [HttpPost]
    public async Task<IActionResult> SendMessage(string recipientId, string content)
    {
        var sender = await _userManager.GetUserAsync(User);
        if (sender == null) return Challenge();  // Not logged in

        var message = new ChatMessage
        {
            SenderId = sender.Id,
            RecipientId = recipientId,
            Content = content,
            Timestamp = DateTime.UtcNow
        };

        _tweetRepo.ChatMessages.Add(message);
        await _tweetRepo.SaveChangesAsync();

        return RedirectToAction("Index");
    }
}