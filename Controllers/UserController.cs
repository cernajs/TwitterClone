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
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authentication;


using TwitterClone.Data;
using TwitterClone.Models;

namespace TwitterClone.Controllers;

public class UserController : Controller
{
    private readonly TwitterContext _tweetRepo;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly IUserService _userService;

    public UserController(TwitterContext db,
                            UserManager<ApplicationUser> userManager,
                            SignInManager<ApplicationUser> signInManager,
                            IUserService userService)
    {
        _tweetRepo = db;
        _userManager = userManager;
        _signInManager = signInManager;
        _userService = userService;
    }

    /// <summary>
    ///     Get user profile page with related data
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    public async Task<IActionResult> Index(string id)
    {
        var user = await _userService.GetUserRelatedDataAsync(id);

        if (user == null)
        {
            return NotFound();
        }

        return View(user);
    }

    /// <summary>
    ///     create new follow relationship in database if it already doesnt exist
    /// </summary>
    /// <param name="userIdToFollow"></param>
    /// <returns></returns>
    [HttpPost]
    [Authorize]
    public async Task<IActionResult> Follow(string userIdToFollow)
    {
        if (string.IsNullOrEmpty(userIdToFollow))
        {
            return BadRequest("User ID cannot be empty.");
        }
        var userToFollow = await _userManager.FindByIdAsync(userIdToFollow);
        var currentUserId = _userManager.GetUserId(User);

        if (userToFollow == null)
        {
            return NotFound("User not found.");
        }

        if (currentUserId == userIdToFollow)
        {
            return BadRequest("You cannot follow yourself.");
        }

        var result = await _userService.FollowUserAsync(_userManager.GetUserId(User), userIdToFollow);

        if (!result)
        {
            return BadRequest("You are already following this user.");
        }

        return RedirectToAction("Index", new { id = userIdToFollow });
    }


    /// <summary>
    ///     remove follow relationship from database if it exists
    /// </summary>
    /// <param name="userIdToUnfollow"></param>
    /// <returns></returns>
    [HttpPost]
    [Authorize]
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

        var result = await _userService.UnfollowUserAsync(currentUserId, userIdToUnfollow);

        if(!result)
        {
            return BadRequest("You are not following this user.");
        }

        return RedirectToAction("Index", new { id = userIdToUnfollow });
    }

    /// <summary>
    ///     Show all users that the current user is following
    ///     or show all user that follows the current user
    ///     based on type parameter
    /// </summary>
    /// <param name="id"></param>
    /// <param name="type"></param>
    /// <returns></returns>
    public async Task<IActionResult> ShowUsers(string id, string type)
    {
        IQueryable<ApplicationUser> userQuery = await _userService.ShowUsersAsync(id, type);

        if(userQuery == null)
        {
            return NotFound();
        }

        var users = await userQuery.ToListAsync();

        users = users ?? new List<ApplicationUser>();

        return View(users);
    }

    /// <summary>
    ///     Edit currently logged in user profile
    /// </summary>
    /// <param name="model"></param>
    /// <returns></returns>
    [HttpPost]
    [Authorize]
    public async Task<IActionResult> EditProfile([FromBody] EditProfileViewModel model)
    {
        Console.WriteLine("EditProfile " + model.Id + " " + model.UserName + " " + model.Email);
        
        if (ModelState.IsValid)
        {
            var result = await _userService.EditUserProfileAsync(model);

            if (result.Success)
            {
                await HttpContext.SignOutAsync(IdentityConstants.ApplicationScheme);

                // get any new claims that have been added to the user
                var user = await _userManager.GetUserAsync(HttpContext.User);
                
                // sign user back in
                await _signInManager.SignInAsync(user, isPersistent: false);

                return Json(new { success = true, action = "profile edited" });
            }
            else
            {
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError("", error);
                }
                return BadRequest(ModelState);
            }
        }
        return BadRequest(ModelState);
    }

    /// <summary>
    ///     Show all tweets that the current user has bookmarked
    /// </summary>
    /// <returns></returns>
    [Authorize]
    public async Task<IActionResult> ShowBookmarks()
    {
        var id = _userManager.GetUserId(User); 

        var bookmarks = await _userService.GetBookmarksAsync(id);

        bookmarks ??= new List<Tweet>();

        return View(bookmarks);
    }
}