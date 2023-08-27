using System;
using Microsoft.AspNetCore.Identity;
using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.SignalR;

using TwitterClone.Data;
using TwitterClone.Models;
using TwitterClone.Hubs;

namespace TwitterClone.Controllers;

public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;
    private readonly TwitterContext _tweetRepo;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IHubContext<NotificationHub> _hubContext;
    
    //private readonly IViewStrategy _viewStrategy;

    public HomeController(ILogger<HomeController> logger, TwitterContext db, UserManager<ApplicationUser> userManager, IHubContext<NotificationHub> hubContext)
    {
        _logger = logger;
        _tweetRepo = db;
        _userManager = userManager;
        _hubContext = hubContext;
        //_viewStrategy = viewStrategy;
    }


    public async Task<IActionResult> Index()
    {
        //retreive tweets by given strategy
        //var tweets = _viewStrategy.GetTweets();

        var tweets = await _tweetRepo.Tweets
            .Include(t => t.User)
            .Include(t => t.Likes)
            .Include(t => t.Replies).ToListAsync();
        return View(tweets);
    }

    public IActionResult Search(string searchQuery)
    {
        var tweets = _tweetRepo.Tweets.Include(t => t.User).ToList();

        if (!string.IsNullOrEmpty(searchQuery))
        {
            tweets = tweets.Where(tweet =>
                tweet.Username.Contains(searchQuery, StringComparison.OrdinalIgnoreCase) ||
                tweet.TweetContent.Contains(searchQuery, StringComparison.OrdinalIgnoreCase))
                .ToList();
        }

        return View("Index", tweets);
    }

    [HttpPost]
    public async Task<IActionResult> Create(string username, string tweet)
    {
        Console.WriteLine("tweet is :" + tweet);
        // Get the logged-in user.
        var user = await _userManager.GetUserAsync(User);

        // Check if the user is logged in and tweet content is provided.
        if(user == null || string.IsNullOrEmpty(tweet))
        {
            return RedirectToAction("Index", "Home");
        }

        // Create and save the new tweet.
        var newTweet = new Tweet 
        { 
            UserId = user.Id, 
            Username = user.UserName, 
            TweetContent = tweet, 
            CreatedAt = DateTime.Now,
            User = user
        };

        _tweetRepo.Tweets.Add(newTweet);
        _tweetRepo.SaveChanges();

        await NotifyFollowersOfNewTweet(user.Id, "New tweet posted!");

                                                            //( id,         username,       content, createdAt,           likesCount,            userId,   isLikedByCurrentUser)
        await _hubContext.Clients.All.SendAsync("ReceiveTweet", newTweet.Id, user.UserName, tweet, DateTime.Now.ToString(), newTweet.Likes.Count, user.Id, false );

        //return RedirectToAction("Index", "Home");
        // CHANGE
        return Json(new { success = true });

    }

    private async Task NotifyFollowersOfNewTweet(string userId, string message)
    {
        // Fetch all followers of the user.
        var followers = await _tweetRepo.UserFollowers
            .Where(uf => uf.FollowingId == userId)
            .Select(uf => uf.FollowerId)
            .ToListAsync();

        foreach(var follower in followers)
        {
            await _hubContext.Clients.User(follower).SendAsync("ReceiveNotification", message);
        }
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
