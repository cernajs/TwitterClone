
using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using TwitterClone.Data;
using tt.Models;

namespace tt.Controllers;

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
        //return View();
        //var tweets = _tweetRepo.Tweets.ToList();
        var tweets = new List<Tweet> { new Tweet{ Id = 1, Username = "test", TweetContent = "test" } };
        return View( new TweetViewModel { Tweets = tweets , NewTweet = new Tweet{ Id = 2 } } );
    }

    // public IActionResult Create()
    // {
    //     Console.WriteLine("Create Tweeeeeet");
    //     return View();
    // }

    [HttpPost]
    public IActionResult Create(TweetViewModel tvmodel)
    {
        Console.WriteLine("Create Tweet");
        // if (tweet == null)
        // {
        //     return View();
        // }
        Console.WriteLine(tvmodel);

        // if (ModelState.IsValid)
        // {
        //     _tweetRepo.Tweets.Add(tweet);
        //     _tweetRepo.SaveChanges();

        //     var tweets = _tweetRepo.Tweets.ToList();
        //     foreach(var t in tweets)
        //     {
        //         Console.WriteLine(t.Username);
        //         Console.WriteLine(t.TweetContent);
        //     }

        //     //TempData["message"] = "Tweet has been added";
        //     return RedirectToAction("Index", "Home");
        // }

        //return View();
        return RedirectToAction("Index", "Home");
    }

    public IActionResult Privacy()
    {
        return View();
        // var tweets = _tweetRepo.Tweets.ToList();
        // return View("Index",tweets);
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
