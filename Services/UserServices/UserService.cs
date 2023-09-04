using System.Security.Claims;
using Microsoft.AspNetCore.Identity;

namespace TwitterClone.Data;

public class UserService : IUserService
{
    private readonly TwitterContext _tweetRepo;
    private readonly UserManager<ApplicationUser> _userManager;

    public UserService(TwitterContext db, UserManager<ApplicationUser> userManager)
    {
        _tweetRepo = db;
        _userManager = userManager;
    }

    public async Task<ApplicationUser> GetUserAsync(ClaimsPrincipal claimsPrincipal)
    {
        return await _userManager.GetUserAsync(claimsPrincipal);
    }
}