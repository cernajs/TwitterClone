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
    private readonly IUserService _userService;

    public ChatController(ILogger<ChatController> logger, TwitterContext db,
                            IHubContext<NotificationHub> hubContext,
                            IUserService userService)
    {
        _logger = logger;
        _tweetRepo = db;
        _userService = userService;
    }

    public async Task<IActionResult> Index()
    {
        var currentUser = await _userService.GetUserAsync(User);
        var allChats = await _tweetRepo.ChatMessages
            .Where(m => m.SenderId == currentUser.Id || m.RecipientId == currentUser.Id)
            .GroupBy(m => m.SenderId == currentUser.Id ? m.RecipientId : m.SenderId)
            .Select(g => new ChatViewModel {
                RecieventName = g.Key == currentUser.Id ? g.FirstOrDefault().Sender.UserName : g.FirstOrDefault().Recipient.UserName,
                ChatId = g.Key,
                MostRecentMessage = g.OrderByDescending(m => m.Timestamp).FirstOrDefault()
            })
            .ToListAsync();

        return View(allChats);
    }


   [ActionName("ChatWithSpecificUser")]
    public async Task<IActionResult> ChatWithSpecificUserAsync(string id)
    {
        var currentUser = await _userService.GetUserAsync(User);
        ViewBag.CurrentUserId = currentUser.Id;
        ViewBag.OtherUserId = id;
        ViewBag.CurrentUserName = currentUser.UserName;

        var messages = await _tweetRepo.ChatMessages
            .Where(m => (m.SenderId == currentUser.Id && m.RecipientId == id) ||
                        (m.SenderId == id && m.RecipientId == currentUser.Id))
            .Include(m => m.Sender)
            .OrderBy(m => m.Timestamp)
            .ToListAsync();

        Console.WriteLine("messages retrieved");

        return View(messages);
    }


    public async Task<IActionResult> CreateMessage([FromBody] ChatMessageDto chatMessageDto)
    {
        var sender = await _userService.GetUserAsync(User);
        if (sender == null) return Challenge();

        var message = new ChatMessageBuilder()
            .WithSenderId(sender.Id)
            .WithRecipientId(chatMessageDto.RecipientId)
            .WithContent(chatMessageDto.Content)
            .WithTimestamp(DateTime.Now)
            .Build();

        _tweetRepo.ChatMessages.Add(message);
        await _tweetRepo.SaveChangesAsync();

        return Ok(new { Success = true });
    }
}