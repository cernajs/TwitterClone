using System.Security.AccessControl;
using System.Net.Http;
using Internal;
using System.Net;
using System;
using Microsoft.AspNetCore.Identity;
using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;


using TwitterClone.Data;
using TwitterClone.Models;

namespace TwitterClone.Controllers;

public class UserController : Controller
{
    private readonly ILogger<HomeController> _logger;
    private readonly TwitterContext _tweetRepo;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly SignInManager<ApplicationUser> _signInManager;

    public UserController(ILogger<HomeController> logger, TwitterContext db, UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signInManager)
    {
        _logger = logger;
        _tweetRepo = db;
        _userManager = userManager;
        _signInManager = signInManager;
    }

    public async Task<IActionResult> Index(string id)
    {
        var user = await _tweetRepo.Users
                .Include(u => u.Followers)
                .Include(u => u.Following)
                .Include(u => u.Retweets)
                    .ThenInclude(r => r.Tweet)
                        .ThenInclude(t => t.User)
                .Include(u => u.Tweets)
                    .ThenInclude(t => t.Likes)
                .Include(u => u.LikedTweets).FirstOrDefaultAsync(u => u.Id == id);

        if (user == null)
        {
            return NotFound();
        }

        return View(user);
    }

    [HttpPost]
    //[Authorize]
    public async Task<IActionResult> Follow(string userIdToFollow)
    {
        if (string.IsNullOrEmpty(userIdToFollow))
        {
            return BadRequest("User ID cannot be empty.");
        }

        var currentUser = await _userManager.GetUserAsync(User);
        var userToFollow = await _userManager.FindByIdAsync(userIdToFollow);

        if (userToFollow == null)
        {
            return NotFound("User not found.");
        }

        if (currentUser.Id == userIdToFollow)
        {
            return BadRequest("You cannot follow yourself.");
        }

        if (_tweetRepo.UserFollowers.Any(uf => uf.FollowerId == currentUser.Id && uf.FollowingId == userIdToFollow))
        {
            return BadRequest("Already following this user.");
        }

        var userFollower = new UserFollower
        {
            FollowerId = currentUser.Id,
            FollowingId = userIdToFollow
        };

        _tweetRepo.UserFollowers.Add(userFollower);
        await _tweetRepo.SaveChangesAsync();

        return RedirectToAction("Index", new { id = userIdToFollow });
    }



    [HttpPost]
    public async Task<IActionResult> Unfollow(string userIdToUnfollow)
    {
        if (string.IsNullOrEmpty(userIdToUnfollow))
        {
            return BadRequest("User ID cannot be empty.");
        }
        var currentUserId = _userManager.GetUserId(User);

        if(currentUserId == userIdToUnfollow)
        {
            return BadRequest("You cannot unfollow yourself.");
        }

        // Find the following relationship
        var followingRelationship = await _tweetRepo.UserFollowers
            .FirstOrDefaultAsync(uf => uf.FollowerId == currentUserId && uf.FollowingId == userIdToUnfollow);

        if (followingRelationship == null)
        {
            return NotFound("Following relationship not found.");
        }

        // Remove the relationship
        _tweetRepo.UserFollowers.Remove(followingRelationship);
        await _tweetRepo.SaveChangesAsync();

        return RedirectToAction("Index", new { id = userIdToUnfollow });
    }


    public async Task<IActionResult> ShowUsers(string id, string type)
    {
        IQueryable<ApplicationUser> userQuery;

        if(type == "followers")
        {
            userQuery = _tweetRepo.Users
                                .Where(u => u.Id == id)
                                .SelectMany(u => u.Followers.Select(f => f.Follower));
        }
        else if(type == "followings")
        {
            userQuery = _tweetRepo.Users
                                .Where(u => u.Id == id)
                                .SelectMany(u => u.Following.Select(f => f.Following));
        }
        else
        {
            return BadRequest("Invalid type specified");
        }

        var users = await userQuery.ToListAsync();

        users = users ?? new List<ApplicationUser>();

        return View(users);
    }

    [HttpPost]
    public async Task<IActionResult> EditProfile([FromBody] EditProfileViewModel model)
    {
        Console.WriteLine("EditProfile " + model.Id + " " + model.UserName + " " + model.Email);
        if (ModelState.IsValid)
        {
            var user = await _userManager.FindByIdAsync(model.Id);
            if (user == null)
            {
                return NotFound("User not found");
            }

            var existingUser = await _userManager.FindByEmailAsync(model.Email);
            if (existingUser != null && existingUser.Id != model.Id)
            {
                return BadRequest("Email already exists.");
            }

            var tweets = await _tweetRepo.Tweets.Where(t => t.UserId == model.Id).ToListAsync();
            foreach (var tweet in tweets)
            {
                tweet.Username = model.UserName;
            }

            user.UserName = model.UserName;
            user.Email = model.Email;

            await _tweetRepo.SaveChangesAsync();

            var result = await _userManager.UpdateAsync(user);
            if (result.Succeeded)
            {
                // sign-out the user first
                await HttpContext.SignOutAsync(IdentityConstants.ApplicationScheme);

                // get any new claims that have been added to the user
                user = await _userManager.GetUserAsync(HttpContext.User);
                
                // sign user back in
                await _signInManager.SignInAsync(user, isPersistent: false);
                
                return Ok(new { Success = true });
            }
            else
            {
                return BadRequest(result.Errors);
            }
        }
        return BadRequest(ModelState);
    }

    public async Task<IActionResult> ShowBookmarks()
    {
        var id = _userManager.GetUserId(User); 

        var bookmarks = await _tweetRepo.TweetBookmarks
            .Where(b => b.UserId == id)
            .Include(b => b.Tweet)
            .ThenInclude(t => t.User)
            .Include(b => b.Tweet.Likes)
            .Include(b => b.Tweet.Bookmarks)
            .Include(b => b.Tweet.Retweets)
            .Include(b => b.Tweet.Replies)
            .Select(b => b.Tweet)
            .ToListAsync();

        if (bookmarks == null)
        {
            return NotFound();
        }

        //bookmarks ??= new List<Tweet>();

        return View(bookmarks);
    }
}