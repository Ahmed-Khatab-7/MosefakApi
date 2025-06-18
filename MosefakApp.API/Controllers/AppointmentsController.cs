using MosefakApp.Core.Dtos.Payment;

namespace MosefakApp.API.Controllers
{
    [Route("api/appointments")]
    [ApiController]
    //[Cached(duration: 600)] // 10 minutes
    [EnableRateLimiting(policyName: RateLimiterType.Concurrency)]
    public class AppointmentsController : ControllerBase
    {
        private readonly IAppointmentService _appointmentsService;
        private readonly IIdProtectorService _idProtectorService;
        public AppointmentsController(IAppointmentService appointmentsService, IIdProtectorService idProtectorService)
        {
            _appointmentsService = appointmentsService;
            _idProtectorService = idProtectorService;
        }

        [HttpGet("patient")]
        [RequiredPermission(Permissions.Appointments.ViewPatientAppointments)]
        public async Task<ActionResult<PaginatedResponse<AppointmentResponse>>> GetPatientAppointments(
        [FromQuery] AppointmentStatus? status = null,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10,
        CancellationToken cancellationToken = default)
        {
            int userId = User.GetUserId();

            // Get paginated appointments
            var (appointments, totalPages) = await _appointmentsService.GetPatientAppointments(
                userId, status, pageNumber, pageSize, cancellationToken);

            if (!appointments.Any())
            {
                return Ok(new PaginatedResponse<AppointmentResponse>
                {
                    Data = new List<AppointmentResponse>(),
                    TotalPages = totalPages,
                    CurrentPage = pageNumber,
                    PageSize = pageSize
                });
            }

            // Protect sensitive IDs
            appointments.ForEach(x =>
            {
                x.Id = ProtectId(x.Id);
                x.DoctorId = ProtectId(x.DoctorId);
                x.DoctorSpecialization?.ForEach(s => s.Id = ProtectId(s.Id)); // Null check
                if (x.AppointmentType != null)
                    x.AppointmentType.Id = ProtectId(x.AppointmentType.Id);
            });

            // Return a paginated response
            return Ok(new PaginatedResponse<AppointmentResponse>
            {
                Data = appointments,
                TotalPages = totalPages,
                CurrentPage = pageNumber,
                PageSize = pageSize
            });
        }


        [HttpGet("{appointmentId}")]
        [RequiredPermission(Permissions.Appointments.View)]
        public async Task<ActionResult<AppointmentResponse>> GetAppointmentById(
        string appointmentId,
        CancellationToken cancellationToken = default)
        {
            var unprotectedId = UnprotectId(appointmentId);

            if (unprotectedId == null)
                return BadRequest("Invalid Id");

            var query = await _appointmentsService.GetAppointmentById(unprotectedId.Value, cancellationToken);

            if (query == null)
                return NotFound("Appointment not found");

            // Protect sensitive IDs
            query.Id = appointmentId;
            query.DoctorId = ProtectId(query.DoctorId);

            if (query.AppointmentType != null)
                query.AppointmentType.Id = ProtectId(query.AppointmentType.Id);

            if (query.DoctorSpecialization != null)
                query.DoctorSpecialization.ForEach(s => s.Id = ProtectId(s.Id));

            return Ok(query);
        }


        // Get Appointments by Date Range (Patient)
        [HttpGet("range")]
        [RequiredPermission(Permissions.Appointments.ViewInRange)]
        public async Task<ActionResult<PaginatedResponse<AppointmentResponse>>> GetAppointmentsByDateRange(
        [FromQuery] DateTimeOffset startDate,
        [FromQuery] DateTimeOffset endDate,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10,
        CancellationToken cancellationToken = default)
        {
            int userId = User.GetUserId();

            // Get paginated appointments
            var (appointments, totalPages) = await _appointmentsService.GetAppointmentsByDateRange(
                userId, startDate, endDate, pageNumber, pageSize, cancellationToken);

            if (!appointments.Any())
            {
                return Ok(new PaginatedResponse<AppointmentResponse>
                {
                    Data = new List<AppointmentResponse>(),
                    TotalPages = totalPages,
                    CurrentPage = pageNumber,
                    PageSize = pageSize
                });
            }

            // Protect sensitive IDs
            appointments.ForEach(x =>
            {
                x.Id = ProtectId(x.Id);
                x.DoctorId = ProtectId(x.DoctorId);
                x.DoctorSpecialization?.ForEach(s => s.Id = ProtectId(s.Id)); // Null check
                if (x.AppointmentType != null)
                    x.AppointmentType.Id = ProtectId(x.AppointmentType.Id);
            });

            // Return paginated response
            return Ok(new PaginatedResponse<AppointmentResponse>
            {
                Data = appointments,
                TotalPages = totalPages,
                CurrentPage = pageNumber,
                PageSize = pageSize
            });
        }


