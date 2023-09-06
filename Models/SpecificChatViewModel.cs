namespace TwitterClone.Models;

public class SpecificChatViewModel
{
    public string currentUserId { get; set; }
    public string otherUserId { get; set; }
    public string currentUserName { get; set; }
    public List<ChatMessage> messages { get; set; }
}