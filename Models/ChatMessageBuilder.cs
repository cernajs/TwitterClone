namespace TwitterClone.Data;

public class ChatMessageBuilder
{
    private readonly ChatMessage _chatMessage;

    public ChatMessageBuilder()
    {
        _chatMessage = new ChatMessage();
    }

    public ChatMessageBuilder WithSenderId(string SenderId)
    {
        _chatMessage.SenderId = SenderId;
        return this;
    }

    public ChatMessageBuilder WithRecipientId(string RecipientId)
    {
        _chatMessage.RecipientId = RecipientId;
        return this;
    }

    public ChatMessageBuilder WithContent(string Content)
    {
        _chatMessage.Content = Content;
        return this;
    }

    public ChatMessageBuilder WithTimestamp(DateTime Timestamp)
    {
        _chatMessage.Timestamp = Timestamp;
        return this;
    }

    public ChatMessage Build()
    {
        // validate?
        return _chatMessage;
    }
}

// public class ChatMessage
// {
//     public int Id { get; set; }
//     public string SenderId { get; set; }
//     public virtual ApplicationUser Sender { get; set; }
//     public string RecipientId { get; set; }
//     public virtual ApplicationUser Recipient { get; set; }

//     public string Content { get; set; }

//     public DateTime Timestamp { get; set; }
// }