namespace MosefakApp.Core.IServices
{
    public interface INotificationService
    {
        Task SendAndSaveNotificationAsync(int recipientUserId, string title, string message, CancellationToken cancellationToken = default);
        Task SendAndSaveBroadcastAsync(string title, string message, CancellationToken cancellationToken = default);
        Task<PaginatedResponse<NotificationResponse>> GetMyNotificationsAsync(int currentUserId, int page = 1, int pageSize = 10, CancellationToken cancellationToken = default);
        Task<int> GetUnreadNotificationsCountAsync(int currentUserId, CancellationToken cancellationToken = default);
        Task MarkNotificationAsReadAsync(int currentUserId, int notificationId, CancellationToken cancellationToken = default);
        Task MarkAllNotificationsAsReadAsync(int currentUserId, CancellationToken cancellationToken = default);
        Task<NotificationResponse?> GetNotificationByIdAsync(int currentUserId, int notificationId, CancellationToken cancellationToken = default);
        Task DeleteNotificationAsync(int currentUserId, int notificationId, CancellationToken cancellationToken = default);
        Task DeleteAllNotificationsAsync(int currentUserId, CancellationToken cancellationToken = default);


    }
}
