namespace MosefakApp.API.Controllers
{
    [Route("api/doctors")]
    [ApiController]
  //  [Cached(duration: 600)] // 10 minutes
    [EnableRateLimiting(policyName: RateLimiterType.Concurrency)]
    public class DoctorsController : ApiBaseController
    {
        private readonly IDoctorService _doctorService;
        private readonly IIdProtectorService _idProtectorService;

        public DoctorsController(IDoctorService doctorService, IIdProtectorService idProtectorService)
        {
            _doctorService = doctorService;
            _idProtectorService = idProtectorService;
        }

        [HttpGet]
        [RequiredPermission(Permissions.Doctors.View)]
        public async Task<ActionResult<PaginatedResponse<DoctorResponse>>> GetAllDoctors(
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10)
        {
            var response = await _doctorService.GetAllDoctors(pageNumber, pageSize);

            // Protect sensitive IDs
            response.Data.ToList().ForEach(d =>
            {
                d.Id = ProtectId(d.Id);
                d.Specializations?.ForEach(s => s.Id = ProtectId(s.Id)); // Null-safe check
            });

            return Ok(response);
        }


        // ✅ Get doctor by ID
        [HttpGet("{doctorId}")]
        [RequiredPermission(Permissions.Doctors.ViewById)]
        public async Task<ActionResult<DoctorDetail>> GetDoctorById(string doctorId)
        {
            var unprotectedId = UnprotectId(doctorId);
            if (unprotectedId == null) return BadRequest("Invalid doctor ID");

            var doctor = await _doctorService.GetDoctorById(unprotectedId.Value);

            ProtectDoctorDetail(doctor);

            return Ok(doctor);
        }

      

