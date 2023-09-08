using System.Collections.Generic;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace TwitterClone.Models;

public class TweetLike
{
    public string UserId { get; set; }
    public ApplicationUser User { get; set; }
    
    public int TweetId { get; set; }
    public Tweet Tweet { get; set; }
    
    public DateTime LikedAt { get; set; }
}
