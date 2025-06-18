using MosefakApp.Core.Dtos.Payment;
using static MosefakApp.Infrastructure.constants.Permissions;

namespace MosefakApi.Business.Services
{
    public class AppointmentService : IAppointmentService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly UserManager<AppUser> _userManager;
        private readonly ICacheService _cacheService;
        private readonly IStripeService _stripeService;
        private readonly IMapper _mapper;
        private readonly IConfiguration _configuration;
        private readonly ILoggerService _loggerService;
        private readonly IIdProtectorService _Protector;
        private readonly INotificationService _notificationService;
        private readonly string _baseUrl;

        public AppointmentService(
            IUnitOfWork unitOfWork, UserManager<AppUser> userManager, ICacheService cacheService,
            IStripeService stripeService, IMapper mapper, ILoggerService loggerService, IIdProtectorService protector, IConfiguration configuration, INotificationService notificationService)
        {
            _unitOfWork = unitOfWork;
            _userManager = userManager;
            _cacheService = cacheService;
            _stripeService = stripeService;
            _mapper = mapper;
            _configuration = configuration;
            _loggerService = loggerService;
            _Protector = protector;
            _baseUrl = _configuration["BaseUrl"] ?? "https://default-url.com/";
            _notificationService = notificationService;
        }

        public async Task<(List<AppointmentResponse> appointmentResponses, int totalPages)> GetPatientAppointments(
            int userIdFromClaims, AppointmentStatus? status = null,
            int pageNumber = 1, int pageSize = 10, CancellationToken cancellationToken = default)
        {
            (var appointments, var totalPages) = await FetchPatientAppointments(userIdFromClaims, status, pageNumber, pageSize);

            if (!appointments.Any()) return (new List<AppointmentResponse>(), totalPages);

            var doctorAppUserIds = appointments.Select(a => a.Doctor.AppUserId).Distinct().ToList();
            var doctorDetails = await FetchDoctorDetails(doctorAppUserIds, cancellationToken);

            return (appointments.Select(a => MapAppointmentResponse(a, doctorDetails)).ToList(), totalPages);
        }

        public async Task<(List<AppointmentResponse> appointmentResponses, int totalPages)> GetDoctorAppointments(
          int doctorId,
          AppointmentStatus? status = null,
          int pageNumber = 1,
          int pageSize = 10,
          CancellationToken cancellationToken = default)
        {
            (var appointments, var totalPages) = await FetchDoctorAppointments(doctorId, status, pageNumber, pageSize);
            if (!appointments.Any()) return (new List<AppointmentResponse>(), totalPages);

            var doctorAppUserIds = appointments.Select(a => a.Doctor.AppUserId).Distinct().ToList();
            var patientAppUserIds = appointments.Select(a => a.PatientId).Distinct().ToList();

            var doctorDetails = await FetchDoctorDetails(doctorAppUserIds, cancellationToken);
            var patientDetails = await FetchPatientDetails(patientAppUserIds, cancellationToken);

            return (
                appointments.Select(a => MapAppointmentResponse(a, doctorDetails, patientDetails)).ToList(),
                totalPages
            );
        }

        #region
        public async Task<(List<AppointmentPatientResponse> appointmentResponses, int totalPages)> GetDoctorAppointmentsWithPatientData(int doctorId, AppointmentStatus? status = null,
           int pageNumber = 1, int pageSize = 10, CancellationToken cancellationToken = default)
        {
            (var appointments, var totalPages) = await FetchDoctorAppointments(doctorId, status, pageNumber, pageSize);
            if (!appointments.Any()) return (new List<AppointmentPatientResponse>(), totalPages);

            var patientIds = appointments.Select(a => a.PatientId).Distinct().ToList();

            var patientDetails = await FetchPatientDetails(patientIds, cancellationToken);

            return (appointments.Select(a => MapAppointmentPatientResponse(a, patientDetails)).ToList(), totalPages);
        }
        #endregion

        public async Task<bool> DeletePayment(int paymentId)
        {
            var payment = await _unitOfWork.Repository<Payment>().FirstOrDefaultAsync(x => x.Id == paymentId);

            if (payment is null)
                throw new ItemNotFound("payment is not found");

            await _unitOfWork.Repository<Payment>().DeleteEntityAsync(payment);
            await _unitOfWork.CommitAsync();

            return true;
        }

        public async Task<AppointmentResponse> GetAppointmentById(int appointmentId, CancellationToken cancellationToken = default)
        {
            var appointment = await _unitOfWork.Repository<Appointment>()
                .FirstOrDefaultAsync(x => x.Id == appointmentId, query => query.Include(x => x.AppointmentType).Include(x => x.Doctor).ThenInclude(x => x.Specializations));

            if (appointment == null) return new AppointmentResponse();

            var doctorDetails = await FetchDoctorDetails(new List<int> { appointment.Doctor.AppUserId }, cancellationToken);

            return MapAppointmentResponse(appointment, doctorDetails);
        }

