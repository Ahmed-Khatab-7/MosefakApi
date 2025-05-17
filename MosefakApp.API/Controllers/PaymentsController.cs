namespace MosefakApp.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PaymentsController : ControllerBase
    {
        private readonly IConfiguration _configuration;
        private readonly ILoggerService _logger;
        private readonly IUnitOfWork _unitOfWork;
        public PaymentsController(IConfiguration configuration, ILoggerService logger, IUnitOfWork unitOfWork)
        {
            _configuration = configuration;
            _logger = logger;
            _unitOfWork = unitOfWork;
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

            // ✅ Update Payment & Appointment Status
            payment.Status = PaymentStatus.Paid;

            var appointment = await _unitOfWork.GetCustomRepository<IAppointmentRepositoryAsync>()
                .FirstOrDefaultAsync(x => x.Id == payment.AppointmentId, query => query.Include(x => x.Payment));

            if (appointment != null)
            {
                appointment.AppointmentStatus = AppointmentStatus.Confirmed;
                appointment.PaymentStatus = PaymentStatus.Paid;
                appointment.ConfirmedAt = DateTimeOffset.UtcNow;
                appointment.Payment = payment;
            }

            await _unitOfWork.CommitAsync();
            _logger.LogInfo($"✅ Payment successful for {payment.AppointmentId}.");

            return Ok();
        }

        private async Task<IActionResult> HandlePaymentFailed(Event stripeEvent)
        {
            var paymentIntent = stripeEvent.Data.Object as PaymentIntent;
            if (paymentIntent == null) return BadRequest();

            var payment = await _unitOfWork.Repository<Payment>()
                .FirstOrDefaultAsync(x => x.StripePaymentIntentId == paymentIntent.Id);

            if (payment == null) return NotFound(new { message = "Payment not found." });

            // ❌ Mark Payment as Failed
            payment.Status = PaymentStatus.Failed;
            await _unitOfWork.CommitAsync();

            _logger.LogError($"❌ Payment failed for {payment.AppointmentId}.");

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
                    // Preserve the original cancellation reason (Doctor or Patient)
                    if (appointment.AppointmentStatus == AppointmentStatus.CanceledByDoctor ||
                        appointment.AppointmentStatus == AppointmentStatus.CanceledByPatient)
                    {
                        appointment.CancelledAt = DateTimeOffset.UtcNow;
                        appointment.PaymentStatus = PaymentStatus.Refunded; // ✅ Update PaymentStatus
                    }
                }
            }
            else
            {
                payment.Status = PaymentStatus.RefundFailed;
            }

            await _unitOfWork.CommitAsync();
            _logger.LogInfo($"Refund update processed for {payment.AppointmentId}, Status: {payment.Status}");

            // ✅ Notify patient about refund status
            if (payment.Status == PaymentStatus.Refunded && appointment != null)
            {
                //await _notificationService.SendNotificationAsync(appointment.PatientId,
                //    $"Your refund for appointment {appointment.Id} has been successfully processed.");
            }

            return Ok();
        }


    }
}
