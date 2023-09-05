using System.Security.AccessControl;
using Internal;
using System;
using Microsoft.AspNetCore.Identity;
using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;

using TwitterClone.Data;
using TwitterClone.Models;
using TwitterClone.Hubs;
using TwitterClone.SD;


namespace TwitterClone.Controllers;

public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;
    private readonly TwitterContext _tweetRepo;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly ITweetRetrievalStrategy _viewStrategy;
    private readonly IPopularTweetStrategy _popularTweetStrategy;
    private readonly INotificationService _notificationService;

    public HomeController(ILogger<HomeController> logger, TwitterContext db,
                         UserManager<ApplicationUser> userManager,
                         ITweetRetrievalStrategy viewStrategy,
                         IPopularTweetStrategy popularTweetStrategy,
                         INotificationService notificationService)
    {
        _logger = logger;
        _tweetRepo = db;
        _userManager = userManager;
        _viewStrategy = viewStrategy;
        _popularTweetStrategy = popularTweetStrategy;
        _notificationService = notificationService;
    }

    /// <summary>
    ///     Retrieve tweets for home page given the strategy.
    /// </summary>
    /// <returns></returns>  
    public async Task<IActionResult> Index()
    {
        var tweets = await _viewStrategy.GetTweetsAsync(userId : null);

        return View(tweets);
    }

    /// <summary>
    ///     Search for tweets by username or hashtag depending on the search query.
    /// </summary>
    /// <param name="searchQuery"></param>
    /// <returns></returns>
    public async Task<IActionResult> Search(string searchQuery)
    {
        ISearchStrategy searchStrategy;

        if (!string.IsNullOrEmpty(searchQuery)) {
            if (searchQuery.StartsWith("#"))
            {
                Console.WriteLine("searching by hashtag");
                searchStrategy = new HashtagSearch();
            }
            else
            {
                searchStrategy = new UsernameSearch();
            }
            var tweets = await searchStrategy.SearchAsync(searchQuery, _tweetRepo);
            return View("Index", tweets);
        }
        else {
            var tweets = _tweetRepo.Tweets.Include(t => t.User).ToList();
            return View("Index", tweets);
        }
        
    }

    /// <summary>
    ///     Retrieve tweets for popular page given the strategy.
    /// </summary>
    /// <returns></returns>  
    [Authorize]
    public async Task<IActionResult> Popular() {
        var popularTweets = await _popularTweetStrategy.GetTweetsAsync();

        popularTweets ??= new List<Tweet>();

        return View(popularTweets);
    }

    /// <summary>
    ///     Show notifications for the current user.
    /// </summary>
    /// <returns></returns>  
    [Authorize]
    public async Task<IActionResult> ShowNotifications()
    {
		var user = await _userManager.GetUserAsync(User);
		if (user == null) return Challenge();

        var filteredNotification = await _notificationService.ShowNotificationsAsync(user.Id);


        return View(filteredNotification);
	}

    public IActionResult Privacy()
    {
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
