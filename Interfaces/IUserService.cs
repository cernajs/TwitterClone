using System.Security.Claims;

using TwitterClone.Models;

namespace TwitterClone.Data;

public interface IUserService
{
    public Task<ApplicationUser?> GetUserAsync(ClaimsPrincipal claimsPrincipal);

    public Task<ApplicationUser?> GetUserRelatedDataAsync(string userId);

    public Task<bool> FollowUserAsync(string followerId, string userIdToFollow);

    public Task<bool> UnfollowUserAsync(string followerId, string userIdToUnfollow);

    public Task<List<ApplicationUser>>? ShowUsersAsync(string userId, string type);

    public Task<EditProfileResult> EditUserProfileAsync(EditProfileViewModel model);

    public Task<List<Tweet>> GetBookmarksAsync(string userId);

    public Task<bool> RelogCurrrentUserAsync();
}