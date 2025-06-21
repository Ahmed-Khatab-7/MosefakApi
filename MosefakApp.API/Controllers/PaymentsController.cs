namespace MosefakApp.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PaymentsController : ControllerBase
    {
        private readonly IConfiguration _configuration;
        private readonly ILoggerService _logger;
        private readonly IUnitOfWork _unitOfWork;
        private readonly INotificationService _notificationService;
        public PaymentsController(IConfiguration configuration, ILoggerService logger, IUnitOfWork unitOfWork, INotificationService notificationService)
        {
            _configuration = configuration;
            _logger = logger;
            _unitOfWork = unitOfWork;
            _notificationService = notificationService;
        }

        [HttpPost("webhook")]
        [AllowAnonymousPermission]
        public async Task<IActionResult> StripeWebhook()
        {
            var json = await new StreamReader(HttpContext.Request.Body).ReadToEndAsync();
            Event stripeEvent;

            try
            {
                var stripeSecret = _configuration["PaymentSettings:WebhookSecret"];
                stripeEvent = EventUtility.ConstructEvent(json, Request.Headers["Stripe-Signature"], stripeSecret);
            }
            catch (Exception ex)
            {
                _logger.LogError($"⚠️ Stripe Webhook Error: {ex.Message}");
                return BadRequest(new { message = "Invalid Webhook Event" });
            }

            try
            {
                switch (stripeEvent.Type)
                {
                    case "payment_intent.succeeded":
                        return await HandlePaymentSuccess(stripeEvent);

                    case "payment_intent.payment_failed":
                        return await HandlePaymentFailed(stripeEvent);

                    case "charge.refund.updated":
                        return await HandleRefundUpdated(stripeEvent);

                    default:
                        _logger.LogWarning($"🔔 Unhandled Stripe event: {stripeEvent.Type}");
                        return Ok();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"⚠️ Stripe Webhook Processing Error: {ex.Message}");
                return StatusCode(500, new { message = "Webhook processing error." });
            }
        }


        private async Task<IActionResult> HandlePaymentSuccess(Event stripeEvent)
        {
            var paymentIntent = stripeEvent.Data.Object as PaymentIntent;
            if (paymentIntent == null) return BadRequest();

            var payment = await _unitOfWork.Repository<Payment>()
                .FirstOrDefaultAsync(x => x.StripePaymentIntentId == paymentIntent.Id);

            if (payment == null) return NotFound(new { message = "Payment not found." });

            // تجنباً لإرسال إشعارات مكررة إذا وصل الـ webhook أكثر من مرة
            if (payment.Status == PaymentStatus.Paid) return Ok();

            payment.Status = PaymentStatus.Paid;

            // [إصلاح] يجب عمل Include للطبيب للحصول على AppUserId الخاص به
            var appointment = await _unitOfWork.GetCustomRepository<IAppointmentRepositoryAsync>()
                .FirstOrDefaultAsync(x => x.Id == payment.AppointmentId,
                                     query => query.Include(x => x.Payment).Include(x => x.Doctor));

            if (appointment != null)
            {
                appointment.AppointmentStatus = AppointmentStatus.Confirmed;
                appointment.PaymentStatus = PaymentStatus.Paid;
                appointment.ConfirmedAt = DateTimeOffset.UtcNow;
                appointment.Payment = payment;
            }

            await _unitOfWork.CommitAsync();
            _logger.LogInfo($"✅ Payment successful for {payment.AppointmentId}.");

            // --- [تمت الإضافة] إرسال الإشعارات ---
            if (appointment != null)
            {
                // 1. إرسال إشعار للمريض
                await _notificationService.SendAndSaveNotificationAsync(
                    recipientUserId: appointment.PatientId,
                    title: "Payment Successful!",
                    message: $"Your payment for the appointment on {appointment.StartDate:g} was successful. Your appointment is now confirmed."
                );

                // 2. إرسال إشعار للطبيب
                await _notificationService.SendAndSaveNotificationAsync(
                    recipientUserId: appointment.Doctor.AppUserId,
                    title: "Appointment Confirmed",
                    message: $"Payment has been completed for the appointment on {appointment.StartDate:g}. It is now confirmed in your schedule."
                );
            }
            // --- نهاية الإضافة ---

            return Ok();
        }
        private async Task<IActionResult> HandlePaymentFailed(Event stripeEvent)
        {
            var paymentIntent = stripeEvent.Data.Object as PaymentIntent;
            if (paymentIntent == null) return BadRequest();

            var payment = await _unitOfWork.Repository<Payment>()
                .FirstOrDefaultAsync(x => x.StripePaymentIntentId == paymentIntent.Id, q => q.Include(x => x.Appointment)); // Include Appointment

            if (payment == null) return NotFound(new { message = "Payment not found." });

            payment.Status = PaymentStatus.Failed;
            await _unitOfWork.CommitAsync();
            _logger.LogError($"❌ Payment failed for {payment.AppointmentId}.");

            // [تمت الإضافة] إرسال إشعار للمريض بفشل الدفع
            if (payment.Appointment != null)
            {
                await _notificationService.SendAndSaveNotificationAsync(
                    recipientUserId: payment.Appointment.PatientId,
                    title: "Payment Failed",
                    message: "We were unable to process your payment for the recent appointment. Please try again or use a different payment method."
                );
            }

            return Ok();
        }
        private async Task<IActionResult> HandleRefundUpdated(Event stripeEvent)
        {
            var charge = stripeEvent.Data.Object as Charge;
            if (charge == null) return BadRequest();

            var payment = await _unitOfWork.Repository<Payment>()
                .FirstOrDefaultAsync(x => x.StripePaymentIntentId == charge.PaymentIntentId);

            if (payment == null) return NotFound(new { message = "Payment not found." });

            var appointment = await _unitOfWork.GetCustomRepository<IAppointmentRepositoryAsync>()
                .FirstOrDefaultAsync(x => x.Id == payment.AppointmentId, query => query.Include(x => x.Payment));

            if (charge.Refunds.Any(r => r.Status == "succeeded"))
            {
                payment.Status = PaymentStatus.Refunded;
                if (appointment != null)
                {
                    appointment.PaymentStatus = PaymentStatus.Refunded;
                }
            }
            else
            {
                payment.Status = PaymentStatus.RefundFailed;
            }

            await _unitOfWork.CommitAsync();
            _logger.LogInfo($"Refund update processed for {payment.AppointmentId}, Status: {payment.Status}");

            // [تم التعديل] تفعيل إشعار استرداد المبلغ للمريض
            if (payment.Status == PaymentStatus.Refunded && appointment != null)
            {
                await _notificationService.SendAndSaveNotificationAsync(
                    recipientUserId: appointment.PatientId,
                    title: "Refund Processed",
                    message: $"Your refund for appointment (ID: {appointment.Id}) has been successfully processed."
                );
            }

            return Ok();
        }

    }
}
