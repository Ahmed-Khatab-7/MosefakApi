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
        private readonly IFirebaseService _firebaseService;
        private readonly string _baseUrl;

        public AppointmentService(
            IUnitOfWork unitOfWork, UserManager<AppUser> userManager, ICacheService cacheService,
            IStripeService stripeService, IMapper mapper, ILoggerService loggerService, IIdProtectorService protector, IConfiguration configuration, IFirebaseService firebaseService)
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
            _firebaseService = firebaseService;
        }

        public async Task<(List<AppointmentResponse> appointmentResponses, int totalPages)> GetPatientAppointments(
            int userIdFromClaims, AppointmentStatus? status = null,
            int pageNumber = 1, int pageSize = 10,CancellationToken cancellationToken = default)
        {
            (var appointments,var totalPages) = await FetchPatientAppointments(userIdFromClaims, status, pageNumber, pageSize);

            if (!appointments.Any()) return (new List<AppointmentResponse>(), totalPages);

            var doctorAppUserIds = appointments.Select(a => a.Doctor.AppUserId).Distinct().ToList();
            var doctorDetails = await FetchDoctorDetails(doctorAppUserIds, cancellationToken);

            return (appointments.Select(a => MapAppointmentResponse(a, doctorDetails)).ToList(), totalPages);
        }

        public async Task<(List<AppointmentResponse> appointmentResponses, int totalPages)> GetDoctorAppointments(int doctorId, AppointmentStatus? status = null,
            int pageNumber = 1, int pageSize = 10, CancellationToken cancellationToken = default)
        {
            (var appointments, var totalPages) = await FetchDoctorAppointments(doctorId, status, pageNumber, pageSize);
            if (!appointments.Any()) return (new List<AppointmentResponse>(), totalPages);

            var doctorAppUserIds = appointments.Select(a => a.Doctor.AppUserId).Distinct().ToList();

            var doctorDetails = await FetchDoctorDetails(doctorAppUserIds, cancellationToken);

            return (appointments.Select(a => MapAppointmentResponse(a, doctorDetails)).ToList(), totalPages);
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
            var appointment = await _unitOfWork.GetCustomRepository<IAppointmentRepositoryAsync>()
                .FirstOrDefaultAsync(x => x.Id == appointmentId && x.PatientId == patientId, query => query.Include(x => x.Payment))
                .ConfigureAwait(false);

            if (appointment == null)
            {
                _loggerService.LogWarning("Attempted cancellation for non-existent or unauthorized appointment {AppointmentId} by patient {PatientId}.", appointmentId, patientId);
                throw new ItemNotFound("Appointment does not exist or you don't have permission.");
            }

            // Step 2: Validate If Appointment Can Be Canceled
            if (!IsCancellable(appointment))
            {
                _loggerService.LogWarning("Attempted to cancel an invalid appointment {AppointmentId} by patient {PatientId}.", appointmentId, patientId);
                throw new BadRequest("Appointment cannot be canceled.");
            }

            appointment.AppointmentStatus = AppointmentStatus.CanceledByPatient;
            appointment.CancelledAt = DateTimeOffset.UtcNow;
            appointment.CancellationReason = !string.IsNullOrWhiteSpace(cancellationReason) ? cancellationReason : null;

            // 🔹 Check if a refund is required
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

                // ✅ Update Payment Status
                appointment.Payment.Status = PaymentStatus.Refunded;
            }

            await _unitOfWork.CommitAsync(cancellationToken);
            await _cacheService.RemoveCachedResponseAsync("/api/Appointments/canceled-appointments").ConfigureAwait(false);

            _loggerService.LogInfo("Appointment {AppointmentId} was canceled by patient {PatientId}.", appointmentId, patientId);

            await NotifyDoctorAsync(appointment.DoctorId, appointment.Id);

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
            var strategy = _unitOfWork.CreateExecutionStrategy(); // ✅ Use EF Core Execution Strategy

            return await strategy.ExecuteAsync(async () =>
            {
                await using var transaction = await _unitOfWork.BeginTransactionAsync();

                try
                {
                    var appointmentRepo = _unitOfWork.GetCustomRepository<IAppointmentRepositoryAsync>();

                    // 🔍 Fetch appointment
                    var appointment = await appointmentRepo.FirstOrDefaultAsync(x => x.Id == appointmentId);

                    if (appointment is null)
                    {
                        _loggerService.LogWarning("Attempted to reschedule non-existent appointment {AppointmentId}.", appointmentId);
                        throw new ItemNotFound("Appointment does not exist.");
                    }

                    if (appointment.AppointmentStatus == AppointmentStatus.Completed ||
                        appointment.AppointmentStatus == AppointmentStatus.CanceledByPatient)
                    {
                        _loggerService.LogWarning("Attempted to reschedule a canceled or completed appointment {AppointmentId}.", appointmentId);
                        throw new BadRequest("Cannot reschedule a canceled or completed appointment.");
                    }

                    if (newTimeSlot.EndTime < newTimeSlot.StartTime)
                        throw new BadRequest("Invalid time slot. End time must be after start time.");

                    // 📅 Convert TimeSlot into UTC-based DateTimeOffsets
                    var startTimeOffset = new DateTimeOffset(
                        selectedDate.Year, selectedDate.Month, selectedDate.Day,
                        newTimeSlot.StartTime.Hour, newTimeSlot.StartTime.Minute, 0,
                        TimeZoneInfo.Local.GetUtcOffset(DateTime.Now));

                    var endTimeOffset = startTimeOffset.Add(newTimeSlot.EndTime - newTimeSlot.StartTime);

                    // ✅ **Check for availability**
                    if (!await appointmentRepo.IsTimeSlotAvailable(appointment.DoctorId, startTimeOffset, endTimeOffset))
                    {
                        _loggerService.LogWarning("Time slot unavailable for rescheduling appointment {AppointmentId}.", appointmentId);
                        throw new InvalidOperationException("The selected time slot is already booked.");
                    }

                    // ✅ **Update appointment**

                    appointment.StartDate = startTimeOffset;
                    appointment.EndDate = endTimeOffset;
                    appointment.AppointmentStatus = appointment.AppointmentStatus;

                    await appointmentRepo.UpdateEntityAsync(appointment);

                    await _unitOfWork.CommitAsync(); // ✅ Commit changes **before** notifications

                    _loggerService.LogInfo("Appointment {AppointmentId} successfully rescheduled to {NewDate}.",
                        appointmentId, startTimeOffset);

                    await transaction.CommitAsync();

                    // ✅ **Send notification outside the transaction**
                    _ = Task.Run(async () =>
                    {
                        await NotifyDoctorAsync(appointment.DoctorId, appointmentId, startTimeOffset);
                    });

                    // ✅ **Clear Cache outside the transaction**
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
            var strategy = _unitOfWork.CreateExecutionStrategy(); // ✅ Use EF Core Execution Strategy

            return await strategy.ExecuteAsync(async () =>
            {
                await using var transaction = await _unitOfWork.BeginTransactionAsync().ConfigureAwait(false);

                try
                {
                   

                    // 1️⃣ Retrieve the doctor with their appointment types.
                    var doctor = await _unitOfWork.GetCustomRepository<IDoctorRepositoryAsync>()
                        .FirstOrDefaultAsync(x => x.Id == int.Parse(request.DoctorId));

                    if (doctor == null)
                    { _loggerService.LogWarning("Attempted to book an appointment with non-existent doctor {DoctorId}.", request.DoctorId);
                        throw new ItemNotFound("Doctor does not exist.");
                    }
                    // 2️⃣ Retrieve the specified appointment type.

                    var appointmentType = await _unitOfWork.Repository<AppointmentType>()
                                                           .FirstOrDefaultAsync(x => x.Id == int.Parse(request.AppointmentTypeId) &&
                                                                                     x.DoctorId == int.Parse(request.DoctorId));

                    if (appointmentType == null)
                    {
                        _loggerService.LogWarning("Attempted to book an appointment with an invalid appointment type {AppointmentTypeId}.", request.AppointmentTypeId);
                        throw new ItemNotFound("This appointment type does not exist for this doctor.");
                    }

                    // 3️⃣ Calculate the appointment's end time by adding the duration to the start date.
                    var endTime = request.StartDate.Add(appointmentType.Duration);

                    // 4️⃣ Check if the new time slot is available.
                    if (!await IsTimeSlotAvailable(doctor.Id, request.StartDate, endTime).ConfigureAwait(false))
                    {
                        _loggerService.LogWarning("Attempted to book an unavailable time slot {StartDate}.", request.StartDate);
                        throw new BadRequest("Cannot book appointment; the selected time slot is already booked. Please try another time.");
                    }
                    // 5️⃣ Create the appointment.
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

                    await _unitOfWork.GetCustomRepository<IAppointmentRepositoryAsync>()
                        .AddEntityAsync(appointment)
                        .ConfigureAwait(false);

                    if (await _unitOfWork.CommitAsync(cancellationToken).ConfigureAwait(false) <= 0)
                        throw new Exception("Failed to book appointment.");

                    // 6️⃣ Commit the transaction.
                    await transaction.CommitAsync(cancellationToken).ConfigureAwait(false);

                    // TODO: Send Notification to doctor to approve

                    var user = await _userManager.Users.Where(u => u.Id == doctor.AppUserId)
                                                       .Select(x => new { x.FirstName, x.LastName, x.FcmToken })
                                                       .FirstOrDefaultAsync();

                    if (!string.IsNullOrEmpty(user?.FcmToken))
                    {
                        try
                        {
                            await _firebaseService.SendNotificationAsync(
                                user.FcmToken,
                                "Appointment Booked",
                                $"Hi {user.FirstName} {user.LastName}, a patient has booked an appointment on {appointment.StartDate:dd/MM/yyyy HH:mm}."
                            );
                        }
                        catch (Exception ex)
                        {
                            _loggerService.LogError("Failed to send booking notification for appointment {AppointmentId}: {ErrorMessage}",
                                appointment.Id, ex.Message);
                        }
                    }
                    // TODO: Remove appointments cache

                    return true;
                }
                catch
                {
                    await transaction.RollbackAsync(cancellationToken).ConfigureAwait(false);
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
            var strategy = _unitOfWork.CreateExecutionStrategy(); // ✅ Use EF Core Execution Strategy

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

                    // TODO: Send Notification tell patient that doctor approved his book

                    var user = await _userManager.Users.Where(x => x.Id == appointment.PatientId)
                                                       .Select(x => new { x.FcmToken, x.FirstName, x.LastName })
                                                       .FirstOrDefaultAsync();

                    if (!string.IsNullOrEmpty(user?.FcmToken))
                    {
                        try
                        {
                            await _firebaseService.SendNotificationAsync(
                                user.FcmToken,
                                "Appointment Approved",
                                $"Hi {user.FirstName} {user.LastName}," +
                                $"Your appointment has been approved. Please complete payment within 24 hours.."
                            );
                        }
                        catch (Exception ex)
                        {
                            _loggerService.LogError("Failed to send booking notification for appointment {appointmentId}: {ErrorMessage}",
                                appointmentId, ex.Message);
                        }
                    }

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
            var strategy = _unitOfWork.CreateExecutionStrategy(); // ✅ Use EF Core Execution Strategy

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

                    // TODO: Send Notification tell patient that doctor approved his book

                    var user = await _userManager.Users.Where(x => x.Id == appointment.PatientId)
                                                       .Select(x => new { x.FcmToken, x.FirstName, x.LastName })
                                                       .FirstOrDefaultAsync();

                    if (!string.IsNullOrEmpty(user?.FcmToken))
                    {
                        try
                        {
                            await _firebaseService.SendNotificationAsync(
                                user.FcmToken,
                                "Appointment Approved",
                                $"Hi {user.FirstName} {user.LastName}," +
                                $"Your appointment request was rejected. Reason: " + (rejectionReason ?? "Not specified.")
                            );
                        }
                        catch (Exception ex)
                        {
                            _loggerService.LogError("Failed to send booking notification for appointment {appointmentId}: {ErrorMessage}",
                                appointmentId, ex.Message);
                        }
                    }

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
            var strategy = _unitOfWork.CreateExecutionStrategy(); // ✅ Use EF Core Execution Strategy

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

                    // TODO: Send Notification tell patient that doctor completed his appointment

                    var user = await _userManager.Users.Where(x => x.Id == appointment.PatientId)
                                                       .Select(x => new { x.FcmToken, x.FirstName, x.LastName })
                                                       .FirstOrDefaultAsync();

                    if (!string.IsNullOrEmpty(user?.FcmToken))
                    {
                        try
                        {
                            await _firebaseService.SendNotificationAsync(
                                user.FcmToken,
                                "Appointment Completed",
                                $"Hi {user.FirstName} {user.LastName}," +
                                $"Your appointment has been successfully completed."
                            );
                        }
                        catch (Exception ex)
                        {
                            _loggerService.LogError("Failed to send booking notification for appointment {appointmentId}: {ErrorMessage}",
                                appointmentId, ex.Message);
                        }
                    }

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

                // ✅ Update Appointment & Payment Status after Refund
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

            // ✅ Notify patient about the cancellation & refund
            //await _notificationService.SendNotificationAsync(appointment.PatientId,
            //    $"Your appointment {appointmentId} has been canceled. If applicable, your refund is being processed.");

            var user = await _userManager.Users.Where(x => x.Id == appointment.PatientId)
                                                      .Select(x => new { x.FcmToken, x.FirstName, x.LastName })
                                                      .FirstOrDefaultAsync();

            if (!string.IsNullOrEmpty(user?.FcmToken))
            {
                try
                {
                    await _firebaseService.SendNotificationAsync(
                        user.FcmToken,
                        "Appointment Canceled",
                        $"Hi {user.FirstName} {user.LastName}," +
                        $"Your appointment {appointmentId} has been canceled. If applicable, your refund is being processed."
                    );
                }
                catch (Exception ex)
                {
                    _loggerService.LogError("Failed to send booking notification for appointment {appointmentId}: {ErrorMessage}",
                        appointmentId, ex.Message);
                }
            }

            _loggerService.LogInfo("Appointment {AppointmentId} successfully canceled by doctor.", appointmentId);
            return true;
        }



        public async Task AutoCancelUnpaidAppointments() // Scheduled with Hangfire
        {
            try
            {
                _loggerService.LogInfo("Started Auto Cancel Unpaid Appointments job.");

                var appointmentRepo = _unitOfWork.GetCustomRepository<IAppointmentRepositoryAsync>();

                // Define the criteria for unpaid and expired appointments
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

                // ✅ Use Batch Update for better efficiency
                await appointmentRepo.ExecuteUpdateAsync(
                    x => expiredAppointments.Select(a => a.Id).Contains(x.Id),
                    x => new Appointment
                    {
                        AppointmentStatus = AppointmentStatus.AutoCanceled,
                        CancelledAt = DateTimeOffset.UtcNow,
                        CancellationReason = "Payment not completed within the required timeframe."
                    });

                _loggerService.LogInfo($"Successfully auto-canceled {expiredAppointments.Count} unpaid appointments.");

                // 🔹 Fetch patients in a single query
                var patientIds = expiredAppointments.Select(x => x.PatientId).Distinct().ToList();
                var users = await _userManager.Users
                    .Where(u => patientIds.Contains(u.Id))
                    .Select(u => new { u.Id, u.FcmToken, u.FirstName, u.LastName })
                    .ToDictionaryAsync(u => u.Id);

                // 🔹 Send notifications in parallel
                await Parallel.ForEachAsync(expiredAppointments, async (appointment, _) =>
                {
                    if (users.TryGetValue(appointment.PatientId, out var user) && !string.IsNullOrEmpty(user.FcmToken))
                    {
                        try
                        {
                            await _firebaseService.SendNotificationAsync(
                                user.FcmToken,
                                "Appointment Auto-Canceled",
                                $"Hi {user.FirstName} {user.LastName}, Your appointment on {appointment.StartDate:MMM dd, yyyy} at {appointment.StartDate:hh:mm tt} was auto-canceled due to non-payment."
                            );
                        }
                        catch (Exception notificationEx)
                        {
                            _loggerService.LogError($"Failed to send notification for appointment {appointment.Id}: {notificationEx.Message}");
                        }
                    }
                });

            }
            catch (Exception ex)
            {
                _loggerService.LogError($"AutoCancelUnpaidAppointments job failed: {ex.Message}", ex);
                throw;
            }
        }

        private async Task NotifyDoctorAsync(int doctorId, int appointmentId, DateTimeOffset newDate)
        {
            var doctor = await _unitOfWork.Repository<Doctor>().FirstOrDefaultAsync(d => d.Id == doctorId);

            if (doctor == null) return;

            var user = await _userManager.Users
                .Where(u => u.Id == doctor.AppUserId)
                .Select(u => new { u.FirstName, u.LastName, u.FcmToken })
                .FirstOrDefaultAsync();

            if (!string.IsNullOrEmpty(user?.FcmToken))
            {
                try
                {
                    await _firebaseService.SendNotificationAsync(
                        user.FcmToken,
                        "Appointment Rescheduled",
                        $"Hi {user.FirstName} {user.LastName}, " +
                        $"A patient has rescheduled an appointment to {newDate:dd/MM/yyyy HH:mm}.");
                }
                catch (Exception ex)
                {
                    _loggerService.LogError("Failed to send reschedule notification for appointment {AppointmentId}: {ErrorMessage}",
                        appointmentId, ex.Message);
                }
            }
        }

        private bool IsCancellable(Appointment appointment)
        {
            return appointment.AppointmentStatus switch
            {
                AppointmentStatus.Completed => false,
                AppointmentStatus.CanceledByDoctor or AppointmentStatus.CanceledByPatient => false,
                _ => true
            };
        }

        private async Task NotifyDoctorAsync(int doctorId, int appointmentId)
        {
            var doctorInfo = await _unitOfWork.Repository<Doctor>().GetByIdAsync(doctorId);

            if (doctorInfo == null) return;

            var user = await _userManager.Users
                .Where(u => u.Id == doctorInfo.AppUserId)
                .Select(u => new { u.FcmToken, u.FirstName, u.LastName })
                .FirstOrDefaultAsync();

            if (user?.FcmToken != null)
            {
                await _firebaseService.SendNotificationAsync(user.FcmToken, "Cancellation",
                    $"The patient {user.FirstName} {user.LastName} has canceled appointment {appointmentId}. If payment was made, a refund has been processed.");
            }
        }


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
    }
}

