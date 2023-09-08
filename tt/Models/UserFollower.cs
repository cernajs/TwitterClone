namespace TwitterClone.Models;

public class UserFollower
{
    // Follower
    public string FollowerId { get; set; }
    public virtual ApplicationUser Follower { get; set; }

    // Following
    public string FollowingId { get; set; }
    public virtual ApplicationUser Following { get; set; }
}