        public async Task<bool> CancelAppointmentByPatient(int patientId, int appointmentId, string? cancellationReason, CancellationToken cancellationToken = default)
        {
            // [إصلاح] تم إضافة Include(x => x.Doctor) لجلب بيانات الطبيب اللازمة للإشعار
            var appointment = await _unitOfWork.GetCustomRepository<IAppointmentRepositoryAsync>()
                .FirstOrDefaultAsync(x => x.Id == appointmentId && x.PatientId == patientId,
                                     query => query.Include(x => x.Payment).Include(x => x.Doctor))
                .ConfigureAwait(false);

            if (appointment == null)
            {
                _loggerService.LogWarning("Attempted cancellation for non-existent or unauthorized appointment {AppointmentId} by patient {PatientId}.", appointmentId, patientId);
                throw new ItemNotFound("Appointment does not exist or you don't have permission.");
            }

            if (!IsCancellable(appointment))
            {
                _loggerService.LogWarning("Attempted to cancel an invalid appointment {AppointmentId} by patient {PatientId}.", appointmentId, patientId);
                throw new BadRequest("Appointment cannot be canceled.");
            }

            appointment.AppointmentStatus = AppointmentStatus.CanceledByPatient;
            appointment.CancelledAt = DateTimeOffset.UtcNow;
            appointment.CancellationReason = !string.IsNullOrWhiteSpace(cancellationReason) ? cancellationReason : null;

            if (appointment.Payment?.Status == PaymentStatus.Paid)
            {
                bool refundSucceeded = await _stripeService.RefundPayment(appointment.Payment.StripePaymentIntentId)
                    .ConfigureAwait(false);

                if (!refundSucceeded)
                {
                    _loggerService.LogError("Refund failed for PaymentIntent {PaymentIntentId} on appointment {AppointmentId}.",
                        appointment.Payment.StripePaymentIntentId, appointmentId);
                    throw new Exception("Failed to process payment refund.");
                }

                appointment.Payment.Status = PaymentStatus.Refunded;
            }

            await _unitOfWork.CommitAsync(cancellationToken);
            await _cacheService.RemoveCachedResponseAsync("/api/Appointments/canceled-appointments").ConfigureAwait(false);
            _loggerService.LogInfo("Appointment {AppointmentId} was canceled by patient {PatientId}.", appointmentId, patientId);

            // [تم التعديل] استبدال الدالة المساعدة القديمة بسطر واحد يرسل الإشعار للطبيب
            await _notificationService.SendAndSaveNotificationAsync(
                recipientUserId: appointment.Doctor.AppUserId,
                title: "Appointment Canceled",
                message: $"An appointment (ID: {appointment.Id}) has been canceled by the patient. Reason: {cancellationReason ?? "Not specified."}",
                cancellationToken: cancellationToken
            );

            return true;
        }

        public async Task<(List<AppointmentResponse> appointmentResponses, int totalPages)> GetAppointmentsByDateRange(
         int patientId,
         DateTimeOffset startDate,
         DateTimeOffset endDate,
         int pageNumber = 1,  // Added pagination parameters
         int pageSize = 10,
         CancellationToken cancellationToken = default)
        {
            // Ensure the time part is considered to avoid extra records.
            var startOfDay = startDate.Date;
            var endOfDay = endDate.Date.AddDays(1).AddTicks(-1);

            (var appointments, var totalCount) = await _unitOfWork.Repository<Appointment>()
                .GetAllAsync(
                    x => x.PatientId == patientId &&
                         x.CreatedAt >= startOfDay && x.CreatedAt <= endOfDay,
                    query => query
                        .Include(x => x.AppointmentType)
                        .Include(x => x.Doctor)
                        .ThenInclude(x => x.Specializations),
                    pageNumber, // ✅ Pass page number
                    pageSize // ✅ Pass page size
                );

            // Calculate total pages
            int totalPages = (int)Math.Ceiling((double)totalCount / pageSize);

            if (!appointments.Any())
                return (new List<AppointmentResponse>(), totalPages);

            // Extract distinct doctor AppUserIds from the appointments.
            var doctorAppUserIds = appointments
                .Select(a => a.Doctor.AppUserId)
                .Distinct()
                .ToList();

            // Fetch doctor details from the identity database.
            var doctorDetails = await FetchDoctorDetails(doctorAppUserIds, cancellationToken);

            // Map appointments to response DTOs.
            return (appointments.Select(a => MapAppointmentResponse(a, doctorDetails)).ToList(), totalPages);
        }


        public async Task<(List<AppointmentResponse> appointmentResponses, int totalPages)> GetAppointmentsByDateRangeForDoctor(
           int doctorId, DateTimeOffset startDate, DateTimeOffset endDate,
           int pageNumber = 1, int pageSize = 10, CancellationToken cancellationToken = default)
        {
            // Convert startDate and endDate to cover the full day
            var startOfDay = startDate.Date; // Converts to `2025-02-25T00:00:00`
            var endOfDay = startDate.Date.AddDays(1).AddTicks(-1); // Converts to `2025-02-25T23:59:59.999`

            (var appointments,var totalCount) = await _unitOfWork.Repository<Appointment>()
                .GetAllAsync(
                    x => x.Doctor.AppUserId == doctorId &&
                         x.StartDate >= startOfDay && x.StartDate <= endOfDay, // ✅ Fixed filtering
                    query => query.Include(x => x.AppointmentType).Include(x => x.Doctor).ThenInclude(x => x.Specializations),
                    pageNumber,
                    pageSize);

            // Calculate total pages
            int totalPages = (int)Math.Ceiling((double)totalCount / pageSize);

            if (!appointments.Any())
                return (new List<AppointmentResponse>(), totalPages);

            // Extract distinct doctor AppUserIds from the appointments.
            var doctorAppUserIds = appointments
                .Select(a => a.Doctor.AppUserId)
                .Distinct()
                .ToList();

            // Fetch doctor details from the identity database.
            var doctorDetails = await FetchDoctorDetails(doctorAppUserIds, cancellationToken);

            // Map appointments to response DTOs.
            return (appointments.Select(a => MapAppointmentResponse(a, doctorDetails)).ToList(), totalPages);
        }


