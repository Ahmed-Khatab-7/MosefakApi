// In: Business/Services/NotificationService.cs

using Microsoft.EntityFrameworkCore;
using MosefakApp.Core.IUnit;
using MosefakApp.Domains.Entities;
using System.Security.Claims;
// تأكد من وجود using لخدمة اللوجر
// using MosefakApp.Core.Services;

public class NotificationService : INotificationService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly UserManager<AppUser> _userManager;
    private readonly IFirebaseService _firebaseService;
    private readonly IMapper _mapper;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly ILoggerService _logger;

    public NotificationService(
        IUnitOfWork unitOfWork,
        UserManager<AppUser> userManager,
        IFirebaseService firebaseService,
        IMapper mapper,
        IHttpContextAccessor httpContextAccessor,
        ILoggerService logger)
    {
        _unitOfWork = unitOfWork;
        _userManager = userManager;
        _firebaseService = firebaseService;
        _mapper = mapper;
        _httpContextAccessor = httpContextAccessor;
        _logger = logger;
    }

    public async Task SendAndSaveNotificationAsync(int recipientUserId, string title, string message, CancellationToken cancellationToken = default)
    {
        var recipient = await _userManager.FindByIdAsync(recipientUserId.ToString());
        if (recipient == null)
        {
            _logger.LogWarning($"Attempted to send a notification to a non-existent user with ID {recipientUserId}.");
            return;
        }

        var notification = new Notification {
            UserId = recipientUserId,       // ID المستخدم الذي سيستقبل الإشعار
            Title = title,                  // عنوان الإشعار
            Message = message,              // نص الإشعار
            IsRead = false                 // القيمة الافتراضية، لم يقرأ بعد
        };
        await _unitOfWork.Repository<Notification>().AddEntityAsync(notification);
        await _unitOfWork.CommitAsync(cancellationToken);

        if (!string.IsNullOrEmpty(recipient.FcmToken))
        {
            try
            {
                await _firebaseService.SendNotificationAsync(recipient.FcmToken, title, message, cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Firebase push notification failed for user {recipientUserId}. Exception: {ex.Message}");
            }
        }
    }

    public async Task SendAndSaveBroadcastAsync(string title, string message, CancellationToken cancellationToken = default)
    {
        var usersWithTokens = await _userManager.Users
        .Where(u => !u.IsDeleted && !string.IsNullOrEmpty(u.FcmToken))
        .Select(u => new { u.Id, u.FcmToken })
        .ToListAsync(cancellationToken);

        if (!usersWithTokens.Any()) return;


        var notifications = usersWithTokens.Select(u => new Notification {
            UserId = u.Id,                  // ID المستخدم من اللفة الحالية
            Title = title,                  // العنوان ثابت للجميع
            Message = message,              // النص ثابت للجميع
            IsRead = false,                 // القيمة الافتراضية
        }).ToList();
        await _unitOfWork.Repository<Notification>().AddRangeAsync(notifications);
        await _unitOfWork.CommitAsync(cancellationToken);

        var fcmTokens = usersWithTokens.Select(u => u.FcmToken!).ToList();
        try
        {
            await _firebaseService.SendNotificationsAsync(fcmTokens, title, message, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError($"Firebase broadcast push notification failed. Exception: {ex.Message}");
        }
    }

    public async Task<PaginatedResponse<NotificationResponse>> GetMyNotificationsAsync(int currentUserId, int page = 1, int pageSize = 10, CancellationToken cancellationToken = default)
    {
        (var notifications, var totalCount) = await _unitOfWork.Repository<Notification>()
            .GetAllAsync(
                expression: x => x.UserId == currentUserId && !x.IsDeleted,
                include: null,
                pageNumber: page,
                pageSize: pageSize);

        // تحويل القائمة باستخدام الدالة المساعدة
        var mappedNotifications = notifications.Select(MapToNotificationResponse).ToList();

        // الترتيب بعد التحويل
        var sortedNotifications = mappedNotifications.OrderByDescending(n => n.CreatedAt).ToList();

        var totalPages = (int)Math.Ceiling((double)totalCount / pageSize);

        return new PaginatedResponse<NotificationResponse>
        {
            Data = sortedNotifications,
            CurrentPage = page,
            PageSize = pageSize,
            TotalPages = totalPages
        };
    }



    public async Task<int> GetUnreadNotificationsCountAsync(int currentUserId, CancellationToken cancellationToken = default)
    {
        long count = await _unitOfWork.Repository<Notification>()
            .GetCountWithConditionAsync(n => n.UserId == currentUserId && !n.IsRead);
        return (int)count;
    }

    public async Task MarkNotificationAsReadAsync(int currentUserId, int notificationId, CancellationToken cancellationToken = default)
    {
        var notification = await _unitOfWork.Repository<Notification>()
            .FirstOrDefaultAsync(n => n.Id == notificationId && n.UserId == currentUserId);

        if (notification == null)
            throw new ItemNotFound("Notification not found or you don't have permission to access it.");

        if (notification.IsRead) return;

        notification.IsRead = true;
        await _unitOfWork.Repository<Notification>().UpdateEntityAsync(notification);
        await _unitOfWork.CommitAsync(cancellationToken);
    }

    public async Task MarkAllNotificationsAsReadAsync(int currentUserId, CancellationToken cancellationToken = default)
    {
        // [الإصلاح النهائي] إصلاح خطأ Deconstruct. الدالة ترجع قائمة مباشرة.
        var unreadNotifications = await _unitOfWork.Repository<Notification>()
            .GetAllAsync(expression: n => n.UserId == currentUserId && !n.IsRead);

        if (!unreadNotifications.Any()) return;

        foreach (var notification in unreadNotifications)
        {
            notification.IsRead = true;
        }

        await _unitOfWork.Repository<Notification>().UpdateRangeAsync(unreadNotifications);
        await _unitOfWork.CommitAsync(cancellationToken);
    }

    public async Task<NotificationResponse?> GetNotificationByIdAsync(int currentUserId, int notificationId, CancellationToken cancellationToken = default)
    {
        var notification = await _unitOfWork.Repository<Notification>()
            .FirstOrDefaultAsync(n => n.Id == notificationId && n.UserId == currentUserId);

        if (notification == null)
        {
            return null;
        }

        // استخدام الدالة المساعدة لتحويل النتيجة
        return MapToNotificationResponse(notification);
    }


    public async Task DeleteNotificationAsync(int currentUserId, int notificationId, CancellationToken cancellationToken = default)
    {
        var notification = await _unitOfWork.Repository<Notification>()
            .FirstOrDefaultAsync(n => n.Id == notificationId && n.UserId == currentUserId);

        if (notification == null)
        {
            throw new ItemNotFound("Notification not found or you don't have permission to delete it.");
        }

        // استخدام Soft Delete من الـ BaseEntity
        notification.MarkAsDeleted(currentUserId);

        await _unitOfWork.Repository<Notification>().UpdateEntityAsync(notification);
        await _unitOfWork.CommitAsync(cancellationToken);
    }

    // [جديد] تنفيذ حذف كل الإشعارات
    public async Task DeleteAllNotificationsAsync(int currentUserId, CancellationToken cancellationToken = default)
    {
        // جلب كل الإشعارات غير المحذوفة للمستخدم
        var allNotifications = await _unitOfWork.Repository<Notification>()
            .GetAllAsync(expression: n => n.UserId == currentUserId && !n.IsDeleted);

        if (!allNotifications.Any())
        {
            // لا يوجد شيء لحذفه
            return;
        }

        foreach (var notification in allNotifications)
        {
            // استخدام Soft Delete من الـ BaseEntity
            notification.MarkAsDeleted(currentUserId);
        }

        await _unitOfWork.Repository<Notification>().UpdateRangeAsync(allNotifications);
        await _unitOfWork.CommitAsync(cancellationToken);
    }

    private int GetCurrentUserId()
    {
        var userIdClaim = _httpContextAccessor.HttpContext?.User?.FindFirstValue(ClaimTypes.NameIdentifier);
        return int.TryParse(userIdClaim, out var userId) ? userId : 0;
    }


    private NotificationResponse MapToNotificationResponse(Notification notification)
    {
        return new NotificationResponse
        {
            Id = notification.Id.ToString(),
            UserId = notification.UserId,
            Title = notification.Title,
            Body = notification.Message,
            IsRead = notification.IsRead,
            CreatedAt = notification.CreatedAt
        };
    }
}