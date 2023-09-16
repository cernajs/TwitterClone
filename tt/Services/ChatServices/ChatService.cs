using Microsoft.EntityFrameworkCore;

using TwitterClone.Interfaces;
using TwitterClone.Models;

namespace TwitterClone.Data;

public class ChatService : IChatService
{
    private readonly TwitterContext _tweetRepo;

    public ChatService(TwitterContext tweetRepo)
    {
        _tweetRepo = tweetRepo;
    }
    /// <summary>
    ///     retrieve all existing chat messages
    ///     for current user
    /// </summary>
    /// <param name="currentUserId"></param>
    /// <returns></returns>
    public Task<List<ChatViewModel>> GetChatsOfSpecificUserAsync(string currentUserId)
    {
        return _tweetRepo.ChatMessages
            .Where(m => m.SenderId == currentUserId || m.RecipientId == currentUserId)
            .GroupBy(m => m.SenderId == currentUserId ? m.RecipientId : m.SenderId)
            .Select(g => new ChatViewModel
            {
                SenderName = g.Key == currentUserId ? g.FirstOrDefault().Recipient.UserName : g.FirstOrDefault().Sender.UserName,
                RecieventName = g.Key == currentUserId ? g.FirstOrDefault().Sender.UserName : g.FirstOrDefault().Recipient.UserName,
                ChatId = g.Key,
                MostRecentMessage = g.OrderByDescending(m => m.Timestamp).FirstOrDefault()
            })
            .ToListAsync();
    }

    /// <summary>
    ///     retrieve all messages between current currentUserId and otherUserId
    /// </summary>
    /// <param name="currentUserId"></param>
    /// <param name="otherUserId"></param>
    /// <returns></returns>
    public Task<List<ChatMessage>> GetMessagesBetweenUsersAsync(string currentUserId, string otherUserId)
    {
        return _tweetRepo.ChatMessages
            .Where(m => (m.SenderId == currentUserId && m.RecipientId == otherUserId) ||
                        (m.SenderId == otherUserId && m.RecipientId == currentUserId))
            .Include(m => m.Sender)
            .Include(m => m.Recipient)
            .OrderBy(m => m.Timestamp)
            .ToListAsync();
    }

    /// <summary>
    ///    create new chat message and return true if successful
    ///    else return false
    /// </summary>
    /// <param name="chatMessageDto"></param>
    /// <param name="userId"></param>
    /// <returns></returns>
    public async Task<bool> CreateMessageAsync(ChatMessageDto chatMessageDto, string userId)
    {
        if(chatMessageDto == null) return false;

        if(chatMessageDto.Content == null) return false;

        var message = new ChatMessageBuilder()
            .WithSenderId(userId)
            .WithRecipientId(chatMessageDto.RecipientId)
            .WithContent(chatMessageDto.Content)
            .WithTimestamp(DateTime.Now)
            .Build();

        if(message == null) return false;

        _tweetRepo.ChatMessages.Add(message);
        await _tweetRepo.SaveChangesAsync();

        return true;
    }
}