        public async Task<bool> RescheduleAppointmentAsync(int appointmentId, DateTime selectedDate, TimeSlot newTimeSlot)
        {
            var strategy = _unitOfWork.CreateExecutionStrategy(); // Use EF Core Execution Strategy

            return await strategy.ExecuteAsync(async () =>
            {
                await using var transaction = await _unitOfWork.BeginTransactionAsync();

                try
                {
                    var appointmentRepo = _unitOfWork.GetCustomRepository<IAppointmentRepositoryAsync>();

                    // [إصلاح] تم إضافة Include لجلب بيانات الطبيب اللازمة للإشعار
                    var appointment = await appointmentRepo.FirstOrDefaultAsync(x => x.Id == appointmentId, q => q.Include(x => x.Doctor));

                    if (appointment is null)
                    {
                        _loggerService.LogWarning("Attempted to reschedule non-existent appointment {AppointmentId}.", appointmentId);
                        throw new ItemNotFound("Appointment does not exist.");
                    }

                    if (appointment.AppointmentStatus == AppointmentStatus.Completed ||
                        appointment.AppointmentStatus == AppointmentStatus.CanceledByPatient ||
                        appointment.AppointmentStatus == AppointmentStatus.CanceledByDoctor)
                    {
                        _loggerService.LogWarning("Attempted to reschedule a canceled or completed appointment {AppointmentId}.", appointmentId);
                        throw new BadRequest("Cannot reschedule a canceled or completed appointment.");
                    }

                    if (newTimeSlot.EndTime < newTimeSlot.StartTime)
                        throw new BadRequest("Invalid time slot. End time must be after start time.");

                    var startTimeOffset = new DateTimeOffset(
                        selectedDate.Year, selectedDate.Month, selectedDate.Day,
                        newTimeSlot.StartTime.Hour, newTimeSlot.StartTime.Minute, 0,
                        TimeZoneInfo.Local.GetUtcOffset(DateTime.Now));

                    var endTimeOffset = startTimeOffset.Add(newTimeSlot.EndTime - newTimeSlot.StartTime);

                    if (!await appointmentRepo.IsTimeSlotAvailable(appointment.DoctorId, startTimeOffset, endTimeOffset))
                    {
                        _loggerService.LogWarning("Time slot unavailable for rescheduling appointment {AppointmentId}.", appointmentId);
                        throw new InvalidOperationException("The selected time slot is already booked.");
                    }

                    appointment.StartDate = startTimeOffset;
                    appointment.EndDate = endTimeOffset;

                    await appointmentRepo.UpdateEntityAsync(appointment);
                    await _unitOfWork.CommitAsync();

                    _loggerService.LogInfo("Appointment {AppointmentId} successfully rescheduled to {NewDate}.",
                        appointmentId, startTimeOffset);

                    await transaction.CommitAsync();

                    // [تم التعديل] استبدال الكود القديم بسطر واحد يرسل الإشعار للطبيب
                    await _notificationService.SendAndSaveNotificationAsync(
                        recipientUserId: appointment.Doctor.AppUserId,
                        title: "Appointment Rescheduled",
                        message: $"An appointment (ID: {appointment.Id}) has been rescheduled by the patient to {startTimeOffset:dd/MM/yyyy HH:mm}."
                    );

                    _ = _cacheService.RemoveCachedResponseAsync("/api/appointments/upcoming");

                    return true;
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    _loggerService.LogError("Failed to reschedule appointment {AppointmentId}: {ErrorMessage}",
                        appointmentId, ex.Message);
                    throw new Exception($"Failed to reschedule appointment {appointmentId}: {ex.Message}", ex);
                }
            });
        }

        public async Task<bool> BookAppointment(BookAppointmentRequest request, int appUserIdFromClaims, CancellationToken cancellationToken = default)
        {
            var strategy = _unitOfWork.CreateExecutionStrategy();

            return await strategy.ExecuteAsync(async () =>
            {
                await using var transaction = await _unitOfWork.BeginTransactionAsync().ConfigureAwait(false);

                try
                {
                    var doctor = await _unitOfWork.GetCustomRepository<IDoctorRepositoryAsync>()
                        .FirstOrDefaultAsync(x => x.Id == int.Parse(request.DoctorId));

                    if (doctor == null)
                    {
                        throw new ItemNotFound("Doctor does not exist.");
                    }

                    var appointmentType = await _unitOfWork.Repository<AppointmentType>()
                        .FirstOrDefaultAsync(x => x.Id == int.Parse(request.AppointmentTypeId) && x.DoctorId == int.Parse(request.DoctorId));

                    if (appointmentType == null)
                    {
                        throw new ItemNotFound("This appointment type does not exist for this doctor.");
                    }

                    var endTime = request.StartDate.Add(appointmentType.Duration);

                    if (!await IsTimeSlotAvailable(doctor.Id, request.StartDate, endTime).ConfigureAwait(false))
                    {
                        throw new BadRequest("Cannot book appointment; the selected time slot is already booked. Please try another time.");
                    }

                    var appointment = new Appointment
                    {
                        PatientId = appUserIdFromClaims,
                        DoctorId = int.Parse(request.DoctorId),
                        AppointmentStatus = AppointmentStatus.PendingApproval,
                        AppointmentTypeId = int.Parse(request.AppointmentTypeId),
                        PaymentStatus = PaymentStatus.Pending,
                        StartDate = request.StartDate,
                        EndDate = endTime,
                        ProblemDescription = !string.IsNullOrEmpty(request.ProblemDescription) ? request.ProblemDescription : null,
                    };

                    await _unitOfWork.GetCustomRepository<IAppointmentRepositoryAsync>().AddEntityAsync(appointment).ConfigureAwait(false);

                    if (await _unitOfWork.CommitAsync(cancellationToken).ConfigureAwait(false) <= 0)
                        throw new Exception("Failed to book appointment.");

                    (string paymentIntentId, string clientSecret) = await _stripeService.GetPaymentIntentId(
                        appointmentType.ConsultationFee,
                        appUserIdFromClaims.ToString(),
                        appointment.Id.ToString()
                    );

                    var payment = new Payment
                    {
                        Amount = appointmentType.ConsultationFee,
                        Currency = "usd",
                        PaymentMethod = MosefakApp.Domains.Enums.PaymentMethod.Stripe,
                        Status = PaymentStatus.Pending,
                        AppointmentId = appointment.Id,
                        StripePaymentIntentId = paymentIntentId,
                        ClientSecret = clientSecret,
                    };

                    await _unitOfWork.Repository<Payment>().AddEntityAsync(payment);

                    if (await _unitOfWork.CommitAsync(cancellationToken).ConfigureAwait(false) <= 0)
                        throw new Exception("Failed to save payment information.");

                    await transaction.CommitAsync(cancellationToken).ConfigureAwait(false);

                    // [تم التعديل] إرسال إشعار للطبيب بوجود طلب حجز جديد
                    await _notificationService.SendAndSaveNotificationAsync(
                        recipientUserId: doctor.AppUserId,
                        title: "New Booking Request",
                        message: "You have received a new appointment request that requires your approval.",
                        cancellationToken: cancellationToken
                    );

                    return true;
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    _loggerService.LogError($"Failed to book appointment: {ex.Message}", ex);
                    throw;
                }
            });
        }



