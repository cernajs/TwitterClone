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
using Microsoft.Extensions.Options;

namespace TwitterClone.Tests.ControllerTests;

public class TweetControllerTests
{
    DbContextOptions<TwitterContext> options;
    Mock<IUserStore<ApplicationUser>> mockUserStore;
    UserManager<ApplicationUser> userManager;
    Mock<SignInManager<ApplicationUser>> mockSignInManager;
    Mock<IHubContext<NotificationHub>> mockHubContext;
    TwitterContext context;
    UserService userService;
    INotificationService notificationService;
    ITweetService tweetService;
    IHashtagService hashtagService;
    TweetController controller;
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

        context = new TwitterContext(options);
        userService = new UserService(context, userManager);
        notificationService = new NotificationService(context, userManager, mockHubContext.Object);
        tweetService = new TweetService(context);
        hashtagService = new HashtagService(context);
        controller = new TweetController(context, userService, notificationService, tweetService, hashtagService);

        var fakeUserId = "1";
        fakeUser = new ApplicationUser { Id = fakeUserId, UserName = "testUser" };
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
    public async Task TestCreateMethod()
    {
        InitializeTest();

        var result = await controller.Create(fakeUser.UserName, "This is a tweet");

        Assert.IsType<JsonResult>(result);
        var jsonResult = (JsonResult)result;
        var value = jsonResult.Value;
        var propertyInfo = value.GetType().GetProperty("success");
        var success = (bool)propertyInfo.GetValue(value, null);
        Assert.True(success);
        var tweet = context.Tweets.FirstOrDefault();
        Assert.NotNull(tweet);
        Assert.Equal("This is a tweet", tweet.TweetContent);
    }


    [Fact]
    public async Task TestDeleteMethod_ValidTweet_ShouldReturnRedirect()
    {
        InitializeTest();

        var tweet = new Tweet { Id = 1, TweetContent = "Test tweet", UserId = fakeUser.Id, User = fakeUser, Username = fakeUser.UserName };
        context.Tweets.Add(tweet);

        await context.SaveChangesAsync();

        var result = await controller.Delete(tweet.Id);

        var redirectToActionResult = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal("Index", redirectToActionResult.ActionName);
        Assert.Equal("Home", redirectToActionResult.ControllerName);

        var deletedTweet = await context.Tweets.FindAsync(tweet.Id);
        Assert.Null(deletedTweet);
        
    }



    [Fact]
    public async Task TestLikeMethod_ValidTweet_ShouldReturnSuccess()
    {
        InitializeTest();

        var tweet = new Tweet { Id = 1, TweetContent = "Test tweet", UserId = fakeUser.Id, User = fakeUser, Username = fakeUser.UserName };
        context.Tweets.Add(tweet);

        await context.SaveChangesAsync();

           
        var result = await controller.Like(tweet.Id);

        var jsonResult = Assert.IsType<JsonResult>(result);
        var value = jsonResult.Value;
        var propertyInfo = value.GetType().GetProperty("success");
        var success = (bool)propertyInfo.GetValue(value, null);
        Assert.True(success);


        var likedTweet = await context.TweetLikes.FindAsync(fakeUser.Id, tweet.Id);
        Assert.NotNull(likedTweet);
        
    }



    [Fact]
    public async Task TestUnlikeMethod_ValidTweet_ShouldReturnSuccess()
    {
        InitializeTest();

        var tweet = new Tweet { Id = 2, TweetContent = "Test tweet", UserId = fakeUser.Id, User = fakeUser, Username = fakeUser.UserName };
        context.Tweets.Add(tweet);
        var tweetLike = new TweetLike { TweetId = tweet.Id, UserId = fakeUser.Id };
        context.TweetLikes.Add(tweetLike);

        await context.SaveChangesAsync();


        var result = await controller.Unlike(tweet.Id);

        var jsonResult = Assert.IsType<JsonResult>(result);
        var value = jsonResult.Value;
        var propertyInfo = value.GetType().GetProperty("success");
        var success = (bool)propertyInfo.GetValue(value, null);
        Assert.True(success);

        var likedTweet = await context.TweetLikes.FindAsync(fakeUser.Id, tweet.Id);
        Assert.Null(likedTweet);
    }



    [Fact]
    public async Task TestShowLikesMethod_NoLikers_ShouldReturnEmptyList()
    {
        InitializeTest();

        var tweet = new Tweet { Id = 1, TweetContent = "Test tweet", UserId = "1", Username = "a" };
        context.Tweets.Add(tweet);
        await context.SaveChangesAsync();


        var result = await controller.ShowLikes(tweet.Id);

        var viewResult = Assert.IsType<ViewResult>(result);
        var model = Assert.IsAssignableFrom<List<ApplicationUser>>(viewResult.Model);

        Assert.Empty(model);
        
    }

