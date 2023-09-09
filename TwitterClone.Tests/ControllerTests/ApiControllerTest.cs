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

namespace TwitterClone.Tests.ControllerTests;

public class ApiControllerTests
{
    DbContextOptions<TwitterContext> options;
    Mock<IUserStore<ApplicationUser>> mockUserStore;
    UserManager<ApplicationUser> userManager;
    Mock<IHubContext<NotificationHub>> mockHubContext;
    TwitterContext context;
    UserService userService;
    INotificationService notificationService;
    Mock<IHomeService> homeService;
    ApiController controller;
    ClaimsPrincipal fakeClaimsPrincipal;
    Mock<UserManager<ApplicationUser>> mockUserManager;
    ApplicationUser fakeUser;

    private void InitializeTest()
    {
        options = new DbContextOptionsBuilder<TwitterContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        mockUserStore = new Mock<IUserStore<ApplicationUser>>();
        userManager = new UserManager<ApplicationUser>(
            mockUserStore.Object, null, null, null, null, null, null, null, null);

        mockHubContext = new Mock<IHubContext<NotificationHub>>();
        notificationService = new NotificationService(context, userManager, mockHubContext.Object);
        homeService = new Mock<IHomeService>();
        context = new TwitterContext(options);

        controller = new ApiController(context, userManager, notificationService, homeService.Object);

        var fakeUserId = "1";
        fakeUser = new ApplicationUser { Id = fakeUserId };
        mockUserStore.Setup(x => x.FindByIdAsync(fakeUserId, CancellationToken.None))
                    .ReturnsAsync(fakeUser);

        var fakeClaimsPrincipal = new ClaimsPrincipal(new ClaimsIdentity(new Claim[]
        {
            new Claim(ClaimTypes.NameIdentifier, fakeUserId),
        }));

        controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = fakeClaimsPrincipal }
        };

    }

    [Fact]
    public async Task GetNotificationCount_ShouldReturnCorrectCount()
    {
        InitializeTest();

        var fakeUserId = "1";
        var fakeUser = new ApplicationUser { Id = fakeUserId };
        mockUserStore.Setup(x => x.FindByIdAsync(fakeUserId, CancellationToken.None))
                    .ReturnsAsync(fakeUser);

        var fakeClaimsPrincipal = new ClaimsPrincipal(new ClaimsIdentity(new Claim[]
        {
            new Claim(ClaimTypes.NameIdentifier, fakeUserId),
        }));

        using (var context = new TwitterContext(options))
        {

            var notification = new Notification { Id = 1, UserId = fakeUserId, TweetId = 6, Message = "dd", IsSeen = false };
            context.Notifications.Add(notification);
            await context.SaveChangesAsync();

            var notificationService = new NotificationService(context, userManager, mockHubContext.Object);

            var homeService = new Mock<IHomeService>();

            var controller = new ApiController(context, userManager, notificationService, homeService.Object)
            {
                ControllerContext = new ControllerContext
                {
                    HttpContext = new DefaultHttpContext { User = fakeClaimsPrincipal }
                }
            };

            var result = await controller.GetNotificationCount();
            var jsonResult = (JsonResult)result;
            var value = jsonResult.Value;
            var propertyInfo = value.GetType().GetProperty("notificationCount");
            var notificationCount = (int)propertyInfo.GetValue(value, null);

            Assert.Equal(1, notificationCount);
        }
    }

    [Fact]
    public async Task GetTrendingTopics_ShouldReturnCorrectTopics()
    {


        var options = new DbContextOptionsBuilder<TwitterContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

        using (var context = new TwitterContext(options))
        {
            var hashtags = new List<Hashtag>
            {
                new Hashtag { Id = 1, Tag = "topic1" },
                new Hashtag { Id = 2, Tag = "topic2" },
            };

            await context.Hashtags.AddRangeAsync(hashtags);

            var tweets = new List<Tweet>
            {
                new Tweet { Id = 1, TweetContent = "a", UserId = "1", Username = "a" },
                new Tweet { Id = 2, TweetContent = "b", UserId = "2", Username = "b" },
            };

            await context.Tweets.AddRangeAsync(tweets);

            var tweetHashtags = new List<TweetHashtag>
            {
                new TweetHashtag { TweetId = 1, HashtagId = 1 },
                new TweetHashtag { TweetId = 2, HashtagId = 1 },
                new TweetHashtag { TweetId = 2, HashtagId = 2 },
            };

            await context.TweetHashtags.AddRangeAsync(tweetHashtags);
            await context.SaveChangesAsync();

            var homeService = new HomeService(context);

            var controller = new ApiController(context, null, null, homeService);

            var result = await controller.GetTrendingTopics();
            var jsonResult = (JsonResult)result;
            var trendingTopics = (List<string>)jsonResult.Value; 

            Assert.Equal(2, trendingTopics.Count);

        }
    }


    [Fact]
    public async Task GetFollowSuggest_ShouldReturnCorrectUsers()
    {
        var options = new DbContextOptionsBuilder<TwitterContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        var fakeUserId = "1";
        var fakeUser = new ApplicationUser { Id = fakeUserId };
        var fakeClaimsPrincipal = new ClaimsPrincipal(new ClaimsIdentity(new Claim[]
        {
            new Claim(ClaimTypes.NameIdentifier, fakeUserId),
        }));

        using (var arrangeContext = new TwitterContext(options))
        {
            arrangeContext.Users.Add(fakeUser);
            arrangeContext.Users.Add(new ApplicationUser { Id = "2" });
            arrangeContext.Users.Add(new ApplicationUser { Id = "3" });
            arrangeContext.SaveChanges();
        }

        using (var context = new TwitterContext(options))
        {
            var mockUserStore = new Mock<IUserStore<ApplicationUser>>();
            var userManager = new UserManager<ApplicationUser>(
                mockUserStore.Object, null, null, null, null, null, null, null, null);

            mockUserStore.Setup(x => x.FindByIdAsync(fakeUserId, CancellationToken.None))
                        .ReturnsAsync(fakeUser);

            var homeService = new HomeService(context);

            var controller = new ApiController(context, userManager, null, homeService)
            {
                ControllerContext = new ControllerContext
                {
                    HttpContext = new DefaultHttpContext { User = fakeClaimsPrincipal }
                }
            };

            var result = await controller.GetFollowSuggest();
            var jsonResult = (JsonResult)result;
            var returnedUsers = (List<ApplicationUser>)jsonResult.Value;

            Assert.Equal(2, returnedUsers.Count);
        }
    }
}
