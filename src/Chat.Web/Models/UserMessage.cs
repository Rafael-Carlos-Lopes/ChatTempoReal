namespace Chat.Web.Models
{
    public class UserMessage
    {
        public ApplicationUser FromUser { get; set; }
        public string FromUserId { get; set; }
        public ApplicationUser ToUser { get; set; }
        public string ToUserId { get; set; }
    }
}
