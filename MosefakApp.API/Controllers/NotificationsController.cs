namespace MosefakApp.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class NotificationsController : ControllerBase
    {
        private readonly IFirebaseService _firebaseService;
        private readonly UserManager<AppUser> _userManager;
        private readonly INotificationService _notificationService;
        private readonly IUserService _userService;
        private readonly IIdProtectorService _idProtectorService;
        private IUnitOfWork _unitOfWork;



        public NotificationsController(IFirebaseService firebaseService, UserManager<AppUser> userManager, INotificationService notificationService, IUserService userService, IIdProtectorService idProtectorService, IUnitOfWork unitOfWork)
        {
            _firebaseService = firebaseService;
            _userManager = userManager;
            _notificationService = notificationService;
            _userService = userService;
            _idProtectorService = idProtectorService;
            _unitOfWork = unitOfWork;
        }

        [HttpPost("send-notification")]
        [AllowAnonymousPermission]

        public async Task<IActionResult> SendNotification([FromBody] SendNotificationDto model, CancellationToken cancellationToken = default)
        {
            var user = await _userManager.FindByIdAsync(model.UserId);
            if (user == null || string.IsNullOrEmpty(user.FcmToken))
                return NotFound("User or FCM Token not found");

            var success = await _firebaseService.SendNotificationAsync(user.FcmToken, model.Title, model.Message, cancellationToken);
            return success ? Ok("Notification sent!") : StatusCode(500, "Error sending notification");
        }

        [HttpGet("get-user-notifications")]
        [AllowAnonymousPermission]

        public async Task<ActionResult<PaginatedResponse<NotificationResponse>>> GetUserNotifications(int userId, int page = 1, int pageSize = 10, CancellationToken cancellationToken = default)
        {
            var query = await _notificationService.GetUserNotificationsAsync(userId,  page, pageSize, cancellationToken);

            return Ok(query);
        }

        [HttpPost("mark-as-read/{notificationId}")]
        [Authorize]
        public async Task<ActionResult<bool>> MarkNotificationAsRead(int notificationId, CancellationToken cancellationToken = default)
        {
            var userId = User.GetUserId();

            var query = await _notificationService.MarkNotificationAsReadAsync(userId, notificationId, cancellationToken);

            return Ok(query);
        }




        [HttpPost("send-to-all")]
        [AllowAnonymousPermission]
        public async Task<IActionResult> SendNotificationToAll([FromBody] SendNotificationToAllRequest request)
        {
            // جلب جميع الـ FCM Tokens من قاعدة البيانات
            var allFcmTokens = await _userService.GetAllFcmTokensAsync();

            if (allFcmTokens == null || !allFcmTokens.Any())
            {
                return BadRequest("No FCM tokens found to send notifications to.");
            }

            bool success = await _firebaseService.SendNotificationsAsync(
                allFcmTokens,
                request.Title,
                request.Body
            );

            if (success)
            {
                // يمكنك هنا حفظ الإشعار في قاعدة البيانات كإشعار عام
                // UserId = Guid.Empty أو معرف خاص للإشعارات العامة
                await _notificationService.AddNotificationAsync(new AddNotificationRequest
                {
                    Title = request.Title,
                    Body = request.Body,
                    IsRead = false
                });
                return Ok("Notification sent to all users successfully.");
            }
            else
            {
                return StatusCode(500, "Failed to send notification to all users.");
            }
        }

        // ✅ إرسال إشعار لمعرف معين
        [HttpPost("send-to-user/{userId}")]
        // [RequiredPermission(Permissions.Notifications.SendToUser)] // أضف الصلاحية المناسبة إذا لزم الأمر
        public async Task<IActionResult> SendNotificationToUserById(string userId, [FromBody] SendNotificationToUserRequest request)
        {
            var unprotectedUserId = UnprotectId(userId); // استخدم UnprotectId إذا كنت تستخدم حماية الـ IDs
            if (unprotectedUserId == null) return BadRequest("Invalid user ID.");

            // جلب الـ FCM Token الخاص بالمستخدم من قاعدة البيانات
            var userFcmToken = await _userService.GetFcmTokenByUserIdAsync(unprotectedUserId.Value);

            if (string.IsNullOrEmpty(userFcmToken))
            {
                return NotFound($"FCM Token not found for user ID: {userId}.");
            }

            bool success = await _firebaseService.SendNotificationAsync(
                userFcmToken,
                request.Title,
                request.Body
            );

            if (success)
            {
                // يمكنك هنا حفظ الإشعار في قاعدة البيانات كإشعار خاص بالمستخدم
                await _notificationService.AddNotificationAsync(new AddNotificationRequest
                {
                    UserId = unprotectedUserId.Value,
                    Title = request.Title,
                    Body = request.Body,
                    IsRead = false
                });
                await _unitOfWork.CommitAsync();
                return Ok($"Notification sent to user {userId} successfully.");
            }
            else
            {
                return StatusCode(500, $"Failed to send notification to user {userId}.");
            }

        }


        private string ProtectId(string id) => _idProtectorService.Protect(int.Parse(id));
        private int? UnprotectId(string id) => _idProtectorService.UnProtect(id);
    }
}
