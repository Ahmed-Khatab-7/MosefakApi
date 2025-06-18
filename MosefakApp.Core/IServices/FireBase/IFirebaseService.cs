namespace MosefakApp.Core.IServices.FireBase
{
    public interface IFirebaseService
    {
        Task<bool> SendNotificationAsync(string fcmToken, string title, string message, CancellationToken cancellationToken = default);
        Task<bool> SendNotificationsAsync(List<string> fcmTokens, string title, string message, CancellationToken cancellationToken = default); // هذا هو السطر الجديد
    }
}
