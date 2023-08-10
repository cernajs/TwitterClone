using Microsoft.EntityFrameworkCore;

namespace TwitterClone.Data
{
    public class TwitterContext : DbContext
    {
        public TwitterContext(DbContextOptions<TwitterContext> options) : base(options)
        {
        }

        // protected override void OnModelCreating(ModelBuilder modelBuilder) {
        //     modelBuilder.Entity.
        // }

        public DbSet<Tweet> Tweets { get; set; }
    }
}