        [HttpPut("{appointmentId}/approve")]
        [RequiredPermission(Permissions.Appointments.Approve)]
        public async Task<ActionResult<bool>> ApproveAppointmentByDoctor(string appointmentId)
        {
            var unprotectedId = UnprotectId(appointmentId);
            if (unprotectedId == null)
                return BadRequest("Invalid ID");
            var query = await _appointmentsService.ApproveAppointmentByDoctor(unprotectedId.Value);

            return Ok(query);
        }

        [HttpPut("{appointmentId}/reject")]
        [RequiredPermission(Permissions.Appointments.Reject)]
        public async Task<ActionResult<bool>> RejectAppointmentByDoctor(string appointmentId, RejectAppointmentRequest request)
        {
            var unprotectedId = UnprotectId(appointmentId);
            if (unprotectedId == null)
                return BadRequest("Invalid ID");

            var query = await _appointmentsService.RejectAppointmentByDoctor(unprotectedId.Value, request.RejectionReason);

            return Ok(query);
        }

        [HttpPut("{appointmentId}/complete")]
        [RequiredPermission(Permissions.Appointments.MarkAsCompleted)]
        public async Task<ActionResult<bool>> MarkAppointmentAsCompleted(string appointmentId)
        {
            var unprotectedId = UnprotectId(appointmentId);
            if (unprotectedId == null)
                return BadRequest("Invalid ID");

            var query = await _appointmentsService.MarkAppointmentAsCompleted(unprotectedId.Value);

            return Ok(query);
        }

        [HttpDelete("{appointmentId}/doctor-cancel")]
        [RequiredPermission(Permissions.Appointments.CancelByDoctor)]
        public async Task<ActionResult<bool>> CancelAppointmentByDoctor(string appointmentId, CancelAppointmentRequest request)
        {
            var unprotectedId = UnprotectId(appointmentId);
            if (unprotectedId == null)
                return BadRequest("Invalid ID");

            var query = await _appointmentsService.CancelAppointmentByDoctor(unprotectedId.Value, request.CancelationReason);

            return Ok(query);
        }

        [HttpPut("{appointmentId}/patient-cancel")]
        [RequiredPermission(Permissions.Appointments.CancelByPatient)]
        public async Task<ActionResult<bool>> CancelAppointmentAsync(string appointmentId, CancelAppointmentRequest request, CancellationToken token = default)
        {
            var unprotectedId = UnprotectId(appointmentId);
            if (unprotectedId == null)
                return BadRequest("Invalid ID");

            int userId = User.GetUserId();

            var query = await _appointmentsService.CancelAppointmentByPatient(userId, unprotectedId.Value, request.CancelationReason, token);

            return Ok(query);
        }


        [HttpPost("create-payment-intent/{appointmentId}")]
        [RequiredPermission(Permissions.Appointments.CreatePaymentIntent)]
        public async Task<ActionResult<string>> CreatePaymentIntent(string appointmentId, CancellationToken cancellationToken = default)
        {
            var unprotectedId = UnprotectId(appointmentId);
            if (unprotectedId == null)
                return BadRequest("Invalid ID");

            var clientSecret = await _appointmentsService.CreatePaymentIntent(unprotectedId.Value, cancellationToken);
 
            return Ok(clientSecret);
        }

        // in case Manually Confirming from Frontend, If webhooks are not used, the frontend calls..

        [HttpPost("confirm-appointment-payment/{appointmentId}")]
        [RequiredPermission(Permissions.Appointments.ConfirmPayment)]
        public async Task<IActionResult> ConfirmAppointmentPayment(string appointmentId, CancellationToken cancellationToken = default)
        {
            var unprotectedId = UnprotectId(appointmentId);
            if (unprotectedId == null)
                return BadRequest("Invalid ID");

            var query = await _appointmentsService.ConfirmAppointmentPayment(unprotectedId.Value, cancellationToken);

            return Ok(query);
        }

        [HttpPut("reschedule")]
        [RequiredPermission(Permissions.Appointments.Reschedule)]
        public async Task<ActionResult<bool>> RescheduleAppointmentAsync(RescheduleAppointmentRequest request)
        {
            var unprotectedId = UnprotectId(request.AppointmentId);
            if (unprotectedId == null)
                return BadRequest("Invalid ID");

            var query = await _appointmentsService.RescheduleAppointmentAsync(unprotectedId.Value, request.selectedDate,request.newTimeSlot);

            return Ok(query);
        }

