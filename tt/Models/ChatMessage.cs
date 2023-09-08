using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TwitterClone.Models;

public class ChatMessage
{
    [Key]
    public int Id { get; set; }

    [ForeignKey("Sender")]
    public string SenderId { get; set; }
    public virtual ApplicationUser Sender { get; set; }

    [ForeignKey("Recipient")]
    public string RecipientId { get; set; }
    public virtual ApplicationUser Recipient { get; set; }

    public string Content { get; set; }

    public DateTime Timestamp { get; set; }
}

