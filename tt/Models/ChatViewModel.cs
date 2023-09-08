namespace TwitterClone.Models;

public class ChatViewModel
{
    public string SenderName { get; set; }
    public string RecieventName { get; set; }
    public string ChatId { get; set; }
    public ChatMessage MostRecentMessage { get; set; }
}