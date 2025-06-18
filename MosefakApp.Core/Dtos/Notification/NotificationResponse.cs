namespace MosefakApp.Core.Dtos.Notification
{
    public class NotificationResponse
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public string FullNameUser { get; set; } = null!;
        public string Title { get; set; } = null!;
        public string Body { get; set; } = null!;
        public bool IsRead { get; set; } = false;
        public DateTimeOffset CreatedAt { get; set; }
    }
}
