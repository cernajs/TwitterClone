
using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using TwitterClone.Data;
using TwitterClone.Models;

namespace TwitterClone.Controllers;

public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;
    private readonly TwitterContext _tweetRepo;

    public HomeController(ILogger<HomeController> logger, TwitterContext db)
    {
        _logger = logger;
        _tweetRepo = db;
    }

    public IActionResult Index()
    {
        var tweets = _tweetRepo.Tweets.ToList();
        return View(tweets);
    }

    public IActionResult Search(string searchQuery)
    {
        var tweets = _tweetRepo.Tweets.ToList();

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
    public IActionResult Create(string username, string tweet)
    {

        if( username == null || tweet == null )
        {
            return RedirectToAction("Index", "Home");
        }

        var newTweet = new Tweet { Username = username, TweetContent = tweet, CreatedAt = DateTime.Now };
        _tweetRepo.Tweets.Add(newTweet);
        _tweetRepo.SaveChanges();

        return RedirectToAction("Index", "Home");
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