        private async Task<bool> IsTimeSlotAvailable(int doctorId, DateTimeOffset startDate, DateTimeOffset endDate)
        {
            var query = await _unitOfWork.GetCustomRepository<IAppointmentRepositoryAsync>().IsTimeSlotAvailable(doctorId, startDate, endDate);

            return query;
        }

        public async Task<bool> ApproveAppointmentByDoctor(int appointmentId)
        {
            var strategy = _unitOfWork.CreateExecutionStrategy();

            return await strategy.ExecuteAsync(async () =>
            {
                await using var transaction = await _unitOfWork.BeginTransactionAsync();

                try
                {
                    var appointmentRepo = _unitOfWork.GetCustomRepository<IAppointmentRepositoryAsync>();
                    var appointment = await appointmentRepo.GetByIdAsync(appointmentId);

                    if (appointment == null)
                    {
                        _loggerService.LogWarning($"Approval failed: Appointment {appointmentId} not found.");
                        throw new ItemNotFound("Appointment does not exist.");
                    }

                    if (appointment.AppointmentStatus != AppointmentStatus.PendingApproval)
                    {
                        _loggerService.LogWarning($"Invalid status: Appointment {appointmentId} cannot be approved.");
                        throw new BadRequest("Appointment is not in a pending approval state.");
                    }

                    appointment.AppointmentStatus = AppointmentStatus.PendingPayment;
                    appointment.ApprovedByDoctor = true;
                    appointment.PaymentDueTime = DateTime.UtcNow.AddHours(24);

                    await appointmentRepo.UpdateEntityAsync(appointment);
                    if (await _unitOfWork.CommitAsync() <= 0)
                        throw new Exception("Failed to approve appointment.");

                    await transaction.CommitAsync();
                    _loggerService.LogInfo($"Appointment {appointmentId} approved successfully.");

                    // [تم التعديل] استبدال الكود القديم بسطر واحد يرسل الإشعار للمريض
                    await _notificationService.SendAndSaveNotificationAsync(
                        recipientUserId: appointment.PatientId,
                        title: "Appointment Approved",
                        message: "Your appointment request has been approved. Please complete the payment within 24 hours to confirm your session."
                    );

                    return true;
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    _loggerService.LogError($"Failed to approve appointment {appointmentId}: {ex.Message}");
                    throw;
                }
            });
        }


        public async Task<bool> RejectAppointmentByDoctor(int appointmentId, string? rejectionReason)
        {
            var strategy = _unitOfWork.CreateExecutionStrategy();

            return await strategy.ExecuteAsync(async () =>
            {
                await using var transaction = await _unitOfWork.BeginTransactionAsync();

                try
                {
                    var appointmentRepo = _unitOfWork.GetCustomRepository<IAppointmentRepositoryAsync>();
                    var appointment = await appointmentRepo.GetByIdAsync(appointmentId);

                    if (appointment == null || appointment.AppointmentStatus != AppointmentStatus.PendingApproval)
                    {
                        _loggerService.LogWarning($"Invalid rejection: Appointment {appointmentId} not found or already processed.");
                        throw new BadRequest("Invalid appointment or already processed.");
                    }

                    appointment.AppointmentStatus = AppointmentStatus.CanceledByDoctor;
                    appointment.CancelledAt = DateTime.UtcNow;
                    appointment.CancellationReason = rejectionReason;

                    await appointmentRepo.UpdateEntityAsync(appointment);
                    if (await _unitOfWork.CommitAsync() <= 0)
                        throw new Exception("Failed to reject appointment.");

                    await transaction.CommitAsync();
                    _loggerService.LogInfo($"Appointment {appointmentId} rejected successfully.");

                    // [تم التعديل] إرسال إشعار للمريض بالرفض
                    await _notificationService.SendAndSaveNotificationAsync(
                        recipientUserId: appointment.PatientId,
                        title: "Appointment Rejected",
                        message: $"Unfortunately, your appointment request has been rejected. Reason: {rejectionReason ?? "Not specified."}"
                    );

                    return true;
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    _loggerService.LogError($"Failed to reject appointment {appointmentId}: {ex.Message}");
                    throw;
                }
            });
        }

