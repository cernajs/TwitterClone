using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TwitterClone.Data;

namespace TwitterClone.Controllers;

public class ApiController : Controller
{
    private readonly TwitterContext _tweetRepo;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly INotificationService _notificationService;
    private readonly IHomeService _homeService;

    public ApiController(TwitterContext db,
                        UserManager<ApplicationUser> userManager,
                        INotificationService notificationService,
                        IHomeService homeService)
    {
        _tweetRepo = db;
        _userManager = userManager;
        _notificationService = notificationService;
        _homeService = homeService;
    }

    /// <summary>
    ///     Update the notification count for the current user
    ///     when there is new notification.
    /// </summary>
    /// <returns></returns>
    [HttpGet("api/getNotificationCount")]
    public async Task<IActionResult> GetNotificationCount()
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null) return Challenge();

        var notificationCount = await _notificationService.GetNotificationCountAsync(user.Id);

        return Json(new { notificationCount });
    }

    /// <summary>
    ///     return the top 3 trending topics based on
    ///     implementation of IHomeService
    /// </summary>
    /// <returns></returns>
    [HttpGet("api/getTrendingTopics")]
    public async Task<IActionResult> GetTrendingTopics()
    {
        var trendingTopics = await _homeService.GetTrendingTopicsAsync();

        return Json(trendingTopics);
    }

    /// <summary>
    ///     return 3 follow suggestions based on
    ///     implementation of IHomeService
    /// </summary>
    /// <returns></returns>
    [HttpGet("/api/getFollowSuggest")]
    public async Task<IActionResult> GetFollowSuggest()
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null) return Json(new List<ApplicationUser>());

        var followSuggest = await _homeService.GetFollowSuggestAsync(user.Id);

        return Json(followSuggest);
    }
}

