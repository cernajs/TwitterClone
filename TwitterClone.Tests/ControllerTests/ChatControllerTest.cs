using System;
using System.Collections.Generic;
using System.Linq;
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

using TwitterClone.Interfaces;
using TwitterClone.Data;
using Microsoft.AspNetCore.SignalR;
using TwitterClone.Hubs;
namespace TwitterClone.Tests.ControllerTests;

public class ChatControllerTest
{
    DbContextOptions<TwitterContext> options;
    Mock<ILogger<ChatController>> mockLogger;
    Mock<IUserService> mockUserService;
    ApplicationUser fakeUser;
    ClaimsPrincipal fakeClaimsPrincipal;
    Mock<INotificationService> mockNotificationService;

    private void InitializeTest()
    {
        options = new DbContextOptionsBuilder<TwitterContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        mockLogger = new Mock<ILogger<ChatController>>();
        mockUserService = new Mock<IUserService>();

        var fakeUserId = "1";
        fakeUser = new ApplicationUser { Id = fakeUserId };

        mockUserService.Setup(u => u.GetUserAsync(It.IsAny<ClaimsPrincipal>()))
                       .ReturnsAsync(fakeUser);


        fakeClaimsPrincipal = new ClaimsPrincipal(new ClaimsIdentity(new Claim[]
        {
            new Claim(ClaimTypes.NameIdentifier, fakeUserId),
        }));
    }
    [Fact]
    public async Task Index_ShouldReturnAllChats()
    {

        InitializeTest();

        using (var context = new TwitterContext(options))
        {
            context.ChatMessages.AddRange(
                new ChatMessage { Id = 1, SenderId = "1", RecipientId = "2", Content = "a" },
                new ChatMessage { Id = 2, SenderId = "1", RecipientId = "3", Content = "b" },
                new ChatMessage { Id = 3, SenderId = "1", RecipientId = "4", Content = "c" },
                new ChatMessage { Id = 4, SenderId = "1", RecipientId = "5", Content = "d" }
            );


            await context.SaveChangesAsync();

            var realChatService = new ChatService(context);

            var controller = new ChatController(mockLogger.Object, context, mockUserService.Object, realChatService, mockNotificationService.Object)
            {
                ControllerContext = new ControllerContext
                {
                    HttpContext = new DefaultHttpContext { User = fakeClaimsPrincipal }
                }
            };

            var result = await controller.Index();
            var viewResult = result as ViewResult;
            var model = viewResult.Model as List<ChatViewModel>;

            Assert.NotNull(model);
            Assert.Equal(4, model.Count);
        }
    }

    
    [Fact]
    public async Task ChatWithSpecificUserAsync_ShouldReturnCorrectViewModel()
    {
        InitializeTest();
        var fakeUserId = "1";
        var fakeUser = new ApplicationUser { Id = fakeUserId, UserName = "User1" };

        mockUserService.Setup(u => u.GetUserAsync(It.IsAny<ClaimsPrincipal>()))
                       .ReturnsAsync(fakeUser);

        using (var context = new TwitterContext(options))
        {
            context.ChatMessages.AddRange(
                new ChatMessage { Id = 1, SenderId = "2", RecipientId = "1", Content = "Message1", Timestamp = DateTime.Now, Sender = new ApplicationUser { Id = "2" } },
                new ChatMessage { Id = 2, SenderId = "1", RecipientId = "2", Content = "Message2", Timestamp = DateTime.Now, Sender = new ApplicationUser { Id = "1" } }
            );


            await context.SaveChangesAsync();

            var realChatService = new ChatService(context);
            var controller = new ChatController(null, context, mockUserService.Object, realChatService, mockNotificationService.Object)
            {
                ControllerContext = new ControllerContext
                {
                    HttpContext = new DefaultHttpContext { User = fakeClaimsPrincipal }
                }
            };


            var result = await controller.ChatWithSpecificUserAsync("2");
            var viewResult = result as ViewResult;
            var model = viewResult.Model as SpecificChatViewModel;

            Assert.NotNull(model);
            Assert.Equal(fakeUserId, model.currentUserId);
            Assert.Equal("2", model.otherUserId);
            Assert.Equal("User1", model.currentUserName);
            Assert.Equal(2, model.messages.Count);
        }
    }

    [Fact]
    public async Task CreateMessage_ShouldReturnOk_WhenMessageIsCreated()
    {
        InitializeTest();

        var chatMessageDto = new ChatMessageDto
        {
            Content = "a",
            RecipientId = "2"
        };

        using (var context = new TwitterContext(options))
        {
            var realChatService = new ChatService(context);
            var controller = new ChatController(null, context, mockUserService.Object, realChatService, mockNotificationService.Object)
            {
                ControllerContext = new ControllerContext
                {
                    HttpContext = new DefaultHttpContext { User = fakeClaimsPrincipal }
                }
            };

            var result = await controller.CreateMessage(chatMessageDto);

            Assert.IsType<OkResult>(result);
        }
    }

}

