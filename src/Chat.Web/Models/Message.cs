using System;

namespace Chat.Web.Models
{
    public class Message
    {
        public int Id { get; set; }
        public string Content { get; set; }
        public DateTime Timestamp { get; set; }
        public ApplicationUser FromUser { get; set; }
        public string FromUserId { get; set; }
        public ApplicationUser ToUser { get; set; }
        public string ToUserId { get; set; }
        public int? ToRoomId { get; set; }
        public Room ToRoom { get; set; }
    }
}