        [HttpPost("book")]
        [RequiredPermission(Permissions.Appointments.Book)]
        public async Task<ActionResult<bool>> BookAppointment(BookAppointmentRequest request, CancellationToken cancellationToken = default)
        {
            var unprotectedDoctorId = UnprotectId(request.DoctorId);
            if (unprotectedDoctorId == null)
                return BadRequest("Invalid ID");

            var unprotectedAppointmentTypeId = UnprotectId(request.AppointmentTypeId);
            if (unprotectedAppointmentTypeId == null)
                return BadRequest("Invalid ID");

            int appUserId = User.GetUserId();

            request.DoctorId = unprotectedDoctorId.Value.ToString();
            request.AppointmentTypeId = unprotectedAppointmentTypeId.Value.ToString();
            var query = await _appointmentsService.BookAppointment(request, appUserId, cancellationToken);

            return Ok(query); 
        }

        [HttpGet("{appointmentId}/status")]
        [RequiredPermission(Permissions.Appointments.ViewStatus)]
        public async Task<ActionResult<AppointmentStatus>> GetAppointmentStatus(string appointmentId)
        {
            var unprotectedId = UnprotectId(appointmentId);
            if (unprotectedId == null)
                return BadRequest("Invalid ID");

            var query = await _appointmentsService.GetAppointmentStatus(unprotectedId.Value);

            return Ok(query);
        }

        [HttpGet("doctor")]
        [RequiredPermission(Permissions.Appointments.ViewDoctorAppointments)]
        public async Task<ActionResult<PaginatedResponse<AppointmentResponse>>> GetDoctorAppointments(
        [FromQuery] AppointmentStatus? status = null,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10,
        CancellationToken cancellationToken = default)
        {
            int doctorId = User.GetUserId();

            // Fetch paginated doctor appointments
            var (appointments, totalPages) = await _appointmentsService.GetDoctorAppointments(
                doctorId, status, pageNumber, pageSize, cancellationToken);

            if (!appointments.Any())
            {
                return Ok(new PaginatedResponse<AppointmentResponse>
                {
                    Data = new List<AppointmentResponse>(),
                    TotalPages = totalPages,
                    CurrentPage = pageNumber,
                    PageSize = pageSize
                });
            }

            // Protect sensitive IDs
            appointments.ForEach(x =>
            {
                x.Id = ProtectId(x.Id);
                x.DoctorId = ProtectId(x.DoctorId);
                x.DoctorSpecialization?.ForEach(s => s.Id = ProtectId(s.Id)); // Null check
                if (x.AppointmentType != null)
                    x.AppointmentType.Id = ProtectId(x.AppointmentType.Id);
            });

            // Return structured paginated response
            return Ok(new PaginatedResponse<AppointmentResponse>
            {
                Data = appointments,
                TotalPages = totalPages,
                CurrentPage = pageNumber,
                PageSize = pageSize
            });
        }

        [HttpGet("doctor/patient-data")]
        [RequiredPermission(Permissions.Appointments.ViewDoctorAppointments)]
        public async Task<ActionResult<PaginatedResponse<AppointmentPatientResponse>>> GetDoctorAppointmentsWithPatientData(
        [FromQuery] string doctorId,
        [FromQuery] AppointmentStatus? status = null,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10,
        CancellationToken cancellationToken = default)
        {
            
            var unprotectedDoctorId = UnprotectId(doctorId);

            if(unprotectedDoctorId is null)
                return BadRequest("Invalid ID");

            // Fetch paginated doctor appointments
            var (appointments, totalPages) = await _appointmentsService.GetDoctorAppointmentsWithPatientData(
                unprotectedDoctorId.Value, status, pageNumber, pageSize, cancellationToken);

            if (!appointments.Any())
            {
                return Ok(new PaginatedResponse<AppointmentPatientResponse>
                {
                    Data = new List<AppointmentPatientResponse>(),
                    TotalPages = totalPages,
                    CurrentPage = pageNumber,
                    PageSize = pageSize
                });
            }

            // Protect sensitive IDs
            appointments.ForEach(x =>
            {
                x.Id = ProtectId(x.Id);
                x.PatientId = ProtectId(x.PatientId);
                if (x.AppointmentType != null)
                    x.AppointmentType.Id = ProtectId(x.AppointmentType.Id);
            });

            // Return structured paginated response
            return Ok(new PaginatedResponse<AppointmentPatientResponse>
            {
                Data = appointments,
                TotalPages = totalPages,
                CurrentPage = pageNumber,
                PageSize = pageSize
            });
        }

