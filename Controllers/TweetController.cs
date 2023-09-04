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


public class TweetController : Controller
{
    private readonly TwitterContext _tweetRepo;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly IHubContext<NotificationHub> _hubContext;
    private readonly IUserService _userService;
    private readonly INotificationService _notificationService;
    private readonly ITweetService _tweetService;
    private readonly IHashtagService _hashtagService;

    public TweetController(TwitterContext db,
                            UserManager<ApplicationUser> userManager,
                            SignInManager<ApplicationUser> signInManager,
                            IHubContext<NotificationHub> hubContext,
                            IUserService userService,
                            INotificationService notificationService,
                            ITweetService tweetService,
                            IHashtagService hashtagService)
    {
        _tweetRepo = db;
        _userManager = userManager;
        _signInManager = signInManager;
        _hubContext = hubContext;
        _userService = userService;
        _notificationService = notificationService;
        _tweetService = tweetService;
        _hashtagService = hashtagService;
    }

    [HttpPost]
    [Authorize]
    public async Task<IActionResult> Create(string username, string tweet)
    {

        var user = await _userService.GetUserAsync(User);
        if(user == null || string.IsNullOrEmpty(tweet))
        {
            return RedirectToAction("Index", "Home");
        }

        var (hashtags, tweetContent) = _hashtagService.ParseHashtags(tweet);

        var newTweet = await _tweetService.CreateTweetAsync(user, tweetContent);

        if(hashtags.Count != 0)
        {
            _hashtagService.CreateHashtagsAsync(hashtags, newTweet.Id);
        }

        await _notificationService.NotifyFollowersOfNewTweetAsync(user.Id, "New tweet posted!", newTweet.Id);

        await _notificationService.SendTweetNotificationAsync(
            newTweet.Id, 
            user.UserName, 
            tweet, 
            DateTime.Now, 
            0, 
            user.Id
        );

        await _tweetRepo.SaveChangesAsync();

        return Json(new { success = true });
    }



    [HttpPost]
    [Authorize]
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
    [Authorize]
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

        return Json(new { success = false, action = "like failed" });
    }

    [HttpPost]
    [Authorize]
    public async Task<IActionResult> Unlike(int tweetId)
    {
        var tweet = _tweetRepo.Tweets.FirstOrDefault(t => t.Id == tweetId);
        if (tweet == null)
        {
            return Json(new { success = false, action = "unlike failed" });
        }

        var userId = _userManager.GetUserId(User);
        var existingLike = await _tweetRepo.TweetLikes
            .FirstOrDefaultAsync(l => l.TweetId == tweetId && l.UserId == userId);

        if (existingLike == null)
        {
            return Json(new { success = false, action = "unlike failed" });
        }

        _tweetRepo.TweetLikes.Remove(existingLike);
        await _tweetRepo.SaveChangesAsync();

        return Json(new { success = true, action = "unliked" });
    }

    public async Task<IActionResult> ShowLikes(int id)
    {
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

    [HttpPost]
    [Authorize]
    public async Task<IActionResult> Reply(int ParentTweetId, string Content)
    {
        if (string.IsNullOrEmpty(Content))
        {
            return RedirectToAction("Index", "Home");
        }

        var currentUser = await _userManager.GetUserAsync(User);
        var parentTweet = _tweetRepo.Tweets.FirstOrDefault(t => t.Id == ParentTweetId);
        if (parentTweet == null)
        {
            return NotFound();
        }

        var newTweet = new TweetBuilder()
            .WithUser(currentUser)
            .WithContent(Content)
            .WithCreatedAt(DateTime.Now)
            .WithParentTweetId(ParentTweetId)
            .WithParentTweet(parentTweet)
            .Build();

        _tweetRepo.Tweets.Add(newTweet);
        _tweetRepo.SaveChanges();

        string referer = Request.Headers["Referer"].ToString();
        if (!string.IsNullOrEmpty(referer))
        {
            return Redirect(referer);
        }

        return RedirectToAction("Index", "Home");
    }


    public async Task<IActionResult> ViewReplies(int id)
    {

        var replies = await _tweetRepo.Tweets
                                        .Include(t => t.ParentTweet)
                                        .Where(t => t.ParentTweetId == id).ToListAsync();

        if (replies == null)
        {
            return NotFound();
        }

        replies ??= new List<Tweet>();

        return View(replies);
    }

    [HttpPost]
    public async Task<IActionResult> Bookmark(int tweetId, bool isBookmarked)
    {
        var currentUser = await _userManager.GetUserAsync(User);

        if(isBookmarked)
        {
            if(_tweetRepo.TweetBookmarks.Any(tb => tb.UserId == currentUser.Id && tb.TweetId == tweetId))
            {
                return BadRequest("Tweet already bookmarked.");
            }

            var tweetBookmark = new TweetBookmark
            {
                UserId = currentUser.Id,
                TweetId = tweetId
            };

            _tweetRepo.TweetBookmarks.Add(tweetBookmark);
        }
        else
        {
            var tweetBookmark = await _tweetRepo.TweetBookmarks
                .Where(tb => tb.UserId == currentUser.Id && tb.TweetId == tweetId)
                .FirstOrDefaultAsync();

            if(tweetBookmark == null)
            {
                return BadRequest("Tweet not bookmarked.");
            }

            if (tweetBookmark != null)
            {
                _tweetRepo.TweetBookmarks.Remove(tweetBookmark);
            }
        }
        await _tweetRepo.SaveChangesAsync();

        return Ok(new { Success = true });
    }

    [HttpPost]
    public async Task<IActionResult> Retweet(int tweetId, bool isRetweet)
    {
        var currentUser = await _userManager.GetUserAsync(User);

        var tweet = await _tweetRepo.Tweets
            .FirstOrDefaultAsync(t => t.Id == tweetId);

        if (tweet == null)
        {
            return NotFound();
        }

        if(isRetweet)
        {
            if(_tweetRepo.Retweets.Any(r => r.UserId == currentUser.Id && r.TweetId == tweetId))
            {
                return BadRequest("Tweet already retweeted.");
            }
            var retweet = new Retweet {
                UserId = currentUser.Id,
                TweetId = tweetId,
                RetweetTime = DateTime.Now
            };

            _tweetRepo.Retweets.Add(retweet);
        }
        else
        {
            var retweet = await _tweetRepo.Retweets
                .Where(r => r.UserId == currentUser.Id && r.TweetId == tweetId)
                .FirstOrDefaultAsync();

            if(retweet == null)
            {
                return BadRequest("Tweet not retweeted.");
            }

            if (retweet != null)
            {
                _tweetRepo.Retweets.Remove(retweet);
            }
        }

        await _tweetRepo.SaveChangesAsync();

        return Ok(new { Success = true });
    }
}