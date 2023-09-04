using System.Security.Claims;

namespace TwitterClone.Data;

public interface IUserService
{
    public Task<ApplicationUser> GetUserAsync(ClaimsPrincipal claimsPrincipal);
}