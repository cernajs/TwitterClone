using System.Collections.Generic;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace TwitterClone.Data
{
    public class ApplicationUser : IdentityUser
    {
        public virtual ICollection<Tweet> Tweets { get; set; } = new List<Tweet>();

        public virtual ICollection<UserFollower> Followers { get; set; } = new List<UserFollower>();
        public virtual ICollection<UserFollower> Following { get; set; } = new List<UserFollower>();


        //likes
        public virtual ICollection<TweetLike> LikedTweets { get; set; } = new List<TweetLike>();
    }
}