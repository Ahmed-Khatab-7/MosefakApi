namespace MosefakApp.Core.Dtos.Notification
{
    public class NotificationResponse
    {
        public string Id { get; set; } = null!;
        public int UserId { get; set; }
        public string FullNameUser { get; set; } = null!;
        public string Title { get; set; } = null!;
        public string Body { get; set; } = null!;
        public bool IsRead { get; set; } = false;
        public DateTimeOffset CreatedAt { get; set; }
    }
}
