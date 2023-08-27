using Internal;
using System.Net;

using System;
using Microsoft.AspNetCore.Identity;
using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TwitterClone.Data;
using TwitterClone.Models;

namespace TwitterClone.Controllers;

public class UserController : Controller
{
    private readonly ILogger<HomeController> _logger;
    private readonly TwitterContext _tweetRepo;
    private readonly UserManager<ApplicationUser> _userManager;

    public UserController(ILogger<HomeController> logger, TwitterContext db, UserManager<ApplicationUser> userManager)
    {
        _logger = logger;
        _tweetRepo = db;
        _userManager = userManager;
    }

    public async Task<IActionResult> Index(string id)
    {
        var user = await _tweetRepo.Users
                .Include(u => u.Followers)
                .Include(u => u.Following)
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

        // currentUser.Following.Add(userFollower);
        // userToFollow.Followers.Add(userFollower);

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
            // Fetching the list of users who follow the given user
            userQuery = _tweetRepo.Users
                                .Where(u => u.Id == id)
                                .SelectMany(u => u.Followers.Select(f => f.Follower));
        }
        else if(type == "followings")
        {
            // Fetching the list of users whom the given user follows
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


        



    
}