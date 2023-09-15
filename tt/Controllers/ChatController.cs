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
using TwitterClone.Interfaces;



namespace TwitterClone.Controllers;


[Authorize]
public class ChatController : Controller
{
    private readonly ILogger<ChatController> _logger;
    private readonly TwitterContext _tweetRepo;
    private readonly IUserService _userService;
    private readonly IChatService _chatService;
    private readonly INotificationService _notificationService;

    public ChatController(ILogger<ChatController> logger,
                            TwitterContext db,
                            IUserService userService,
                            IChatService chatService,
                            INotificationService notificationService)
    {
        _logger = logger;
        _tweetRepo = db;
        _userService = userService;
        _chatService = chatService;
        _notificationService = notificationService;
    }

    /// <summary>
    ///     Returns a view with all chats of the current user
    /// </summary>
    /// <returns></returns>
    public async Task<IActionResult> Index()
    {
        var currentUser = await _userService.GetUserAsync(User);

        if (currentUser == null) return Challenge();

        var allChats = await _chatService.GetChatsOfSpecificUserAsync(currentUser.Id);

        allChats ??= new List<ChatViewModel>();

        return View(allChats);
    }

    /// <summary>
    ///     Return a view with all messages between the current user and the user with the id
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    [ActionName("ChatWithSpecificUser")]
    public async Task<IActionResult> ChatWithSpecificUserAsync(string id)
    {
        var currentUser = await _userService.GetUserAsync(User);

        if (currentUser == null) return Challenge();

        var messages = await _chatService.GetMessagesBetweenUsersAsync(currentUser.Id, id);

        messages ??= new List<ChatMessage>();

        SpecificChatViewModel viewModel = new SpecificChatViewModel
        {
            currentUserId = currentUser.Id,
            otherUserId = id,
            currentUserName = currentUser.UserName,
            messages = messages
        };

        return View(viewModel);
    }

    /// <summary>
    ///     Create a message between currentUser
    ///     and recipient with content located in chatMessageDto
    ///     and add it to the database
    /// </summary>
    /// <param name="chatMessageDto"></param>
    /// <returns></returns>
    public async Task<IActionResult> CreateMessage([FromBody] ChatMessageDto chatMessageDto)
    {
        var sender = await _userService.GetUserAsync(User);
        if (sender == null) return Challenge();

        var result = await _chatService.CreateMessageAsync(chatMessageDto, sender.Id);

        await _notificationService.NotifyRecipientAsync(chatMessageDto, sender.Id);

        if (!result) return RedirectToAction("ChatWithSpecificUser", new { id = chatMessageDto.RecipientId });

        return Ok();
    }
}