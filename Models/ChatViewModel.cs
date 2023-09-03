namespace TwitterClone.Data;

public class ChatViewModel
{
    public string RecieventName { get; set; }
    public string ChatId { get; set; }
    public ChatMessage MostRecentMessage { get; set; }
}