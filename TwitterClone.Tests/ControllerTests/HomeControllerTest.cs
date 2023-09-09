using System.Security.Claims;
using Moq;
using Moq.EntityFrameworkCore;
using TwitterClone.Controllers;
using TwitterClone.Data;
using TwitterClone.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Xunit;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System.Diagnostics;

using TwitterClone.Data;
using Microsoft.AspNetCore.SignalR;
using TwitterClone.Hubs;
using TwitterClone.Data;
using TwitterClone.Models;

namespace TwitterClone.Tests.ControllerTests;
public class HomeControllerTest
{
    DbContextOptions<TwitterContext> options;
    Mock<ILogger<HomeController>> mockLogger;
    Mock<IUserStore<ApplicationUser>> mockUserStore;
    UserManager<ApplicationUser> userManager;
    Mock<INotificationService> mockNotificationService;
    Mock<IHubContext<NotificationHub>> mockHubContext;
    ApplicationUser fakeUser;
    Mock<IPopularTweetStrategy> mockPopularTweetStrategy;

    public void InitializeTest()
    {
        options = new DbContextOptionsBuilder<TwitterContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        mockLogger = new Mock<ILogger<HomeController>>();
        mockUserStore = new Mock<IUserStore<ApplicationUser>>();
        userManager = new UserManager<ApplicationUser>(
            mockUserStore.Object, null, null, null, null, null, null, null, null);
        mockNotificationService = new Mock<INotificationService>();

        mockHubContext = new Mock<IHubContext<NotificationHub>>();

        var fakeUserId = "1";
        fakeUser = new ApplicationUser { Id = fakeUserId };
        mockUserStore.Setup(x => x.FindByIdAsync(fakeUserId, CancellationToken.None))
                    .ReturnsAsync(fakeUser);

        mockPopularTweetStrategy = new Mock<IPopularTweetStrategy>();
    }
    [Fact]
    public async Task Index_ReturnsTweetsFromViewStrategy()
    {

        InitializeTest();

        using (var context = new TwitterContext(options))
        {
            var user1 = new ApplicationUser { Id = "1", UserName = "user1" };
            var user2 = new ApplicationUser { Id = "2", UserName = "user2" };
            var user3 = new ApplicationUser { Id = "3", UserName = "user3" };
            var user4 = new ApplicationUser { Id = "4", UserName = "user4" };

            context.Users.AddRange(user1, user2, user3, user4);

            context.Tweets.AddRange(
                new Tweet
                {
                    Id = 1,
                    TweetContent = "a",
                    UserId = "1",
                    User = user1,
                    Username = user1.UserName,
                    Likes = new List<TweetLike>(),
                    Bookmarks = new List<TweetBookmark>(),
                    Replies = new List<Tweet>(),
                    Retweets = new List<Retweet>()
                },
                new Tweet
                {
                    Id = 2,
                    TweetContent = "b",
                    UserId = "2",
                    User = user2,
                    Username = user2.UserName,
                    Likes = new List<TweetLike>(),
                    Bookmarks = new List<TweetBookmark>(),
                    Replies = new List<Tweet>(),
                    Retweets = new List<Retweet>()
                },
                new Tweet
                {
                    Id = 3,
                    TweetContent = "c",
                    UserId = "3",
                    User = user3,
                    Username = user3.UserName,
                    Likes = new List<TweetLike>(),
                    Bookmarks = new List<TweetBookmark>(),
                    Replies = new List<Tweet>(),
                    Retweets = new List<Retweet>()
                },
                new Tweet
                {
                    Id = 4,
                    TweetContent = "d",
                    UserId = "4",
                    User = user4,
                    Username = user4.UserName,
                    Likes = new List<TweetLike>(),
                    Bookmarks = new List<TweetBookmark>(),
                    Replies = new List<Tweet>(),
                    Retweets = new List<Retweet>()
                }
            );

            await context.SaveChangesAsync();


            var viewStrategy = new GetAllTweets(context);

            var controller = new HomeController(mockLogger.Object, context,
                                                userManager, viewStrategy,
                                                mockPopularTweetStrategy.Object, mockNotificationService.Object);

            var result = await controller.Index();

            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsAssignableFrom<IEnumerable<Tweet>>(viewResult.ViewData.Model);
            Assert.Equal(4, model.Count());
        }
    }

