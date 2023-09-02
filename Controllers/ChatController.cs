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

//[ApiController]
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
        ViewBag.CurrentUserName = currentUser.UserName;

        var messages = await _tweetRepo.ChatMessages
            .Where(m => (m.SenderId == currentUser.Id && m.RecipientId == id) ||
                        (m.SenderId == id && m.RecipientId == currentUser.Id))
            .Include(m => m.Sender)
            .OrderBy(m => m.Timestamp)
            .ToListAsync();

        return View(messages);
    }

    [HttpPost("api/messages")]
    public async Task<IActionResult> CreateMessage([FromBody] ChatMessageDto chatMessageDto)
    {
        //var senderId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        var sender = await _userManager.GetUserAsync(User);
        if (sender == null) return Challenge();  // Not logged in

        //Console.WriteLine("recipient: " + chatMessageDto.RecipientId + " content: " + chatMessageDto.Content);

        var message = new ChatMessage
        {
            SenderId = sender.Id,
            RecipientId = chatMessageDto.RecipientId,
            Content = chatMessageDto.Content,
            Timestamp = DateTime.Now
        };

        _tweetRepo.ChatMessages.Add(message);
        await _tweetRepo.SaveChangesAsync();

        return Ok(new { Success = true });
    }
}