        public async Task<(List<AppointmentResponse> appointmentResponses, int totalPages)> GetPendingAppointmentsForDoctor(
            int doctorId, int pageNumber = 1, int pageSize = 10, CancellationToken cancellationToken = default)
        {
            // Retrieve appointments with necessary navigations: AppointmentType, Doctor, and Doctor.Specializations.
            (var appointments, var totalCount) = await _unitOfWork.Repository<Appointment>()
                .GetAllAsync(
                    x => x.Doctor.AppUserId == doctorId && x.AppointmentStatus == AppointmentStatus.PendingApproval,
                    query => query.Include(x => x.AppointmentType).Include(x => x.Doctor).ThenInclude(x => x.Specializations),
                    pageNumber,
                    pageSize);

            // Calculate total pages
            int totalPages = (int)Math.Ceiling((double)totalCount / pageSize);

            if (!appointments.Any())
                return (new List<AppointmentResponse>(), totalPages);

            // Extract distinct doctor AppUserIds from the appointments.
            var doctorAppUserIds = appointments
                .Select(a => a.Doctor.AppUserId)
                .Distinct()
                .ToList();

            // Fetch doctor details from the identity database.
            var doctorDetails = await FetchDoctorDetails(doctorAppUserIds, cancellationToken);

            // Map appointments to response DTOs.
            return (appointments.Select(a => MapAppointmentResponse(a, doctorDetails)).ToList(), totalPages);
        }

        public async Task<bool> MarkAppointmentAsCompleted(int appointmentId)
        {
            var strategy = _unitOfWork.CreateExecutionStrategy();

            return await strategy.ExecuteAsync(async () =>
            {
                await using var transaction = await _unitOfWork.BeginTransactionAsync();

                try
                {
                    var appointmentRepo = _unitOfWork.GetCustomRepository<IAppointmentRepositoryAsync>();
                    var appointment = await appointmentRepo.GetByIdAsync(appointmentId);

                    if (appointment == null || appointment.AppointmentStatus != AppointmentStatus.Confirmed)
                    {
                        _loggerService.LogWarning($"Completion failed: Appointment {appointmentId} not found or not confirmed.");
                        throw new BadRequest("Invalid appointment status.");
                    }

                    appointment.AppointmentStatus = AppointmentStatus.Completed;
                    appointment.CompletedAt = DateTime.UtcNow;
                    appointment.ServiceProvided = true;

                    await appointmentRepo.UpdateEntityAsync(appointment);
                    if (await _unitOfWork.CommitAsync() <= 0)
                        throw new Exception("Failed to mark appointment as completed.");

                    await transaction.CommitAsync();
                    _loggerService.LogInfo($"Appointment {appointmentId} marked as completed.");

                    // [تم التعديل] إرسال إشعار للمريض باكتمال الموعد
                    await _notificationService.SendAndSaveNotificationAsync(
                        recipientUserId: appointment.PatientId,
                        title: "Appointment Completed",
                        message: "Your appointment has been marked as completed. We hope you had a great experience!"
                    );

                    return true;
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    _loggerService.LogError($"Failed to complete appointment {appointmentId}: {ex.Message}");
                    throw;
                }
            });
        }
        public async Task<string> CreatePaymentIntent(int appointmentId, CancellationToken cancellationToken = default)
        {
            var appointment = await _unitOfWork.GetCustomRepository<IAppointmentRepositoryAsync>()
                .FirstOrDefaultAsync(x => x.Id == appointmentId, query => query.Include(x => x.AppointmentType).Include(x => x.Payment))
                .ConfigureAwait(false);

            if (appointment == null)
                throw new ItemNotFound("Appointment does not exist.");

            if (appointment.AppointmentStatus != AppointmentStatus.PendingPayment)
                throw new BadRequest("Invalid appointment status for payment.");

            decimal amountToCharge = appointment.AppointmentType.ConsultationFee;

            // 🔍 Check if a PaymentIntent already exists for this appointment
            var existingPayment = await _unitOfWork.Repository<Payment>()
                .FirstOrDefaultAsync(x => x.AppointmentId == appointmentId);

            if (existingPayment != null && existingPayment.Status == PaymentStatus.Pending)
            {
                return existingPayment.ClientSecret; // ✅ Return existing ClientSecret (Safe for frontend)
            }

            var protectedAppointmentId = ProtectId(appointmentId.ToString());
            var protectedPatinetId = ProtectId(appointment.PatientId.ToString());

            // 🔹 Generate new PaymentIntent from Stripe
            (string paymentIntentId, string clientSecret) = await _stripeService.GetPaymentIntentId(amountToCharge, protectedPatinetId, protectedAppointmentId)
                .ConfigureAwait(false);

            if (string.IsNullOrEmpty(paymentIntentId))
                throw new Exception("Failed to generate PaymentIntent.");

            // 🔹 Save PaymentIntentId and ClientSecret in the database
            var payment = new Payment
            {
                AppointmentId = appointmentId,
                StripePaymentIntentId = paymentIntentId, // 🔥 Stored securely in DB
                ClientSecret = clientSecret, // 🔥 Safe for frontend use
                Amount = amountToCharge,
                Status = PaymentStatus.Pending
            };

            await _unitOfWork.Repository<Payment>().AddEntityAsync(payment);
            var rowsAffected = await _unitOfWork.CommitAsync(cancellationToken);

            if (rowsAffected <= 0)
                throw new Exception("Payment operation failed.");

            return clientSecret;
        }