    [Fact]
    public async Task Search_ReturnsCorrectTweets()
    {
        InitializeTest();

        using (var context = new TwitterContext(options))
        {
            var user1 = new ApplicationUser { Id = "1", UserName = "user1" };
            var user2 = new ApplicationUser { Id = "2", UserName = "user2" };
            var user3 = new ApplicationUser { Id = "3", UserName = "user3" };
            var user4 = new ApplicationUser { Id = "4", UserName = "user4" };

            context.Users.AddRange(user1, user2, user3, user4);

            context.Tweets.AddRange(
                new Tweet
                {
                    Id = 1,
                    TweetContent = "a",
                    UserId = "1",
                    User = user1,
                    Username = user1.UserName,
                    Likes = new List<TweetLike>(),
                    Bookmarks = new List<TweetBookmark>(),
                    Replies = new List<Tweet>(),
                    Retweets = new List<Retweet>()
                },
                new Tweet
                {
                    Id = 2,
                    TweetContent = "b",
                    UserId = "2",
                    User = user2,
                    Username = user2.UserName,
                    Likes = new List<TweetLike>(),
                    Bookmarks = new List<TweetBookmark>(),
                    Replies = new List<Tweet>(),
                    Retweets = new List<Retweet>()
                },
                new Tweet
                {
                    Id = 3,
                    TweetContent = "#a #b",
                    UserId = "3",
                    User = user3,
                    Username = user3.UserName,
                    Likes = new List<TweetLike>(),
                    Bookmarks = new List<TweetBookmark>(),
                    Replies = new List<Tweet>(),
                    Retweets = new List<Retweet>()
                },
                new Tweet
                {
                    Id = 4,
                    TweetContent = "#a",
                    UserId = "4",
                    User = user4,
                    Username = user4.UserName,
                    Likes = new List<TweetLike>(),
                    Bookmarks = new List<TweetBookmark>(),
                    Replies = new List<Tweet>(),
                    Retweets = new List<Retweet>()
                }
            );

            var hashtag = new Hashtag { Id = 1, Tag = "a" };
            var tweetHashtag = new TweetHashtag { TweetId = 3, HashtagId = 1 };
            var tweetHashtag2 = new TweetHashtag { TweetId = 4, HashtagId = 1 };
            context.Hashtags.Add(hashtag);
            context.TweetHashtags.Add(tweetHashtag);
            context.TweetHashtags.Add(tweetHashtag2);

            await context.SaveChangesAsync();

            var controller = new HomeController(mockLogger.Object, context,
                                                userManager, new GetAllTweets(context),
                                                mockPopularTweetStrategy.Object, mockNotificationService.Object);

            // Case 1: Null or empty searchQuery
            var result = await controller.Search("");
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsAssignableFrom<IEnumerable<Tweet>>(viewResult.ViewData.Model);
            Assert.Equal(4, model.Count());

            // Case 2: searchQuery starts with #
            result = await controller.Search("#a");
            viewResult = Assert.IsType<ViewResult>(result);
            model = Assert.IsAssignableFrom<IEnumerable<Tweet>>(viewResult.ViewData.Model);
            Assert.Equal(2, model.Count());

            // Case 3: searchQuery does not start with #
            result = await controller.Search("user1");
            viewResult = Assert.IsType<ViewResult>(result);
            model = Assert.IsAssignableFrom<IEnumerable<Tweet>>(viewResult.ViewData.Model);
            Assert.Single(model);
        }
    }


