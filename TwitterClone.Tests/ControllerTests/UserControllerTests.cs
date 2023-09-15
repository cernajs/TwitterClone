using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Xunit;
using Microsoft.AspNetCore.Mvc;
using TwitterClone.Controllers;
using TwitterClone.Data;
using TwitterClone.Hubs;
using TwitterClone.Models;
using Microsoft.AspNetCore.Authentication;

namespace TwitterClone.Tests.ControllerTests;

public class UserControllerTests
{
    
    DbContextOptions<TwitterContext> options;
    Mock<IUserStore<ApplicationUser>> mockUserStore;
    UserManager<ApplicationUser> userManager;
    Mock<SignInManager<ApplicationUser>> mockSignInManager;
    TwitterContext context;
    UserService userService;
    UserController controller;
    ClaimsPrincipal fakeClaimsPrincipal;
    Mock<UserManager<ApplicationUser>> mockUserManager;
    ApplicationUser fakeUser;
    Mock<INotificationService> mockNotificationService;


    private void InitializeTest()
    {
        options = new DbContextOptionsBuilder<TwitterContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        mockUserStore = new Mock<IUserStore<ApplicationUser>>();
        userManager = new UserManager<ApplicationUser>(
            mockUserStore.Object, null, null, null, null, null, null, null, null);
        
        ///
        ///

        mockUserManager = new Mock<UserManager<ApplicationUser>>(
            mockUserStore.Object, null, null, null, null, null, null, null, null);

        ///
        ///

        mockSignInManager = new Mock<SignInManager<ApplicationUser>>
            (userManager, new HttpContextAccessor(), new Mock<IUserClaimsPrincipalFactory<ApplicationUser>>().Object, null, null, null, null);

        mockNotificationService = new Mock<INotificationService>();
        context = new TwitterContext(options);
        userService = new UserService(context, userManager, mockNotificationService.Object);
        controller = new UserController(context, userManager, mockSignInManager.Object, userService);

        var fakeUserId = "1";
        fakeUser = new ApplicationUser { Id = fakeUserId, UserName = "testUser" };


        fakeClaimsPrincipal = new ClaimsPrincipal(new ClaimsIdentity(new Claim[]
        {
            new Claim(ClaimTypes.NameIdentifier, fakeUserId),
        }));

        controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = fakeClaimsPrincipal }
        };

    }

    [Fact]
    public async Task TestIndex_UserNotFound_ShouldReturnNotFound()
    {
        InitializeTest();

        context.Users.Add(fakeUser);
        await context.SaveChangesAsync();

        var result = await controller.Index("1");
        var viewResult = Assert.IsType<ViewResult>(result);
        var model = Assert.IsAssignableFrom<ApplicationUser>(viewResult.ViewData.Model);

        Assert.Equal(fakeUser.Id, model.Id);
    }

    [Fact]
    public async Task TestFollow()
    {
        InitializeTest();

        var user = new ApplicationUser { Id = "2", UserName = "testUser" };

        mockUserStore.Setup(x => x.FindByIdAsync("2", default))
            .ReturnsAsync(user);
        context.Users.Add(fakeUser);

        await context.SaveChangesAsync();

        var result = await controller.Follow("2");
        var viewResult = Assert.IsType<RedirectToActionResult>(result);

        var userFollower = await context.UserFollowers.FirstOrDefaultAsync(x => x.FollowerId == fakeUser.Id && x.FollowingId == user.Id);
        Assert.NotNull(userFollower);
    }

    [Fact]
    public async Task TestUnfollow()
    {
        InitializeTest();

        var user = new ApplicationUser { Id = "2", UserName = "testUser" };

        mockUserStore.Setup(x => x.FindByIdAsync("2", default))
            .ReturnsAsync(user);
        context.Users.Add(fakeUser);

        var userFollower = new UserFollower { FollowerId = fakeUser.Id, FollowingId = user.Id };
        context.UserFollowers.Add(userFollower);

        await context.SaveChangesAsync();

        var result = await controller.Unfollow("2");
        var viewResult = Assert.IsType<RedirectToActionResult>(result);

        var userFollowerDeleted = await context.UserFollowers.FirstOrDefaultAsync(x => x.FollowerId == fakeUser.Id && x.FollowingId == user.Id);
        Assert.Null(userFollowerDeleted);
    }

    [Fact]
    public async Task TestShowFollowers()
    {
        InitializeTest();

        var user = new ApplicationUser { Id = "2", UserName = "testUser" };

        mockUserStore.Setup(x => x.FindByIdAsync("2", default))
            .ReturnsAsync(user);
        context.Users.Add(fakeUser);
        context.Users.Add(user);

        var userFollower = new UserFollower { FollowerId = fakeUser.Id, FollowingId = user.Id };
        context.UserFollowers.Add(userFollower);
        await context.SaveChangesAsync();

        var result = await controller.ShowUsers("2", "followers");
        var viewResult = Assert.IsType<ViewResult>(result);
        var model = Assert.IsAssignableFrom<List<ApplicationUser>>(viewResult.ViewData.Model);

        Assert.Equal(1, model.Count);
    }

    [Fact]
    public async Task TeshShowBookmark()
    {
        InitializeTest();

        var user = new ApplicationUser { Id = "2", UserName = "testUser" };

        mockUserStore.Setup(x => x.FindByIdAsync("2", default))
            .ReturnsAsync(user);
        context.Users.Add(fakeUser);
        context.Users.Add(user);

        var tweet = new Tweet { Id = 1, TweetContent = "test tweet", UserId = fakeUser.Id, Username = fakeUser.UserName };
        context.Tweets.Add(tweet);

        var bookmark = new TweetBookmark { Id = 1, UserId = fakeUser.Id, TweetId = tweet.Id };
        context.TweetBookmarks.Add(bookmark);

        await context.SaveChangesAsync();

        var result = await controller.ShowBookmarks();
        var viewResult = Assert.IsType<ViewResult>(result);
        var model = Assert.IsAssignableFrom<List<Tweet>>(viewResult.ViewData.Model);

        Assert.Equal(1, model.Count);
    }


    //[Fact]
    //public async Task EditProfile_Success_ReturnsJsonResult()
    //{
    //    var options = new DbContextOptionsBuilder<TwitterContext>()
    //        .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
    //        .Options;
    //    var mockUserStore = new Mock<IUserStore<ApplicationUser>>();

    //    mockUserManager = new Mock<UserManager<ApplicationUser>>(
    //        mockUserStore.Object, null, null, null, null, null, null, null, null);

    //    var mockSignInManager = new Mock<SignInManager<ApplicationUser>>
    //        (mockUserManager.Object, new HttpContextAccessor(), new Mock<IUserClaimsPrincipalFactory<ApplicationUser>>().Object, null, null, null, null);

    //    context = new TwitterContext(options);
    //    userService = new UserService(context, mockUserManager.Object);
    //    controller = new UserController(context, mockUserManager.Object, mockSignInManager.Object, userService);

    //    var fakeUserId = "1";
    //    var fakeUser = new ApplicationUser { Id = fakeUserId, UserName = "testUser" };


    //    var fakeClaimsPrincipal = new ClaimsPrincipal(new ClaimsIdentity(new Claim[]
    //    {
    //        new Claim(ClaimTypes.NameIdentifier, fakeUserId),
    //    }));

    //    controller.ControllerContext = new ControllerContext
    //    {
    //        HttpContext = new DefaultHttpContext { User = fakeClaimsPrincipal }
    //    };

    //    var editProfileViewModel = new EditProfileViewModel { Id = "1", Email = "newemail@example.com", UserName = "NewName" };
    //    mockUserManager.Setup(um => um.FindByIdAsync(It.IsAny<string>())).ReturnsAsync(fakeUser);
    //    mockUserManager.Setup(um => um.UpdateAsync(It.IsAny<ApplicationUser>())).ReturnsAsync(IdentityResult.Success);
    //    mockUserManager.Setup(um => um.FindByEmailAsync(It.IsAny<string>()))
    //        .ReturnsAsync(fakeUser);

    //    var result = await controller.EditProfile(editProfileViewModel);

    //    var jsonResult = Assert.IsType<JsonResult>(result);
    //    dynamic returnValue = jsonResult.Value;
    //    Assert.True((bool)returnValue.success);
    //}

}