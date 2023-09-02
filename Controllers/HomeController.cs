using System.Security.AccessControl;
using Internal;
using System;
using Microsoft.AspNetCore.Identity;
using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.SignalR;
using System.Text.RegularExpressions;

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
    private readonly IHubContext<NotificationHub> _hubContext;
    private readonly ITweetRetrievalStrategy _viewStrategy;

    public HomeController(ILogger<HomeController> logger, TwitterContext db,
                         UserManager<ApplicationUser> userManager, IHubContext<NotificationHub> hubContext,
                         ITweetRetrievalStrategy viewStrategy)
    {
        _logger = logger;
        _tweetRepo = db;
        _userManager = userManager;
        _hubContext = hubContext;
        _viewStrategy = viewStrategy;
    }


    public async Task<IActionResult> Index()
    {
        //retreive tweets by given strategy
        //var tweets = await _viewStrategy.GetTweetsAsync(userId : null);

        var tweets = await _tweetRepo.Tweets
            .Include(t => t.User)
            .Include(t => t.Likes)
            .Include(t => t.Bookmarks)
            .Include(t => t.Replies)
            .Include(t => t.Retweets)
            .Include(t => t.Replies).ToListAsync();
            
        return View(tweets);
    }

    public async Task<IActionResult> Search(string searchQuery)
    {
        // var tweets = _tweetRepo.Tweets.Include(t => t.User).ToList();

        // if (!string.IsNullOrEmpty(searchQuery))
        // {
        //     tweets = tweets.Where(tweet =>
        //         tweet.Username.Contains(searchQuery, StringComparison.OrdinalIgnoreCase) ||
        //         tweet.TweetContent.Contains(searchQuery, StringComparison.OrdinalIgnoreCase))
        //         .ToList();
        // }

        // return View("Index", tweets);

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

    [HttpPost]
    public async Task<IActionResult> Create(string username, string tweet)
    {
        Console.WriteLine("tweet is :" + tweet);

        var user = await _userManager.GetUserAsync(User);

        // Check if the user is logged in and tweet content is provided.
        if(user == null || string.IsNullOrEmpty(tweet))
        {
            return RedirectToAction("Index", "Home");
        }

        MatchCollection matches = Regex.Matches(tweet, @"#\w+");
        List<string> hashtags = matches.Cast<Match>().Select(match => match.Value).ToList();
        
        if(hashtags.Count != 0) 
        {
            tweet = StaticMethods.ConvertToHtmlWithClickableHashtags(tweet);
        }

        var newTweet = new Tweet
        { 
            UserId = user.Id, 
            Username = user.UserName, 
            TweetContent = tweet, 
            CreatedAt = DateTime.Now,
            User = user
        };

        _tweetRepo.Tweets.Add(newTweet);
        //_tweetRepo.SaveChanges();

        

        if(hashtags.Count != 0)
        {
            
            var tweetHashtags = new List<TweetHashtag>();
            foreach(var hashtag in hashtags)
            {
                
                var newHashtag = new Hashtag
                {
                    Tag = hashtag.Substring(1)
                };
                _tweetRepo.Hashtags.Add(newHashtag);


                var newTweetHashtag = new TweetHashtag
                {
                    TweetId = newTweet.Id,
                    HashtagId = newHashtag.Id
                };
                tweetHashtags.Add(newTweetHashtag);
            }
            _tweetRepo.TweetHashtags.AddRange(tweetHashtags);
            
        }

        await _tweetRepo.SaveChangesAsync();

        await NotifyFollowersOfNewTweet(user.Id, "New tweet posted!");

                                                            //( id,         username,       content, createdAt,           likesCount,            userId,   isLikedByCurrentUser)
        await _hubContext.Clients.All.SendAsync("ReceiveTweet", newTweet.Id, user.UserName, tweet, DateTime.Now.ToString(), newTweet.Likes.Count, user.Id, false );

        //return RedirectToAction("Index", "Home");
        // CHANGE
        return Json(new { success = true });

    }


    // public async Task SendNotification(string userId, string message)
    // {
    //     await Clients.User(userId).SendAsync("ReceiveNotification", message);
    // }

    // public async Task SendTweet(string username, string content, string createdAt)
    // {
    //     await Clients.All.SendAsync("ReceiveTweet", username, content, createdAt);
    // }

    // public async Task SendMessage(string user, string message)
    // {
    //     await Clients.All.SendAsync("ReceiveMessage", user, message);
    // }

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
            //await _hubContext.SendNotification(follower, message);
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
