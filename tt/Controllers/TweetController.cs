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

namespace TwitterClone.Controllers;


public class TweetController : Controller
{
    private readonly TwitterContext _tweetRepo;
    private readonly IUserService _userService;
    private readonly INotificationService _notificationService;
    private readonly ITweetService _tweetService;
    private readonly IHashtagService _hashtagService;

    public TweetController(TwitterContext db,
                            IUserService userService,
                            INotificationService notificationService,
                            ITweetService tweetService,
                            IHashtagService hashtagService)
    {
        _tweetRepo = db;
        _userService = userService;
        _notificationService = notificationService;
        _tweetService = tweetService;
        _hashtagService = hashtagService;
    }

    /// <summary>
    ///    Create a new tweet and notify followers of the user
    ///    also parse hashtags and create them if they dont exist
    /// </summary>
    /// <param name="username"></param>
    /// <param name="tweet"></param>
    /// <returns></returns>
    [HttpPost]
    [Authorize]
    public async Task<IActionResult> Create(string username, string tweet)
    {

        var user = await _userService.GetUserAsync(User);
        if(user == null || string.IsNullOrEmpty(tweet))
        {
            return Json(new { success = false, redirectUrl = Url.Action("Index", "Home") });
        }

        var (hashtags, tweetContent) = _hashtagService.ParseHashtags(tweet);

        var newTweet = await _tweetService.CreateTweetAsync(user, tweetContent);

        if(hashtags.Count != 0)
        {
            await _hashtagService.CreateHashtagsAsync(hashtags, newTweet.Id);
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


    /// <summary>
    ///     Delete a tweet if the user is the owner of the tweet
    /// </summary>
    /// <param name="tweetId"></param>
    /// <returns></returns>
    [HttpPost]
    [Authorize]
    public async Task<IActionResult> Delete(int tweetId)
    {

        var currentUser = await _userService.GetUserAsync(User);
        if(currentUser == null)
        {
            return BadRequest();
        }
        var result = await _tweetService.DeleteAsync(currentUser.Id, tweetId);

        if(!result)
        {
            return BadRequest();
        }

        return RedirectToAction("Index", "Home");
    }

    /// <summary>
    ///     Like a tweet by adding a TweetLike relationship to the database
    /// </summary>
    /// <param name="tweetId"></param>
    /// <returns></returns>
    [HttpPost]
    [Authorize]
    public async Task<IActionResult> Like(int tweetId)
    {
        var user = await _userService.GetUserAsync(User);
        if(user == null)
        {
            return Json(new { success = false, action = "like failed" });
        }
        var userId = user.Id;

        var like = await _tweetService.LikeTweetAsync(userId, tweetId);

        if(!like)
        {
            return Json(new { success = false, action = "like failed" });
        }
        return Json(new { success = true, action = "liked" });
    }

    /// <summary>
    ///     Unlike a tweet by removing the TweetLike relationship from the database
    /// </summary>
    /// <param name="tweetId"></param>
    /// <returns></returns>
    [HttpPost]
    [Authorize]
    public async Task<IActionResult> Unlike(int tweetId)
    {
        var user = await _userService.GetUserAsync(User);
        if(user == null)
        {
            return Json(new { success = false, action = "unlike failed" });
        }
        var userId = user.Id;

        var unline = await _tweetService.UnlikeTweetAsync(userId, tweetId);

        if (!unline)
        {
            return Json(new { success = false, action = "unlike failed" });
        }

        return Json(new { success = true, action = "unliked" });
    }

    /// <summary>
    ///     Show all users that have liked a tweet
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    public async Task<IActionResult> ShowLikes(int id)
    {
        var likers = await _tweetService.ShowLikesAsync(id);

        var likes = likers?.ToList() ?? new List<ApplicationUser>();

        return View(likes);
    }

    /// <summary>
    ///     Reply to a tweet by creating a new tweet with the parent tweet id = ParentTweetId
    /// </summary>
    /// <param name="ParentTweetId"></param>
    /// <param name="Content"></param>
    /// <returns></returns>
    [HttpPost]
    [Authorize]
    public async Task<IActionResult> Reply(int ParentTweetId, string Content)
    {
        if (string.IsNullOrEmpty(Content))
        {
            return RedirectToAction("Index", "Home");
        }

        var currentUser = await _userService.GetUserAsync(User);
        if(currentUser == null)
        {
            return NotFound();
        }
        var tweet = await _tweetService.ReplyToTweetAsync(ParentTweetId, Content, currentUser);
        if(tweet == null)
        {
            return NotFound();
        }

        string referer = Request.Headers["Referer"].ToString();
        if (!string.IsNullOrEmpty(referer))
        {
            return Redirect(referer);
        }

        return RedirectToAction("Index", "Home");
    }

    /// <summary>
    ///     Show all replies to a tweet
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    public async Task<IActionResult> ViewReplies(int id)
    {
        var (parentTweet, replies) = await _tweetService.ViewRepliesAndParentAsync(id);

        if(parentTweet == null)
        {
            return NotFound();
        }

        replies ??= new List<Tweet>();

        return View(Tuple.Create(parentTweet, replies));
    }


    /// <summary>
    ///     Bookmark a tweet by adding a TweetBookmark relationship to the database
    /// </summary>
    /// <param name="tweetId"></param>
    /// <param name="isBookmarked"></param>
    /// <returns></returns>
    [HttpPost]
    public async Task<IActionResult> Bookmark(int tweetId, bool isBookmarked)
    {
        var currentUser = await _userService.GetUserAsync(User);
        if(currentUser == null)
        {
            return Json(new { success = false, action = "bookmark failed" });
        }

        var bookmarkChangeResult = await _tweetService.BookmarkTweetAsync(tweetId, currentUser.Id, isBookmarked);

        if(!bookmarkChangeResult)
        {
            if(isBookmarked)
            {
                return Json(new { success = true, action = "Tweet already bookmarked" });
            }
            else
            {
                return Json(new { success = true, action = "Tweet not bookmarked" });
            }
        }
        return Ok(new { Success = true });
    }


    /// <summary>
    ///     Retweet a tweet by adding a TweetRetweet relationship to the database
    /// </summary>
    /// <param name="tweetId"></param>
    /// <param name="isRetweet"></param>
    /// <returns></returns>
    [HttpPost]
    [Authorize]
    public async Task<IActionResult> Retweet(int tweetId, bool isRetweet)
    {
        var currentUser = await _userService.GetUserAsync(User);
        if(currentUser == null)
        {
            return Json(new { success = false, action = "User not logged in" });
        }
        var currentUserId = currentUser.Id;

        bool retweetResult;
        try {
            retweetResult = await _tweetService.Retweet(tweetId, currentUserId, isRetweet);
        }
        catch (Exception e)
        {
            return Json(new { success = false, action = "Tweet doesnt exist" });
        }

        if (!retweetResult)
        {
            if (isRetweet)
            {
                return Json(new { success = false, action = "Tweet already retweeted" });
            }
            else
            {
                return Json(new { success = false, action = "Tweet not retweeted" });
            }
        }

        return Ok(new { Success = true });
    }
}