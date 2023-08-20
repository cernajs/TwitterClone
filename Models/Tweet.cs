//using EntityFrameworkCore.InMemory;
//using System.Data.Entity.Core;
using System.ComponentModel.DataAnnotations;

namespace TwitterClone.Data;

public class Tweet
{
    [Key]
    [Required]
    public int Id { get; set; }
    [Required]
    //[MaxLength(50)]
    public string Username { get; set; }
    [Required]
    //[MaxLength(280)]
    public string TweetContent { get; set; }
    public DateTime CreatedAt { get; set; }

    public override string ToString()
    {
        return $"Username: {Username}, TweetContent: {TweetContent}";
    }

    // User reference
    public string UserId { get; set; }
    public virtual ApplicationUser User { get; set; }

}