        public async Task<bool> ConfirmAppointmentPayment(int appointmentId, CancellationToken cancellationToken = default)
        {
            var strategy = _unitOfWork.CreateExecutionStrategy(); // ✅ Use EF Core Execution Strategy

            return await strategy.ExecuteAsync(async () =>
            {
                await using var transaction = await _unitOfWork.BeginTransactionAsync();

                var payment = await _unitOfWork.Repository<Payment>()
                    .FirstOrDefaultAsync(x => x.AppointmentId == appointmentId);

                if (payment == null)
                    throw new ItemNotFound("Payment record not found.");

                var paymentIntentId = payment.StripePaymentIntentId;
                var paymentStatus = await _stripeService.VerifyPaymentStatus(paymentIntentId);

                if (paymentStatus == "error")
                    throw new Exception("Error verifying payment.");

                if (paymentStatus != "succeeded")
                    return false; // Payment not completed

                // ✅ Update Payment & Appointment Status
                payment.Status = PaymentStatus.Paid;

                var appointment = await _unitOfWork.GetCustomRepository<IAppointmentRepositoryAsync>()
                    .FirstOrDefaultAsync(x => x.Id == appointmentId, query => query.Include(x => x.Payment));

                if (appointment != null)
                {
                    appointment.AppointmentStatus = AppointmentStatus.Confirmed;
                    appointment.Payment = payment;
                }

                await _unitOfWork.CommitAsync(cancellationToken);
                await transaction.CommitAsync(); // ✅ Commit inside execution strategy

                return true;
            });
        }

        public async Task<PaginatedResponse<PaymentResponse>> GetPayments(int pageNumber = 1, int pageSize = 10)
        {
            // Fetch payments and total count with included Appointment
            (var payments, var totalCount) = await _unitOfWork.Repository<Payment>()
                .GetAllAsync(x => x.Include(x => x.Appointment), pageNumber, pageSize);

            // Prepare paginated response
            var response = new PaginatedResponse<PaymentResponse>()
            {
                CurrentPage = pageNumber,
                PageSize = pageSize,
                TotalPages = (int)Math.Ceiling((double)totalCount / pageSize)
            };

            // If no payments, return empty list
            if (!payments.Any())
            {
                response.Data = new List<PaymentResponse>();
                return response;
            }

            // Gather unique patient IDs from appointments
            var usersIds = payments.Select(x => x.Appointment.PatientId).ToHashSet();

            // Fetch user details (FullName, Image) for these patients
            var userDetails = await _unitOfWork.GetCustomRepository<IDoctorRepositoryAsync>()
                .GetUserDetailsAsync(usersIds);

            // Map payments to PaymentResponse, joining user details
            response.Data = payments.Select(payment =>
            {
                var patientId = payment.Appointment.PatientId;
                var (fullName, imagePath) = userDetails.TryGetValue(patientId, out var details)
                    ? details
                    : (string.Empty, string.Empty);

                return new PaymentResponse
                {
                    Id = payment.Id.ToString(),
                    AppointmentId = payment.AppointmentId.ToString(),
                    Amount = payment.Amount,
                    Status = payment.Status,
                    FullName = fullName,
                    Image = _baseUrl + imagePath,
                    CreatedAt = payment.CreatedAt
                };
            }).ToList();

            return response;
        }


        public async Task<AppointmentStatus> GetAppointmentStatus(int appointmentId)
        {
            var appointment = await _unitOfWork.Repository<Appointment>().FirstOrDefaultAsync(x=> x.Id == appointmentId);

            if (appointment == null)
                throw new ItemNotFound("appointment does not exist");

            return appointment.AppointmentStatus;
        }

        public async Task<bool> CancelAppointmentByDoctor(int appointmentId, string? cancellationReason, CancellationToken cancellationToken = default)
        {
            var appointment = await _unitOfWork.GetCustomRepository<IAppointmentRepositoryAsync>()
                .FirstOrDefaultAsync(x => x.Id == appointmentId, query => query.Include(x => x.Payment))
                .ConfigureAwait(false);

            if (appointment == null)
            {
                _loggerService.LogWarning("Appointment with ID {AppointmentId} not found.", appointmentId);
                throw new ItemNotFound("Appointment does not exist.");
            }

            if (appointment.AppointmentStatus != AppointmentStatus.Confirmed)
            {
                _loggerService.LogWarning("Attempt to cancel appointment {AppointmentId} with invalid status: {Status}.",
                    appointmentId, appointment.AppointmentStatus);
                throw new BadRequest("Only confirmed appointments can be canceled by the doctor.");
            }

            appointment.AppointmentStatus = AppointmentStatus.CanceledByDoctor;
            appointment.CancelledAt = DateTimeOffset.UtcNow;
            appointment.CancellationReason = cancellationReason ?? appointment.CancellationReason;

            if (appointment.Payment?.Status == PaymentStatus.Paid)
            {
                bool refundSucceeded = await _stripeService.RefundPayment(appointment.Payment.StripePaymentIntentId)
                    .ConfigureAwait(false);

                if (!refundSucceeded)
                {
                    _loggerService.LogError("Refund failed for PaymentIntent {PaymentIntentId} on appointment {AppointmentId}.",
                        appointment.Payment.StripePaymentIntentId, appointmentId);
                    throw new Exception("Failed to process payment refund.");
                }

                appointment.Payment.Status = PaymentStatus.Refunded;
            }

            await _unitOfWork.GetCustomRepository<IAppointmentRepositoryAsync>()
                .UpdateEntityAsync(appointment)
                .ConfigureAwait(false);

            var rowsAffected = await _unitOfWork.CommitAsync()
                .ConfigureAwait(false);

            if (rowsAffected <= 0)
            {
                _loggerService.LogError("Failed to cancel appointment {AppointmentId}.", appointmentId);
                throw new Exception("Failed to cancel appointment.");
            }

            // [تم التعديل] إرسال إشعار للمريض بإلغاء الموعد من جهة الطبيب
            await _notificationService.SendAndSaveNotificationAsync(
                recipientUserId: appointment.PatientId,
                title: "Appointment Canceled by Doctor",
                message: $"We are sorry to inform you that your appointment (ID: {appointmentId}) has been canceled by the doctor. A refund will be processed if applicable.",
                cancellationToken: cancellationToken
            );

            _loggerService.LogInfo("Appointment {AppointmentId} successfully canceled by doctor.", appointmentId);
            return true;
        }