        [HttpPost("search")]
        [RequiredPermission(Permissions.Doctors.Search)]
        public async Task<ActionResult<PaginatedResponse<DoctorResponse>>> SearchDoctors(
        [FromBody] DoctorUnifiedSearchFilter filter,
        [FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10)
        {
            var result = await _doctorService.SearchDoctorsUnifiedAsync(filter, pageNumber, pageSize);

            if (result?.Data != null)
            {
                foreach (var doc in result.Data)
                {
                    doc.Id = ProtectId(doc.Id);
                    if (doc.Specializations != null)
                    {
                        foreach (var s in doc.Specializations)
                        {
                            s.Id = ProtectId(s.Id);
                        }
                    }
                }
            }

            return Ok(result);
        }


        // ✅ Get doctor profile (Authenticated Doctor)
        [HttpGet("profile")]
        [RequiredPermission(Permissions.Doctors.ViewProfile)] // Permissions.Doctors.ViewDoctorProfile
        public async Task<ActionResult<DoctorProfileResponse>> GetDoctorProfile()
        {
            var userId = User.GetUserId();
            var profile = await _doctorService.GetDoctorProfile(userId);
            
            profile.Id = ProtectId(userId.ToString());
            profile.Specializations.ForEach(s => s.Id = ProtectId(s.Id));
            profile.Experiences.ForEach(s => s.Id = ProtectId(s.Id));
            profile.Awards.ForEach(s => s.Id = ProtectId(s.Id));
            profile.Education.ForEach(s => s.Id = ProtectId(s.Id));

            return Ok(profile);
        }

        // ✅ Get top 10 doctors
        [HttpGet("top-ten")]
        [RequiredPermission(Permissions.Doctors.ViewTopTen)]
        public async Task<ActionResult<List<DoctorResponse>>> GetTopTenDoctors()
        {
            var doctors = await _doctorService.TopTenDoctors();

            if (doctors is null)
                return Ok();

            doctors.ForEach(d => d.Id = ProtectId(d.Id));
            doctors.ForEach(d => d.Specializations.ForEach(s => s.Id = ProtectId(s.Id)));

            return Ok(doctors);
        }


        // ✅ Get upcoming appointments
        [HttpGet("appointments/upcoming")]
        [RequiredPermission(Permissions.Doctors.ViewUpcomingAppointments)]
        public async Task<ActionResult<PaginatedResponse<AppointmentDto>>> GetUpcomingAppointmentsAsync(
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 10)
        {
            var userId = User.GetUserId();
            var query = await _doctorService.GetUpcomingAppointmentsAsync(userId, pageNumber, pageSize);

            if(query is not null)
            {
                query.Data.ToList().ForEach(a => a.Id = ProtectId(a.Id));
                query.Data.ToList().ForEach(a => a.PatientId = ProtectId(a.PatientId));
            }

            return Ok(query);
        }

        [HttpGet("appointments/today")]
        [RequiredPermission(Permissions.Doctors.GetToDayAppointments)]
        public async Task<PaginatedResponse<AppointmentPatinetDetail>> GetToDayAppointmentsAsync(int pageNumber = 1, int pageSize = 10)
        {
            var userId = User.GetUserId();

            var query = await _doctorService.GetToDayAppointmentsAsync(userId, pageNumber, pageSize);

            if (query is not null)
            {
                query.Data.ToList().ForEach(a => a.Id = ProtectId(a.Id));
                query.Data.ToList().ForEach(a => a.PatientId = ProtectId(a.PatientId));
            }

            return query!;
        }

        // ✅ Get past appointments
        [HttpGet("appointments/past")]
        [RequiredPermission(Permissions.Doctors.ViewPastAppointments)]
        public async Task<ActionResult<PaginatedResponse<AppointmentDto>>> GetPastAppointmentsAsync(
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10)
        {
            var userId = User.GetUserId();
            var query = await _doctorService.GetPastAppointmentsAsync(userId, pageNumber, pageSize);

            if (query is not null)
            {
                query.Data.ToList().ForEach(a => a.Id = ProtectId(a.Id));
                query.Data.ToList().ForEach(a => a.PatientId = ProtectId(a.PatientId));
            }

            return Ok(query);
        }


        // ✅ Get total appointments
        [HttpGet("appointments/total")]
        [RequiredPermission(Permissions.Doctors.GetTotalAppointments)]
        public async Task<ActionResult<long>> GetTotalAppointmentsAsync()
        {
            var doctorId = User.GetUserId();
            var query = await _doctorService.GetTotalAppointmentsAsync(doctorId);
            return Ok(query);
        }

        // ✅ Upload profile image
        [HttpPost("profile/image")]
        [RequiredPermission(Permissions.Doctors.UploadProfileImage)]
        public async Task<ActionResult<bool>> UploadProfileImageAsync(IFormFile imageFile, CancellationToken cancellationToken = default)
        {
            var userId = User.GetUserId();
            var query = await _doctorService.UploadProfileImageAsync(userId, imageFile, cancellationToken);
            return Ok(query);
        }

        [HttpGet("specializations")]
        [RequiredPermission(Permissions.Specializations.View)]
        public async Task<ActionResult<PaginatedResponse<SpecializationResponse>>> GetSpecializations(
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 10)
        {
            var userId = User.GetUserId();
            var query = await _doctorService.GetSpecializations(userId, pageNumber, pageSize);
            if(query != null)
                query.Data.ToList().ForEach(s => s.Id = ProtectId(s.Id));
            return Ok(query);
        }

        [HttpGet("specializations/admin")]
        [RequiredPermission(Permissions.Specializations.ViewForAdmin)]
        public async Task<ActionResult<PaginatedResponse<SpecializationResponse>>> GetSpecializationsForAdmin(
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 10)
        {
            var query = await _doctorService.GetSpecializations(pageNumber, pageSize);
            if (query != null)
                query.Data.ToList().ForEach(s => s.Id = ProtectId(s.Id));
            return Ok(query);
        }

        // ✅ Specialization Management
        [HttpPost("specializations")]
        [RequiredPermission(Permissions.Specializations.Create)]
        public async Task<ActionResult<bool>> AddSpecializationAsync([FromBody] SpecializationRequest request)
        {
            var doctorId = User.GetUserId();
            var query = await _doctorService.AddSpecializationAsync(doctorId, request);
            return Ok(query);
        }

        [HttpPut("specializations/{specializationId}")]
        [RequiredPermission(Permissions.Specializations.Edit)]
        public async Task<ActionResult<bool>> EditSpecializationAsync(string specializationId, [FromBody] SpecializationRequest request)
        {
            var unprotectedSpecializationId = UnprotectId(specializationId);
            if (unprotectedSpecializationId == null) return BadRequest("Invalid specialization ID");

            var doctorId = User.GetUserId();
            var query = await _doctorService.EditSpecializationAsync(doctorId, unprotectedSpecializationId.Value, request);
            return Ok(query);
        }

        [HttpDelete("specializations/{specializationId}")]
        [RequiredPermission(Permissions.Specializations.Remove)]
        public async Task<ActionResult<bool>> RemoveSpecializationAsync(string specializationId)
        {
            var unprotectedSpecializationId = UnprotectId(specializationId);
            if (unprotectedSpecializationId == null) return BadRequest("Invalid specialization ID");

            var doctorId = User.GetUserId();
            var query = await _doctorService.RemoveSpecializationAsync(doctorId, unprotectedSpecializationId.Value);
            return Ok(query);
        }

        // ✅ Admin: Add a new doctor
        [HttpPost]
        [RequiredPermission(Permissions.Doctors.Create)]
        public async Task<IActionResult> AddDoctor([FromBody] DoctorRequest request)
        {
            await _doctorService.AddDoctor(request);
            return CreatedAtAction(nameof(GetDoctorById), new { doctorId = request.AppUserId }, null);
        }

        // ✅ Complete doctor profile
        [HttpPost("profile/complete")]
        [AllowAnonymousPermission()]
        public async Task<IActionResult> CompleteDoctorProfile([FromQuery] string userId, [FromForm] CompleteDoctorProfileRequest request)
        {
            var unprotectedUserId = UnprotectId(userId);

            if (unprotectedUserId == null)
                return BadRequest("Invalid clinic ID");

            await _doctorService.CompleteDoctorProfile(unprotectedUserId.Value, request);
            return Ok();
        }

        [HttpPut("update-working-times/{clinicId}")]
        [RequiredPermission(Permissions.Doctors.UpdateWorkingTimesAsync)]
        //[Cached(duration: 0)] 
        public async Task<ActionResult<bool>> UpdateWorkingTimesAsync(string clinicId, IEnumerable<WorkingTimeRequest> workingTimes)
        {
            var unprotectedId = UnprotectId(clinicId);
            if (unprotectedId == null) return BadRequest("Invalid clinic ID");

            var userId = User.GetUserId();
            var query = await _doctorService.UpdateWorkingTimesAsync(userId, unprotectedId.Value, workingTimes);

            return Ok(query);
        }

        // ✅ Update doctor profile
        [HttpPut("profile/update")]
        [RequiredPermission(Permissions.Doctors.EditProfile)]
        public async Task<IActionResult> UpdateDoctorProfile(DoctorProfileUpdateRequest request, CancellationToken cancellationToken = default)
        {
            var userId = User.GetUserId();
            await _doctorService.UpdateDoctorProfile(request, userId, cancellationToken);
            return Ok();
        }

        // ✅ Admin: Delete a doctor
        [HttpDelete("{doctorId}")]
        [RequiredPermission(Permissions.Doctors.Delete)]
        public async Task<IActionResult> DeleteDoctor(string doctorId)
        {
            var unprotectedId = UnprotectId(doctorId);
            if (unprotectedId == null) return BadRequest("Invalid doctor ID");

            await _doctorService.DeleteDoctor(unprotectedId.Value);
            return NoContent();
        }

        // ✅ Get doctor's available time slots
        [HttpGet("{doctorId}/clinics/{clinicId}/appointments/{appointmentTypeId}/available-slots")]
        [RequiredPermission(Permissions.Doctors.ViewAvailableTimeSlots)]
        public async Task<ActionResult<List<TimeSlot>>> GetAvailableTimeSlots(string doctorId, string clinicId, string appointmentTypeId, [FromQuery] DayOfWeek selectedDay)
        {
            var unprotectedId = UnprotectId(doctorId);
            var unprotectedClinicId = UnprotectId(clinicId);
            var unprotectedAppointmentTypeId = UnprotectId(appointmentTypeId);

            if (unprotectedId == null || unprotectedClinicId == null || unprotectedAppointmentTypeId == null)
                return BadRequest("Invalid ID provided");

            var availableSlots = await _doctorService.GetAvailableTimeSlots(unprotectedId.Value, unprotectedClinicId.Value, unprotectedAppointmentTypeId.Value, selectedDay);
            return Ok(availableSlots);
        }

        [HttpGet("awards")]
        [RequiredPermission(Permissions.Awards.View)]
        public async Task<ActionResult<PaginatedResponse<AwardResponse>>> GetAwards(
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10)
        {
            var userId = User.GetUserId();
            var query = await _doctorService.GetAwards(userId, pageNumber, pageSize);
            if (query != null)
                query.Data.ToList().ForEach(s => s.Id = ProtectId(s.Id));
            return Ok(query);
        }

        // ✅ Awards Management
        [HttpPost("awards")]
        [RequiredPermission(Permissions.Awards.Create)]
        public async Task<ActionResult<bool>> AddAwardAsync([FromBody] AwardRequest request, CancellationToken cancellationToken = default)
        {
            var doctorId = User.GetUserId();
            var query = await _doctorService.AddAwardAsync(doctorId, request, cancellationToken);
            return Ok(query);
        }

        [HttpPut("awards/{awardId}")]
        [RequiredPermission(Permissions.Awards.Edit)]
        public async Task<ActionResult<bool>> EditAwardAsync(string awardId, [FromBody] AwardRequest request)
        {
            var unprotectedAwardId = UnprotectId(awardId);
            if (unprotectedAwardId == null) return BadRequest("Invalid Award ID");

            var doctorId = User.GetUserId();
            var query = await _doctorService.EditAwardAsync(doctorId, unprotectedAwardId.Value, request);
            return Ok(query);
        }

        [HttpDelete("awards/{awardId}")]
        [RequiredPermission(Permissions.Awards.Remove)]
        public async Task<ActionResult<bool>> RemoveAwardAsync(string awardId)
        {
            var unprotectedAwardId = UnprotectId(awardId);
            if (unprotectedAwardId == null) return BadRequest("Invalid Award ID");

            var doctorId = User.GetUserId();
            var query = await _doctorService.RemoveAwardAsync(doctorId, unprotectedAwardId.Value);
            return Ok(query);
        }

        [HttpGet("educations")]
        [RequiredPermission(Permissions.Educations.View)]
        public async Task<ActionResult<PaginatedResponse<EducationResponse>>> GetEducations(
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10)
        {
            var userId = User.GetUserId();
            var query = await _doctorService.GetEducations(userId, pageNumber, pageSize);
            if (query != null)
                query.Data.ToList().ForEach(s => s.Id = ProtectId(s.Id));
            return Ok(query);
        }

        // ✅ Education Management
        [HttpPost("education")]
        [RequiredPermission(Permissions.Educations.Create)]
        public async Task<ActionResult<bool>> AddEducationAsync([FromForm] EducationRequest request, CancellationToken cancellationToken = default)
        {
            var doctorId = User.GetUserId();
            var query = await _doctorService.AddEducationAsync(doctorId, request, cancellationToken);
            return Ok(query);
        }

        [HttpPut("education/{educationId}")]
        [RequiredPermission(Permissions.Educations.Edit)]
        public async Task<ActionResult<bool>> EditEducationAsync(string educationId, [FromForm] EducationRequest request, CancellationToken cancellationToken = default)
        {
            var unprotectedEducationId = UnprotectId(educationId);
            if (unprotectedEducationId == null) return BadRequest("Invalid Education ID");

            var doctorId = User.GetUserId();
            var query = await _doctorService.EditEducationAsync(doctorId, unprotectedEducationId.Value, request, cancellationToken);
            return Ok(query);
        }

        [HttpDelete("education/{educationId}")]
        [RequiredPermission(Permissions.Educations.Remove)]
        public async Task<ActionResult<bool>> RemoveEducationAsync(string educationId)
        {
            var unprotectedEducationId = UnprotectId(educationId);
            if (unprotectedEducationId == null) return BadRequest("Invalid Education ID");

            var doctorId = User.GetUserId();
            var query = await _doctorService.RemoveEducationAsync(doctorId, unprotectedEducationId.Value);
            return Ok(query);
        }

        [HttpGet("experiences")]
        [RequiredPermission(Permissions.Experiences.View)]
        public async Task<ActionResult<PaginatedResponse<ExperienceResponse>>> GetExperiences(
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10)
        {
            var userId = User.GetUserId();
            var query = await _doctorService.GetExperiences(userId, pageNumber, pageSize);
            if (query != null)
                query.Data.ToList().ForEach(s => s.Id = ProtectId(s.Id));
            return Ok(query);
        }

        [HttpPost("experience")]
        [RequiredPermission(Permissions.Experiences.Create)]
        public async Task<ActionResult<bool>> AddExperienceAsync([FromForm] ExperienceRequest request, CancellationToken cancellationToken = default)
        {
            var doctorId = User.GetUserId();
            var query = await _doctorService.AddExperienceAsync(doctorId, request, cancellationToken);
            return Ok(query);
        }


        [HttpPut("experience/{experienceId}")]
        [RequiredPermission(Permissions.Experiences.Edit)]
        public async Task<ActionResult<bool>> EditExperienceAsync(string experienceId, [FromForm] ExperienceRequest request, CancellationToken cancellationToken = default)
        {
            var unprotectedExperienceId = UnprotectId(experienceId);
            if (unprotectedExperienceId == null) return BadRequest("Invalid Experience ID");

            var doctorId = User.GetUserId();
            var query = await _doctorService.EditExperienceAsync(doctorId, unprotectedExperienceId.Value, request, cancellationToken);
            return Ok(query);
        }

        [HttpDelete("experience/{experienceId}")]
        [RequiredPermission(Permissions.Experiences.Remove)]
        public async Task<ActionResult<bool>> RemoveExperienceAsync(string experienceId)
        {
            var unprotectedExperienceId = UnprotectId(experienceId);
            if (unprotectedExperienceId == null) return BadRequest("Invalid Experience ID");

            var doctorId = User.GetUserId();
            var query = await _doctorService.RemoveExperienceAsync(doctorId, unprotectedExperienceId.Value);
            return Ok(query);
        }

        // ✅ Add a new clinic
        [HttpPost("clinics")]
        [RequiredPermission(Permissions.Clinics.Create)]
        public async Task<ActionResult<bool>> AddClinicAsync([FromForm] ClinicRequest request, CancellationToken cancellationToken = default)
        {
            var doctorId = User.GetUserId();
            var query = await _doctorService.AddClinicAsync(doctorId, request, cancellationToken);
            return Ok(query);
        }

        // ✅ Edit a clinic
        [HttpPut("clinics/{clinicId}")]
        [RequiredPermission(Permissions.Clinics.Edit)]
        public async Task<ActionResult<bool>> EditClinicAsync(string clinicId, [FromForm] ClinicRequest request, CancellationToken cancellationToken = default)
        {
            var unprotectedClinicId = UnprotectId(clinicId);
            if (unprotectedClinicId == null) return BadRequest("Invalid Clinic ID");

            var doctorId = User.GetUserId();
            var query = await _doctorService.EditClinicAsync(doctorId, unprotectedClinicId.Value, request, cancellationToken);
            return Ok(query);
        }

        // ✅ Remove a clinic
        [HttpDelete("clinics/{clinicId}")]
        [RequiredPermission(Permissions.Clinics.Remove)]
        public async Task<ActionResult<bool>> RemoveClinicAsync(string clinicId)
        {
            var unprotectedClinicId = UnprotectId(clinicId);
            if (unprotectedClinicId == null) return BadRequest("Invalid Clinic ID");

            var doctorId = User.GetUserId();
            var query = await _doctorService.RemoveClinicAsync(doctorId, unprotectedClinicId.Value);
            return Ok(query);
        }

        // ✅ Get all clinics of a doctor for patient
        [HttpGet("{doctorId}/clinics")]
        [RequiredPermission(Permissions.Clinics.View)]
        //[Cached(duration: 0)]
        public async Task<ActionResult<PaginatedResponse<ClinicResponse>>> GetDoctorClinicsAsync(
            string doctorId,
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 10)
        {
            var unprotectedDoctorId = UnprotectId(doctorId);
            if (unprotectedDoctorId == null) return BadRequest("Invalid Doctor ID");

            var query = await _doctorService.GetDoctorClinicsAsync(unprotectedDoctorId.Value, pageNumber, pageSize);
            query.Data.ToList().ForEach(c => c.Id = ProtectId(c.Id));
            query.Data.ToList().ForEach(c => c.WorkingTimes.ForEach(x => x.Id = ProtectId(x.Id)));
            query.Data.ToList().ForEach(c => c.WorkingTimes.ForEach(x => x.Periods.ForEach(p => p.Id = ProtectId(p.Id))));

            return Ok(query);
        }

        // ✅ Get all clinics of a doctor for doctor
        [HttpGet("clinics")]
        [RequiredPermission(Permissions.Clinics.View)]
        //[Cached(duration: 0)]
        public async Task<ActionResult<PaginatedResponse<ClinicResponse>>> GetDoctorClinicsAsync(
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 10)
        {
            var doctorId = User.GetUserId();
            var query = await _doctorService.GetDoctorClinicsForDoctorAsync(doctorId, pageNumber, pageSize);

            query.Data.ToList().ForEach(c => c.Id = ProtectId(c.Id));
            query.Data.ToList().ForEach(c => c.WorkingTimes.ForEach(x => x.Id = ProtectId(x.Id)));
            query.Data.ToList().ForEach(c => c.WorkingTimes.ForEach(x => x.Periods.ForEach(p => p.Id = ProtectId(p.Id))));

            return Ok(query);
        }

        // ✅ Get average rating
        [HttpGet("reviews/{doctorId}/average-rating")]
        [RequiredPermission(Permissions.Doctors.ViewAverageRating)]
        public async Task<ActionResult<double>> GetAverageRatingAsync(string doctorId)
        {
            var unprotectedDoctorId = UnprotectId(doctorId);
            if (unprotectedDoctorId == null) return BadRequest("Invalid Doctor ID");
            var query = await _doctorService.GetAverageRatingAsync(unprotectedDoctorId.Value);
            return Ok(query);
        }

        // ✅ Get total patients served
        [HttpGet("analytics/total-patients")]
        [RequiredPermission(Permissions.Doctors.ViewTotalPatientsServed)]
        public async Task<ActionResult<long>> GetTotalPatientsServedAsync()
        {
            var doctorId = User.GetUserId();
            var query = await _doctorService.GetTotalPatientsServedAsync(doctorId);
            return Ok(query);
        }

        // ✅ Get doctor's earnings
        [HttpGet("earnings-report")]
        [RequiredPermission(Permissions.Doctors.ViewEarningsReport)]
        public async Task<ActionResult<DoctorEarningsResponse>> GetEarningsReportAsync([FromQuery] DateTimeOffset startDate, [FromQuery] DateTimeOffset endDate)
        {
            var doctorId = User.GetUserId();
            var query = await _doctorService.GetEarningsReportAsync(doctorId, startDate, endDate);
            return Ok(query);
        }

        // 🔥 Utility Methods for ID Protection
        private string ProtectId(string id) => _idProtectorService.Protect(int.Parse(id));
        private int? UnprotectId(string id) => _idProtectorService.UnProtect(id);

        private void ProtectDoctorDetail(DoctorDetail doctor)
        {
            doctor.Id = ProtectId(doctor.Id);
            doctor.AppointmentTypes.ForEach(a => a.Id = ProtectId(a.Id));
            doctor.Educations.ForEach(e => e.Id = ProtectId(e.Id));
            doctor.Specializations.ForEach(s => s.Id = ProtectId(s.Id));
            doctor.Awards.ForEach(a => a.Id = ProtectId(a.Id));
            doctor.Clinics.ForEach(c => c.Id = ProtectId(c.Id));
            doctor.Clinics.ForEach(c => c.WorkingTimes.ForEach(w => w.Id = ProtectId(w.Id)));
            doctor.Clinics.ForEach(c => c.WorkingTimes.ForEach(w => w.Periods.ForEach(p => p.Id = ProtectId(p.Id))));
            doctor.Experiences.ForEach(ex => ex.Id = ProtectId(ex.Id));
            doctor.Reviews.ForEach(x => x.Id = ProtectId(x.Id));
            //doctor.Reviews.ForEach(x => x.AppUserId = ProtectId(x.AppUserId));
        }
    }

}