    [Fact]
    public async Task Popular_ReturnsPopularTweets()
    {
        InitializeTest();

        using (var context = new TwitterContext(options))
        {
            var user1 = new ApplicationUser { Id = "1", UserName = "user1" };
            var user2 = new ApplicationUser { Id = "2", UserName = "user2" };
            var user3 = new ApplicationUser { Id = "3", UserName = "user3" };

            context.Users.AddRange(user1, user2, user3);

            context.Tweets.AddRange(
                new Tweet
                {
                    Id = 1,
                    TweetContent = "a",
                    UserId = "1",
                    User = user1,
                    Username = user1.UserName,
                    Likes = new List<TweetLike>(),
                    Bookmarks = new List<TweetBookmark>(),
                    Replies = new List<Tweet>(),
                    Retweets = new List<Retweet>()
                },
                new Tweet
                {
                    Id = 2,
                    TweetContent = "b",
                    UserId = "2",
                    User = user2,
                    Username = user2.UserName,
                    Likes = new List<TweetLike>(),
                    Bookmarks = new List<TweetBookmark>(),
                    Replies = new List<Tweet>(),
                    Retweets = new List<Retweet>()
                },
                new Tweet
                {
                    Id = 3,
                    TweetContent = "#a #b",
                    UserId = "3",
                    User = user3,
                    Username = user3.UserName,
                    Likes = new List<TweetLike>(),
                    Bookmarks = new List<TweetBookmark>(),
                    Replies = new List<Tweet>(),
                    Retweets = new List<Retweet>()
                }
            );

            var like = new TweetLike { TweetId = 1, UserId = "2" };
            var reply1 = new Tweet {
                Id = 5,
                TweetContent = "reply1",
                UserId = "2",
                User = user2,
                Username = user2.UserName,
                ParentTweetId = 2,
                Likes = new List<TweetLike>(),
                Bookmarks = new List<TweetBookmark>(),
                Replies = new List<Tweet>(),
                Retweets = new List<Retweet>() };
            var reply2 = new Tweet
            {
                Id = 6,
                TweetContent = "reply2",
                UserId = "3",
                User = user3,
                Username = user3.UserName,
                ParentTweetId = 2,
                Likes = new List<TweetLike>(),
                Bookmarks = new List<TweetBookmark>(),
                Replies = new List<Tweet>(),
                Retweets = new List<Retweet>()
            };

            context.TweetLikes.Add(like);
            context.Tweets.Add(reply1);
            context.Tweets.Add(reply2);

            await context.SaveChangesAsync();

            var popularTweetStrategy = new PopularTweetStrategy(context);


            var controller = new HomeController(mockLogger.Object, context,
                                                userManager, new GetAllTweets(context),
                                                popularTweetStrategy, mockNotificationService.Object);

            var result = await controller.Popular();
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsAssignableFrom<IEnumerable<Tweet>>(viewResult.ViewData.Model);

            Assert.Equal(5, model.Count());
            Assert.Equal(2, model.ElementAt(0).Id);
            Assert.Equal(1, model.ElementAt(1).Id);
        }
    }

    [Fact]
    public async Task ShowNotifications_ShowAllNotifications()
    {
        InitializeTest();

        var fakeClaimsPrincipal = new ClaimsPrincipal(new ClaimsIdentity(new Claim[]
        {
            new Claim(ClaimTypes.NameIdentifier, fakeUser.Id),
        }));

        using (var context = new TwitterContext(options))
        {
            var user1 = new ApplicationUser { Id = "1", UserName = "user1" };
            var user2 = new ApplicationUser { Id = "2", UserName = "user2" };

            context.Users.AddRange(user1, user2);

            context.Tweets.AddRange(
                new Tweet
                {
                    Id = 1,
                    TweetContent = "a",
                    UserId = "1",
                    User = user1,
                    Username = user1.UserName,
                    Likes = new List<TweetLike>(),
                    Bookmarks = new List<TweetBookmark>(),
                    Replies = new List<Tweet>(),
                    Retweets = new List<Retweet>()
                }
            );

            var notification1 = new Notification { Id = 1, UserId = "1", User = user1, TweetId = 1, Message = "a" };
            var notification2 = new Notification { Id = 2, UserId = "2", User = user1, TweetId = 2, Message = "b" };

            context.Notifications.AddRange(notification1, notification2);

            await context.SaveChangesAsync();

            var popularTweetStrategy = new PopularTweetStrategy(context);


            var controller = new HomeController(mockLogger.Object, context,
                                                userManager, new GetAllTweets(context),
                                                popularTweetStrategy, new NotificationService(context, userManager, mockHubContext.Object))
            {
                ControllerContext = new ControllerContext
                {
                    HttpContext = new DefaultHttpContext { User = fakeClaimsPrincipal }
                }
            };

            var result = await controller.ShowNotifications();
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsAssignableFrom<IEnumerable<Notification>>(viewResult.ViewData.Model);

            Assert.Single(model);
            Assert.Equal(1, model.ElementAt(0).Id);
        }
    }
}

