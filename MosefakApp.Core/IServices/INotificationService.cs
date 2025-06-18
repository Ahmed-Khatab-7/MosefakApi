namespace MosefakApp.Core.IServices
{
    public interface INotificationService
    {
        Task<PaginatedResponse<NotificationResponse>> GetUserNotificationsAsync(int userId, int pageNumber, int pageSize, CancellationToken cancellationToken=default); // تم التعديل ليتوافق مع Notification.UserId
        Task<NotificationResponse?> GetNotificationByIdAsync(int notificationId, CancellationToken cancellationToken = default); // تم التعديل ليتوافق مع Notification.Id
        Task<bool> MarkNotificationAsReadAsync(int notificationId, int userId, CancellationToken cancellationToken = default); // تم التعديل ليتوافق مع Notification.Id و UserId
        Task<bool> AddNotificationAsync(AddNotificationRequest request, CancellationToken cancellationToken = default);

    }
}