        public async Task AutoCancelUnpaidAppointments() // Scheduled with Hangfire
        {
            try
            {
                _loggerService.LogInfo("Started Auto Cancel Unpaid Appointments job.");

                var appointmentRepo = _unitOfWork.GetCustomRepository<IAppointmentRepositoryAsync>();

                // جلب كل المواعيد التي تمت الموافقة عليها ولكن لم يتم دفعها وتجاوزت المهلة
                var expiredAppointments = await appointmentRepo.GetAllAsync(
                    x => x.PaymentStatus == PaymentStatus.Pending &&
                         x.AppointmentStatus == AppointmentStatus.PendingPayment &&
                         x.PaymentDueTime < DateTimeOffset.UtcNow
                );

                if (!expiredAppointments.Any())
                {
                    _loggerService.LogInfo("No expired unpaid appointments found.");
                    return;
                }

                // احتفظنا بهذا الجزء لأنه فعال ويقوم بتحديث كل المواعيد في قاعدة البيانات مرة واحدة
                await appointmentRepo.ExecuteUpdateAsync(
                    x => expiredAppointments.Select(a => a.Id).Contains(x.Id),
                    x => new Appointment
                    {
                        AppointmentStatus = AppointmentStatus.AutoCanceled,
                        CancelledAt = DateTimeOffset.UtcNow,
                        CancellationReason = "Payment not completed within the required timeframe."
                    });

                _loggerService.LogInfo($"Successfully auto-canceled {expiredAppointments.Count} unpaid appointments.");

                // [تم التعديل] إزالة الكود المعقد القديم واستبداله بحلقة بسيطة
                // ترسل إشعاراً لكل مريض تم إلغاء موعده
                foreach (var appointment in expiredAppointments)
                {
                    await _notificationService.SendAndSaveNotificationAsync(
                        recipientUserId: appointment.PatientId,
                        title: "Appointment Auto-Canceled",
                        message: "Your appointment was automatically canceled because the payment was not completed within the required timeframe."
                    );
                }
            }
            catch (Exception ex)
            {
                _loggerService.LogError($"AutoCancelUnpaidAppointments job failed: {ex.Message}", ex);
                throw;
            }
        }

        //private async Task NotifyDoctorAsync(int doctorId, int appointmentId, DateTimeOffset newDate)
        //{
        //    var doctor = await _unitOfWork.Repository<Doctor>().FirstOrDefaultAsync(d => d.Id == doctorId);

        //    if (doctor == null) return;

        //    var user = await _userManager.Users
        //        .Where(u => u.Id == doctor.AppUserId)
        //        .Select(u => new { u.FirstName, u.LastName, u.FcmToken })
        //        .FirstOrDefaultAsync();

        //    if (!string.IsNullOrEmpty(user?.FcmToken))
        //    {
        //        try
        //        {
        //            await _firebaseService.SendNotificationAsync(
        //                user.FcmToken,
        //                "Appointment Rescheduled",
        //                $"Hi {user.FirstName} {user.LastName}, " +
        //                $"A patient has rescheduled an appointment to {newDate:dd/MM/yyyy HH:mm}.");
        //        }
        //        catch (Exception ex)
        //        {
        //            _loggerService.LogError("Failed to send reschedule notification for appointment {AppointmentId}: {ErrorMessage}",
        //                appointmentId, ex.Message);
        //        }
        //    }
        //}

        private bool IsCancellable(Appointment appointment)
        {
            return appointment.AppointmentStatus switch
            {
                AppointmentStatus.Completed => false,
                AppointmentStatus.CanceledByDoctor or AppointmentStatus.CanceledByPatient => false,
                _ => true
            };
        }

        //private async Task NotifyDoctorAsync(int doctorId, int appointmentId)
        //{
        //    var doctorInfo = await _unitOfWork.Repository<Doctor>().GetByIdAsync(doctorId);

        //    if (doctorInfo == null) return;

        //    var user = await _userManager.Users
        //        .Where(u => u.Id == doctorInfo.AppUserId)
        //        .Select(u => new { u.FcmToken, u.FirstName, u.LastName })
        //        .FirstOrDefaultAsync();

        //    if (user?.FcmToken != null)
        //    {
        //        await _firebaseService.SendNotificationAsync(user.FcmToken, "Cancellation",
        //            $"The patient {user.FirstName} {user.LastName} has canceled appointment {appointmentId}. If payment was made, a refund has been processed.");
        //    }
        //}


        private async Task<(IEnumerable<Appointment> appointments,int totalPages)> FetchPatientAppointments(int userId, AppointmentStatus? status, int pageNumber = 1, int pageSize = 10)
        {
            (var items, var totalCount) = await _unitOfWork.Repository<Appointment>()
                .GetAllAsync(
                    x => x.PatientId == userId && (status == null || x.AppointmentStatus == status),
                    query => query.Include(x => x.AppointmentType).Include(x => x.Doctor).ThenInclude(x => x.Specializations),
                    pageNumber,
                    pageSize)
                .ConfigureAwait(false);

            // Calculate total pages
            int totalPages = (int)Math.Ceiling((double)totalCount / pageSize);

            return (items, totalPages);
        }

        private async Task<(IEnumerable<Appointment> appointments, int totalPages)> FetchDoctorAppointments(int doctorId, AppointmentStatus? status, int pageNumber = 1, int pageSize = 10)
        {
            (var items, var totalCount) = await _unitOfWork.Repository<Appointment>()
                .GetAllAsync(
                    x => x.Doctor.AppUserId == doctorId && (status == null || x.AppointmentStatus == status),
                    query => query.Include(x => x.AppointmentType).Include(x => x.Doctor).ThenInclude(x => x.Specializations),
                    pageNumber,
                    pageSize)
                .ConfigureAwait(false);

            // Calculate total pages
            int totalPages = (int)Math.Ceiling((double)totalCount / pageSize);

            return (items, totalPages);
        }

