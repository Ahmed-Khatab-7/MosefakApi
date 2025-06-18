// In: API/Controllers/NotificationsController.cs

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MosefakApp.API.Controllers;
using static MosefakApp.Infrastructure.constants.Permissions;

[Route("api/notifications")]
[ApiController]
[Authorize] // كل وظائف هذا الـ Controller تتطلب تسجيل الدخول
public class NotificationsController : ApiBaseController // Assuming you use a base controller
{
    private readonly INotificationService _notificationService;
    private readonly IIdProtectorService _idProtectorService;

    public NotificationsController(INotificationService notificationService, IIdProtectorService idProtectorService)
    {
        _notificationService = notificationService;
        _idProtectorService = idProtectorService;
    }

    #region ========== User-Facing Endpoints (للمستخدم العادي) ==========

    /// <summary>
    /// يجلب قائمة الإشعارات الخاصة بالمستخدم الحالي.
    /// </summary>
    [HttpGet]
    [RequiredPermission(Permissions.Notifications.View)]
    public async Task<ActionResult<PaginatedResponse<NotificationResponse>>> GetMyNotifications([FromQuery] int page = 1, [FromQuery] int pageSize = 10, CancellationToken cancellationToken = default)
    {
        var userId = User.GetUserId();
        var response = await _notificationService.GetMyNotificationsAsync(userId, page, pageSize, cancellationToken);

        // [تم الإصلاح] استخدام الدالة المساعدة لضمان استدعاء الـ overload الصحيح
        foreach (var notification in response.Data)
        {
            notification.Id = ProtectId(notification.Id);
        }

        return Ok(response);
    }


    [HttpGet("{notificationId}")]
    [RequiredPermission(Permissions.Notifications.View)] // نفس صلاحية عرض القائمة
    public async Task<ActionResult<NotificationResponse>> GetNotificationById(string notificationId, CancellationToken cancellationToken = default)
    {
        var userId = User.GetUserId();
        var unprotectedId = _idProtectorService.UnProtect(notificationId);

        if (unprotectedId == null)
        {
            return BadRequest("Invalid Notification ID.");
        }

        var response = await _notificationService.GetNotificationByIdAsync(userId, unprotectedId.Value, cancellationToken);

        if (response == null)
        {
            return NotFound();
        }

        // حماية الـ ID قبل إرساله للـ client
        response.Id = ProtectId(response.Id);

        return Ok(response);
    }
    /// <summary>
    /// يجلب عدد الإشعارات غير المقروءة للمستخدم الحالي.
    /// </summary>
    [HttpGet("unread-count")]
    [RequiredPermission(Permissions.Notifications.View)]
    public async Task<ActionResult<int>> GetUnreadCount(CancellationToken cancellationToken = default)
    {
        var userId = User.GetUserId();
        var count = await _notificationService.GetUnreadNotificationsCountAsync(userId, cancellationToken);
        return Ok(count);
    }

    /// <summary>
    /// يضع علامة "مقروء" على إشعار معين.
    /// </summary>
    [HttpPost("{notificationId}/read")]
    [RequiredPermission(Permissions.Notifications.MarkAsRead)]
    public async Task<IActionResult> MarkAsRead(string notificationId, CancellationToken cancellationToken = default)
    {
        // لا تحتاج لتشفير هنا، بل فك تشفير
        var unprotectedId = _idProtectorService.UnProtect(notificationId);
        if (unprotectedId == null) return BadRequest("Invalid Notification ID.");

        var userId = User.GetUserId();
        await _notificationService.MarkNotificationAsReadAsync(userId, unprotectedId.Value, cancellationToken);

        return Ok("Notification marked as read.");
    }

    /// <summary>
    /// يضع علامة "مقروء" على كل إشعارات المستخدم الحالي.
    /// </summary>
    [HttpPost("read-all")]
    [RequiredPermission(Permissions.Notifications.MarkAsRead)]
    public async Task<IActionResult> MarkAllAsRead(CancellationToken cancellationToken = default)
    {
        var userId = User.GetUserId();
        await _notificationService.MarkAllNotificationsAsReadAsync(userId, cancellationToken);

        return Ok("All notifications marked as read.");
    }

    #endregion

    #region ========== Admin-Facing Endpoints (للأدمن فقط) ==========

    /// <summary>
    /// [للأدمن] يرسل إشعارًا عامًا لجميع المستخدمين.
    /// </summary>
    [HttpPost("broadcast")]
    [RequiredPermission(Permissions.Notifications.SendBroadcast)]
    public async Task<IActionResult> SendBroadcastNotification([FromBody] BroadcastNotificationRequest request, CancellationToken cancellationToken = default)
    {
        await _notificationService.SendAndSaveBroadcastAsync(request.Title, request.Body, cancellationToken);
        return Ok("Broadcast notification has been sent successfully.");
    }

    /// <summary>
    /// [للأدمن] يرسل إشعارًا لمستخدم معين بواسطة الـ ID الخاص به.
    /// </summary>
    [HttpPost("user/{userId}")]
    [RequiredPermission(Permissions.Notifications.SendToSpecificUser)]
    public async Task<IActionResult> SendNotificationToUser(string userId, [FromBody] AdminSendToUserRequest request, CancellationToken cancellationToken = default)
    {
        // هنا أيضاً نستخدم فك التشفير
        var unprotectedId = _idProtectorService.UnProtect(userId);
        if (unprotectedId == null)
        {
            return BadRequest("Invalid User ID.");
        }

        await _notificationService.SendAndSaveNotificationAsync(
            recipientUserId: unprotectedId.Value,
            title: request.Title,
            message: request.Body,
            cancellationToken: cancellationToken
        );

        return Ok($"Notification sent successfully to user.");
    }

    [HttpDelete("all")] // يجب وضع هذا قبل الـ endpoint الذي يحتوي على parameter لتجنب التعارض
    [RequiredPermission(Permissions.Notifications.Delete)]
    public async Task<IActionResult> DeleteAllMyNotifications(CancellationToken cancellationToken = default)
    {
        var userId = User.GetUserId();
        await _notificationService.DeleteAllNotificationsAsync(userId, cancellationToken);
        return Ok("All notifications have been deleted successfully.");
    }

    /// <summary>
    /// [جديد] يقوم بحذف إشعار معين بالـ ID.
    /// </summary>
    [HttpDelete("{notificationId}")]
    [RequiredPermission(Permissions.Notifications.Delete)]
    public async Task<IActionResult> DeleteNotification(string notificationId, CancellationToken cancellationToken = default)
    {
        var userId = User.GetUserId();
        var unprotectedId = _idProtectorService.UnProtect(notificationId);

        if (unprotectedId == null)
        {
            return BadRequest("Invalid Notification ID.");
        }

        await _notificationService.DeleteNotificationAsync(userId, unprotectedId.Value, cancellationToken);

        return Ok("Notification deleted successfully.");
    }


    private string ProtectId(string id) => _idProtectorService.Protect(int.Parse(id));

    #endregion
}