        [HttpGet("pending")]
        [RequiredPermission(Permissions.Appointments.ViewPendingForDoctor)]
        public async Task<ActionResult<PaginatedResponse<AppointmentResponse>>> GetPendingAppointmentsForDoctor(
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10,
        CancellationToken cancellationToken = default)
        {
            int doctorId = User.GetUserId();

            // Fetch paginated pending appointments
            var (appointments, totalPages) = await _appointmentsService.GetPendingAppointmentsForDoctor(
                doctorId, pageNumber, pageSize, cancellationToken);

            if (!appointments.Any())
            {
                return Ok(new PaginatedResponse<AppointmentResponse>
                {
                    Data = new List<AppointmentResponse>(),
                    TotalPages = totalPages,
                    CurrentPage = pageNumber,
                    PageSize = pageSize
                });
            }

            // Protect sensitive IDs
            appointments.ForEach(x =>
            {
                x.Id = ProtectId(x.Id);
                x.DoctorId = ProtectId(x.DoctorId);
                x.DoctorSpecialization?.ForEach(s => s.Id = ProtectId(s.Id)); // Null check
                if (x.AppointmentType != null)
                    x.AppointmentType.Id = ProtectId(x.AppointmentType.Id);
            });

            return Ok(new PaginatedResponse<AppointmentResponse>
            {
                Data = appointments,
                TotalPages = totalPages,
                CurrentPage = pageNumber,
                PageSize = pageSize
            });
        }

        [HttpGet("doctor/range")]
        [RequiredPermission(Permissions.Appointments.ViewInRangeForDoctor)]
        public async Task<ActionResult<PaginatedResponse<AppointmentResponse>>> GetAppointmentsByDateRangeForDoctor(
        [FromQuery] DateTimeOffset startDate,
        [FromQuery] DateTimeOffset endDate,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10,
        CancellationToken cancellationToken = default)
        {
            int doctorId = User.GetUserId();

            // Fetch paginated appointments in date range
            var (appointments, totalPages) = await _appointmentsService.GetAppointmentsByDateRangeForDoctor(
                doctorId, startDate, endDate, pageNumber, pageSize, cancellationToken);

            if (!appointments.Any())
            {
                return Ok(new PaginatedResponse<AppointmentResponse>
                {
                    Data = new List<AppointmentResponse>(),
                    TotalPages = totalPages,
                    CurrentPage = pageNumber,
                    PageSize = pageSize
                });
            }

            // Protect sensitive IDs
            appointments.ForEach(x =>
            {
                x.Id = ProtectId(x.Id);
                x.DoctorId = ProtectId(x.DoctorId);
                x.DoctorSpecialization?.ForEach(s => s.Id = ProtectId(s.Id)); // Null check
                if (x.AppointmentType != null)
                    x.AppointmentType.Id = ProtectId(x.AppointmentType.Id);
            });

            return Ok(new PaginatedResponse<AppointmentResponse>
            {
                Data = appointments,
                TotalPages = totalPages,
                CurrentPage = pageNumber,
                PageSize = pageSize
            });
        }

        [HttpGet("Payments")]
        [RequiredPermission(Permissions.Appointments.Payments)]
        [AllowAnonymousPermission]
        public async Task<ActionResult<PaginatedResponse<PaymentResponse>>> GetPayments(
            [FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10)
        {
            var payments = await _appointmentsService.GetPayments(pageNumber, pageSize);

            payments.Data.Select(x => x.Id = ProtectId(x.Id.ToString())).ToList();
            payments.Data.Select(x => x.AppointmentId = ProtectId(x.AppointmentId.ToString())).ToList();
            
            return Ok(payments);
        }

        [HttpDelete("Payments/{paymentId}")]
        [RequiredPermission(Permissions.Appointments.DeletePayment)]
        public async Task<ActionResult<bool>> DeletePayment(string paymentId)
        {
            var unprotectedId = UnprotectId(paymentId);
            if (unprotectedId == null)
                return BadRequest("Invalid ID");

            var query = await _appointmentsService.DeletePayment(unprotectedId.Value);

            return Ok(query);
        }

        // 🔥 Reusable Helper Method for ID Protection
        private int? UnprotectId(string protectedId) => _idProtectorService.UnProtect(protectedId);

        private string ProtectId(string id) => _idProtectorService.Protect(int.Parse(id));
    }
}
