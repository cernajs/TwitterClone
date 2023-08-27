using Internal;
using System;
using Microsoft.AspNetCore.Identity;
using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TwitterClone.Data;
using TwitterClone.Models;

namespace TwitterClone.Controllers;

public class TweetController : Controller
{
    private readonly ILogger<HomeController> _logger;
    private readonly TwitterContext _tweetRepo;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly SignInManager<ApplicationUser> _signInManager;

    public TweetController(ILogger<HomeController> logger, TwitterContext db, UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signInManager)
    {
        _logger = logger;
        _tweetRepo = db;
        _userManager = userManager;
        _signInManager = signInManager;
    }

    [HttpPost]
    public async Task<IActionResult> Create(string tweetContent)
    {
        if (string.IsNullOrEmpty(tweetContent))
        {
            return RedirectToAction("Index", "Home");
        }

        var currentUser = await _userManager.GetUserAsync(User);

        var newTweet = new Tweet { TweetContent = tweetContent, CreatedAt = DateTime.Now, User = currentUser };
        _tweetRepo.Tweets.Add(newTweet);
        _tweetRepo.SaveChanges();

        return RedirectToAction("Index", "Home");
    }

    [HttpPost]
    public async Task<IActionResult> Delete(int tweetId)
    {
        var tweet = _tweetRepo.Tweets.FirstOrDefault(t => t.Id == tweetId);
        if (tweet == null)
        {
            return NotFound();
        }

        var currentUser = await _userManager.GetUserAsync(User);
        if (tweet.User != currentUser)
        {
            return Unauthorized();
        }

        _tweetRepo.Tweets.Remove(tweet);
        _tweetRepo.SaveChanges();

        return RedirectToAction("Index", "Home");
    }

    [HttpPost]
    //[Authorize]
    public async Task<IActionResult> Like(int tweetId)
    {
        var userId = _userManager.GetUserId(User);
        var existingLike = await _tweetRepo.TweetLikes
            .FirstOrDefaultAsync(l => l.TweetId == tweetId && l.UserId == userId);

        var tweet = _tweetRepo.Tweets.FirstOrDefault(t => t.Id == tweetId);
        Console.WriteLine("tweet is :" + tweet);

        if (existingLike == null)
        {
            Console.WriteLine("existingLike is : null");
            var like = new TweetLike
            {
                TweetId = tweetId,
                UserId = userId,
                LikedAt = DateTime.Now
            };
            _tweetRepo.TweetLikes.Add(like);
            await _tweetRepo.SaveChangesAsync();

            return Json(new { success = true, action = "liked" });
        }

        // string referer = Request.Headers["Referer"].ToString();
        // if (!string.IsNullOrEmpty(referer))
        // {
        //     return Redirect(referer);
        // }

        //return RedirectToAction("Index", "Home");
        return Json(new { success = false, action = "like failed" });
    }

    [HttpPost]
    public async Task<IActionResult> Unlike(int tweetId)
    {
        var tweet = _tweetRepo.Tweets.FirstOrDefault(t => t.Id == tweetId);
        if (tweet == null)
        {
            //return NotFound();
            return Json(new { success = false, action = "unlike failed" });
        }

        var userId = _userManager.GetUserId(User);
        var existingLike = await _tweetRepo.TweetLikes
            .FirstOrDefaultAsync(l => l.TweetId == tweetId && l.UserId == userId);

        if (existingLike == null)
        {
            //return NotFound();
            return Json(new { success = false, action = "unlike failed" });
        }

        _tweetRepo.TweetLikes.Remove(existingLike);
        await _tweetRepo.SaveChangesAsync();

        // string referer = Request.Headers["Referer"].ToString();
        // if (!string.IsNullOrEmpty(referer))
        // {
        //     return Redirect(referer);
        // }

        // return RedirectToAction("Index", "Home");

        return Json(new { success = true, action = "unliked" });
    }

    public async Task<IActionResult> ShowLikes(int id)
    {
        // <a asp-controller="Tweet" asp-action="ShowLikes" asp-route-id="@tweet.Id">
        //      # of likes : @tweet.Likes.Count
        // </a>

        // var tweet = await _tweetRepo.Tweets
        //     .Include(t => t.Likes)
        //         .ThenInclude(l => l.User)
        //     .FirstOrDefaultAsync(t => t.Id == id);

        var likers = await _tweetRepo.Tweets
            .Where(t => t.Id == id)
            .SelectMany(t => t.Likes.Select(l => l.User))
            .ToListAsync();

        if (likers == null)
        {
            return NotFound();
        }

        var likes = likers?.ToList() ?? new List<ApplicationUser>();

        return View(likes);
    }

    //combine with Create
    [HttpPost]
    public async Task<IActionResult> Reply(int ParentTweetId, string Content)
    {
        if (string.IsNullOrEmpty(Content))
        {
            return RedirectToAction("Index", "Home");
        }
        if(_signInManager.IsSignedIn(User) == false)
        {
            return RedirectToAction("Index", "Home");
        }

        var currentUser = await _userManager.GetUserAsync(User);
        var parentTweet = _tweetRepo.Tweets.FirstOrDefault(t => t.Id == ParentTweetId);
        if (parentTweet == null)
        {
            return NotFound();
        }

        var newTweet = new Tweet
        {
            UserId = currentUser.Id,
            Username = currentUser.UserName,
            TweetContent = Content,
            CreatedAt = DateTime.Now,
            ParentTweetId = ParentTweetId,
            ParentTweet = parentTweet
        };

        _tweetRepo.Tweets.Add(newTweet);
        _tweetRepo.SaveChanges();

        return RedirectToAction("Index", "Home");
    }
}