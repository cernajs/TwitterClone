using System.Collections.Generic;
using System.Threading.Tasks;
using TwitterClone.Models;

namespace TwitterClone.Interfaces;

public interface IChatService
{
    public Task<List<ChatViewModel>> GetChatsOfSpecificUserAsync(string currentUserId);
    
    Task<List<ChatMessage>> GetMessagesBetweenUsersAsync(string currentUserId, string otherUserId);
    
    Task<bool> CreateMessageAsync(ChatMessageDto chatMessageDto, string userId);
}