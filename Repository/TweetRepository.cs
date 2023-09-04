// using TwitterClone.

// namespace TwitterClone.Data;

// public class TweetRepository : IRepository<Tweet>
// {
//     private readonly ApplicationDbContext _context;

//     public TweetRepository(ApplicationDbContext context)
//     {
//         _context = context;
//     }

//     public async Task<Tweet> GetByIdAsync(string id)
//     {
//         return await _context.Tweets
//             .Include(t => t.User)
//             .Include(t => t.ParentTweet)
//             .Include(t => t.TweetHashtags)
//             .Include(t => t.Likes)
//             .Include(t => t.Bookmarks)
//             .Include(t => t.Retweets)
//             .Include(t => t.Replies)
//             .FirstOrDefaultAsync(t => t.Id == id);
//     }

//     public async Task<IEnumerable<Tweet>> ListAllAsync()
//     {
//         return await _context.Tweets
//             .Include(t => t.User)
//             .Include(t => t.ParentTweet)
//             .Include(t => t.TweetHashtags)
//             .Include(t => t.Likes)
//             .Include(t => t.Bookmarks)
//             .Include(t => t.Retweets)
//             .Include(t => t.Replies)
//             .ToListAsync();
//     }

//     public async Task<Tweet> AddAsync(Tweet entity)
//     {
//         await _context.Tweets.AddAsync(entity);
//         await _context.SaveChangesAsync();
//         return entity;
//     }

//     public async Task UpdateAsync(Tweet entity)
//     {
//         _context.Tweets.Update(entity);
//         await _context.SaveChangesAsync();
//     }

//     public async Task DeleteAsync(Tweet entity)
//     {
//         _context.Tweets.Remove(entity);
//         await _context.SaveChangesAsync();
//     }
// }