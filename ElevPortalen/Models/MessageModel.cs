using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ElevPortalen.Models
{
    public class MessageModel
    {
        [Key]
        public int MessageId { get; set; } // Primary key
        public int ReceiverId { get; set; } // ID of the Student or company receiving the message
        public int SenderId { get; set; } // ID of the Student or company sending the message
        public int theSendersReceiverId { get; set; } // the id used for answare the message
        [Column(TypeName = "nvarchar(100)")]
        public string? SenderName { get; set; } // Name of the Sender
        [Column(TypeName = "nvarchar(200)")]
        public string? Subject { get; set; }  // Subject of the message
        [Column(TypeName = "nvarchar(max)")]
        public string? Content { get; set; }  // Content of the message
        [Required]
        public DateTime Timestamp { get; set; }  // timestamp
        public bool IsRead { get; set; }  // read or not?
    }
}
