using FirebaseAdmin.Messaging; // هذا الـ using ضروري لـ Firebase Notification
using Microsoft.Extensions.Configuration;
using MosefakApp.Core.IServices.Logging;
using System.Threading;
using System.Threading.Tasks;

// لتجنب تضارب الأسماء مع MosefakApp.Domains.Entities.Notification
// يمكننا استخدام اسم مستعار لـ FirebaseAdmin.Messaging.Notification
using FirebaseNotification = FirebaseAdmin.Messaging.Notification;

namespace MosefakApi.Business.Services.FireBase
{
    public class FirebaseService : IFirebaseService
    {
        private readonly ILoggerService _logger;

        public FirebaseService(ILoggerService logger)
        {
            _logger = logger;
        }

        public async Task<bool> SendNotificationAsync(string fcmToken, string title, string message, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(fcmToken))
            {
                _logger.LogWarning("FCM token is missing. Notification not sent.");
                return false;
            }

            try
            {
                var firebaseMessage = new Message()
                {
                    Token = fcmToken,
                    Notification = new FirebaseNotification
                    {
                        Title = title,
                        Body = message
                    },
                };

                string response = await FirebaseMessaging.DefaultInstance.SendAsync(firebaseMessage, cancellationToken);

                _logger.LogInfo($"✅ Firebase Notification Sent Successfully to {fcmToken}. Response: {response}");
                return true;
            }
            catch (FirebaseAdmin.FirebaseException fbEx)
            {
                _logger.LogError($"🔥 Firebase Notification Failed (FirebaseException): {fbEx.Message}", fbEx);
                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError($"❌ Unexpected Error while sending Firebase notification: {ex.Message}", ex);
                return false;
            }
        }

        // تطبيق الدالة الجديدة لإرسال الإشعارات إلى عدة أجهزة
        public async Task<bool> SendNotificationsAsync(List<string> fcmTokens, string title, string message, CancellationToken cancellationToken = default)
        {
            if (fcmTokens == null || !fcmTokens.Any())
            {
                _logger.LogWarning("FCM token list is empty. No notifications sent.");
                return false;
            }

            try
            {
                var multicastMessage = new MulticastMessage()
                {
                    Tokens = fcmTokens,
                    Notification = new FirebaseNotification
                    {
                        Title = title,
                        Body = message
                    }
                };

                var response = await FirebaseMessaging.DefaultInstance.SendEachForMulticastAsync(multicastMessage, cancellationToken);

                if (response.FailureCount > 0)
                {
                    _logger.LogError($"🔥 Firebase Multicast Notification Failed. Success: {response.SuccessCount}, Failure: {response.FailureCount}");
                    foreach (var resp in response.Responses)
                    {
                        if (!resp.IsSuccess)
                        {
                            _logger.LogError($"  Failure Reason: {resp.Exception?.Message ?? resp.Exception?.InnerException?.Message}");
                        }
                    }
                    return false;
                }

                _logger.LogInfo($"✅ Firebase Multicast Notification Sent Successfully to {response.SuccessCount} devices.");
                return true;
            }
            catch (FirebaseAdmin.FirebaseException fbEx)
            {
                _logger.LogError( $"🔥 Firebase Multicast Notification Failed (FirebaseException): {fbEx.Message}", fbEx);
                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError($"❌ Unexpected Error while sending Firebase multicast notification: {ex.Message}", ex);
                return false;
            }
        }
    }
}