        private async Task<Dictionary<int, DoctorDetails>> FetchDoctorDetails(List<int> doctorAppUserIds,CancellationToken cancellationToken)
        {
            return await _userManager.Users
                .Where(u => doctorAppUserIds.Contains(u.Id))
                .Select(u => new DoctorDetails
                {
                    Id = u.Id,
                    FirstName = u.FirstName ?? "Unknown",
                    LastName = u.LastName ?? "Unknown",
                    ImagePath = u.ImagePath ?? string.Empty
                })
                .ToDictionaryAsync(u => u.Id, cancellationToken);
        }

        private async Task<Dictionary<int, PatientDetails>> FetchPatientDetails(List<int> patientIds, CancellationToken cancellationToken)
        {
            return await _userManager.Users
                .Where(u => patientIds.Contains(u.Id))
                .Select(u => new PatientDetails
                {
                    Id = u.Id,
                    FirstName = u.FirstName ?? "Unknown",
                    LastName = u.LastName ?? "Unknown",
                    ImagePath = u.ImagePath ?? string.Empty,
                    Email = u.Email ?? string.Empty,
                    PhoneNumber = u.PhoneNumber?? string.Empty
                })
                .ToDictionaryAsync(u => u.Id, cancellationToken);
        }

       
        private string ProtectId(string id) => _Protector.Protect(int.Parse(id));
        private AppointmentResponse MapAppointmentResponse(Appointment appointment, Dictionary<int, DoctorDetails> doctorDetails)
        {
            var doctor = doctorDetails.GetValueOrDefault(appointment.Doctor.AppUserId) ?? new DoctorDetails
            {
                Id = appointment.Doctor.AppUserId,
                FirstName = "Unknown",
                LastName = "Unknown",
                ImagePath = string.Empty
            };

            return new AppointmentResponse
            {
                Id = appointment.Id.ToString(),
                StartDate = appointment.StartDate,
                EndDate = appointment.EndDate,
                AppointmentStatus = appointment.AppointmentStatus,
                AppointmentType = _mapper.Map<AppointmentTypeResponse>(appointment.AppointmentType),
                DoctorId = appointment.Doctor.Id.ToString(),
                DoctorFullName = $"{doctor.FirstName} {doctor.LastName}",
                DoctorImage = !string.IsNullOrEmpty(doctor.ImagePath)
                                ? $"{_baseUrl}{doctor.ImagePath}"
                                : $"{_baseUrl}default.jpg",
                DoctorSpecialization = _mapper.Map<List<SpecializationResponse>>(appointment.Doctor.Specializations ?? new List<Specialization>())
            };
        }

        // Updated mapping method to include patient details
        private AppointmentResponse MapAppointmentResponse(
            Appointment appointment,
            Dictionary<int, DoctorDetails> doctorDetails,
            Dictionary<int, PatientDetails> patientDetails)
        {
            var doctor = doctorDetails.GetValueOrDefault(appointment.Doctor.AppUserId) ?? new DoctorDetails
            {
                Id = appointment.Doctor.AppUserId,
                FirstName = "Unknown",
                LastName = "Unknown",
                ImagePath = string.Empty
            };

            var patient = patientDetails.GetValueOrDefault(appointment.PatientId) ?? new PatientDetails
            {
                Id = appointment.PatientId,
                FirstName = "Unknown",
                LastName = "Unknown",
                ImagePath = string.Empty
            };

            return new AppointmentResponse
            {
                Id = appointment.Id.ToString(),
                StartDate = appointment.StartDate,
                EndDate = appointment.EndDate,
                AppointmentStatus = appointment.AppointmentStatus,
                AppointmentType = _mapper.Map<AppointmentTypeResponse>(appointment.AppointmentType),
                DoctorId = appointment.Doctor.Id.ToString(),
                DoctorFullName = $"{doctor.FirstName} {doctor.LastName}",
                DoctorImage = !string.IsNullOrEmpty(doctor.ImagePath)
                                ? $"{_baseUrl}{doctor.ImagePath}"
                                : $"{_baseUrl}default.jpg",
                DoctorSpecialization = _mapper.Map<List<SpecializationResponse>>(appointment.Doctor.Specializations ?? new List<Specialization>()),
                PatientFullName = $"{patient.FirstName} {patient.LastName}",
                PatientImage = !string.IsNullOrEmpty(patient.ImagePath)
                                ? $"{_baseUrl}{patient.ImagePath}"
                                : $"{_baseUrl}default.jpg"
            };
        }

        private AppointmentPatientResponse MapAppointmentPatientResponse(Appointment appointment, Dictionary<int, PatientDetails> patientDetails)
        {
            var patient = patientDetails.GetValueOrDefault(appointment.PatientId) ?? new PatientDetails
            {
                Id = appointment.Doctor.AppUserId,
                FirstName = "Unknown",
                LastName = "Unknown",
                ImagePath = string.Empty,
                PhoneNumber = "Unknown",
                Email = "Unknown"
            };

            return new AppointmentPatientResponse
            {
                Id = appointment.Id.ToString(),
                Email = patient.Email,
                PhoneNumber = patient.PhoneNumber,
                StartDate = appointment.StartDate,
                EndDate = appointment.EndDate,
                AppointmentStatus = appointment.AppointmentStatus,
                AppointmentType = _mapper.Map<AppointmentTypeResponse>(appointment.AppointmentType),
                PatientId = appointment.PatientId.ToString(),
                FullName = $"{patient.FirstName} {patient.LastName}",
                PatientImage = !string.IsNullOrEmpty(patient.ImagePath)
                                ? $"{_baseUrl}{patient.ImagePath}"
                                : $"{_baseUrl}default.jpg",
            };
        }
    }
}