    [Fact]
    public async Task TestShowLikesMethod_WithLikers_ShouldReturnList()
    {
        InitializeTest();

        var user1 = new ApplicationUser { Id = "1", UserName = "user1" };
        var user2 = new ApplicationUser { Id = "2", UserName = "user2" };
        context.Users.AddRange(user1, user2);

        var tweet = new Tweet { Id = 2, TweetContent = "Test tweet", UserId = "1", Username = "a" };
        context.Tweets.Add(tweet);

        var tweetLike1 = new TweetLike { UserId = user1.Id, TweetId = tweet.Id };
        var tweetLike2 = new TweetLike { UserId = user2.Id, TweetId = tweet.Id };
        context.TweetLikes.AddRange(tweetLike1, tweetLike2);

        await context.SaveChangesAsync();

        var result = await controller.ShowLikes(tweet.Id);

        var viewResult = Assert.IsType<ViewResult>(result);
        var model = Assert.IsAssignableFrom<List<ApplicationUser>>(viewResult.Model);

        Assert.Equal(2, model.Count);
        Assert.Contains(model, u => u.Id == user1.Id);
        Assert.Contains(model, u => u.Id == user2.Id);
        
    }


    [Fact]
    public async Task TestReply_EmptyContent_ShouldRedirectToHomeIndex()
    {
        InitializeTest();

        var tweet = new Tweet { Id = 3, TweetContent = "Test tweet", UserId = "1", Username = "a" };
        context.Tweets.Add(tweet);
        await context.SaveChangesAsync();

            
        var result = await controller.Reply(1, string.Empty);

        var redirectToActionResult = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal("Index", redirectToActionResult.ActionName);
        Assert.Equal("Home", redirectToActionResult.ControllerName);
        
    }

    [Fact]
    public async Task TestReply_TweetNotCreated_ShouldReturnRedirectToActionResult()
    {
        InitializeTest();

        var result = await controller.Reply(1, "d");

        Assert.IsType<RedirectToActionResult>(result);
        
    }

    [Fact]
    public async Task TestReply_Successful_ShouldRedirectToReferer()
    {
        InitializeTest();

        var tweet = new Tweet { Id = 4, TweetContent = "Test tweet", UserId = "1", Username = "a" };

        context.Tweets.Add(tweet);
        await context.SaveChangesAsync();

        var result = await controller.Reply(4, "Test reply");

        var redirectResult = Assert.IsType<RedirectToActionResult>(result);

        Assert.Equal("Index", redirectResult.ActionName);
        Assert.Equal("Home", redirectResult.ControllerName);
    }

    [Fact]
    public async Task TestViewReplies_ParentTweetNotFound_ShouldReturnNotFound()
    {
        InitializeTest();

        var result = await controller.ViewReplies(1);

        Assert.IsType<RedirectToActionResult>(result);
    }


    [Fact]
    public async Task TestViewReplies_ShouldReturnNotFound()
    {
        InitializeTest();

        var parentTweet = new Tweet { Id = 1, TweetContent = "Test tweet", UserId = "1", Username = "a", User = fakeUser };
        var replie = new Tweet { Id = 2, TweetContent = "Reply", UserId = "1", Username = "a", ParentTweetId = 1, ParentTweet = parentTweet, User = fakeUser } ;

        context.Tweets.Add(parentTweet);
        context.Tweets.Add(replie);

        await context.SaveChangesAsync();

        var result = await controller.ViewReplies(1);

        var viewResult = Assert.IsType<ViewResult>(result);
        var model = Assert.IsType<Tuple<Tweet, IEnumerable<Tweet>>>(viewResult.Model);
        Assert.Equal(parentTweet, model.Item1);
        var items = model.Item2.ToList();
        Assert.Equal(replie, items[0]);
    }


    [Fact]
    public async Task TestBookmark_BookmarkUnbookmark()
    {
        InitializeTest();

        var tweet = new Tweet { Id = 1, TweetContent = "Test tweet", UserId = "1", Username = "a", User = fakeUser };
            
        context.Tweets.Add(tweet);

        await context.SaveChangesAsync();

        var result = await controller.Bookmark(tweet.Id, true);
        var jsonResult = Assert.IsType<OkObjectResult>(result);
        var value = jsonResult.Value;
        var propertyInfo = value.GetType().GetProperty("Success");
        var success = (bool)propertyInfo.GetValue(value, null);
        Assert.True(success);

        var result1 = await controller.Bookmark(tweet.Id, false);
        var jsonResult1 = Assert.IsType<OkObjectResult>(result1);
        var value1 = jsonResult.Value;
        var propertyInfo1 = value.GetType().GetProperty("Success");
        var success1 = (bool)propertyInfo.GetValue(value1, null);
        Assert.True(success1);
    }

    [Fact]
    public async Task TestRetweet_RetweetUnretweet()
    {
        InitializeTest();

        var tweet = new Tweet { Id = 1, TweetContent = "Test tweet", UserId = "1", Username = "a", User = fakeUser };

        context.Tweets.Add(tweet);

        await context.SaveChangesAsync();


        var result = await controller.Retweet(tweet.Id, true);
        var jsonResult = Assert.IsType<OkObjectResult>(result);
        var value = jsonResult.Value;
        var propertyInfo = value.GetType().GetProperty("Success");
        var success = (bool)propertyInfo.GetValue(value, null);
        Assert.True(success);

        var result1 = await controller.Retweet(tweet.Id, false);
        var jsonResult1 = Assert.IsType<OkObjectResult>(result1);
        var value1 = jsonResult.Value;
        var propertyInfo1 = value.GetType().GetProperty("Success");
        var success1 = (bool)propertyInfo.GetValue(value1, null);
        Assert.True(success1);
    }
}

