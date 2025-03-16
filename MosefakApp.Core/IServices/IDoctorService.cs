namespace MosefakApp.Core.IServices
{
    public interface IDoctorService
    {
        // 🔹 Doctor Profile Management
        Task<DoctorProfileResponse> GetDoctorProfile(int doctorId); // FromUserClaims
        Task UpdateDoctorProfile(DoctorProfileUpdateRequest request, int doctorId, CancellationToken cancellationToken = default); // FromUserClaims
        Task<bool> UploadProfileImageAsync(int doctorId, IFormFile imageFile, CancellationToken cancellationToken = default); // FromUserClaims
        Task CompleteDoctorProfile(int doctorId, CompleteDoctorProfileRequest doctor, CancellationToken cancellationToken = default); // // FromUserClaims // note, for Doctor: appUserId will take it FromUserClaims too and this represent doctor row in doctors table to update in his profile.


        // 🔹 Doctor Settings 
        Task<bool> UpdateWorkingTimesAsync(int doctorId, int clinicId, IEnumerable<WorkingTimeRequest> workingTimes); // FromUserClaims
        Task<List<TimeSlot>> GetAvailableTimeSlots(int doctorId, int clinicId, int appointmentTypeId, DayOfWeek selectedDay);  

        // 🔹 Search & Ranking & Recommendations
        Task<DoctorDetail> GetDoctorById(int doctorId); // Pass doctorId explicitly not claims
        Task<List<DoctorResponse>?> TopTenDoctors();
        Task<PaginatedResponse<DoctorResponse>> SearchDoctorsAsync(DoctorSearchFilter filter, int pageNumber = 1, int pageSize = 10);
        Task<PaginatedResponse<DoctorResponse>> SearchDoctorsBySpecialityAsync(DoctorSearchBySpecialityCategoryFilter filter, int pageNumber = 1, int pageSize = 10);

        // 🔹 Doctor's Appointments
        Task<PaginatedResponse<AppointmentDto>> GetUpcomingAppointmentsAsync(int doctorId, int pageNumber = 1, int pageSize = 10);
        Task<PaginatedResponse<AppointmentDto>> GetPastAppointmentsAsync(int doctorId, int pageNumber = 1, int pageSize = 10);
        Task<long> GetTotalAppointmentsAsync(int doctorId); // (For Analytics 🔥) // FromUserClaims

        // 🔹 Specializations & Experience
        Task<PaginatedResponse<SpecializationResponse>> GetSpecializations(int doctorId, int pageNumber = 1, int pageSize = 10);
        Task<bool> AddSpecializationAsync(int doctorId, SpecializationRequest request); // FromUserClaims
        Task<bool> RemoveSpecializationAsync(int doctorId, int specializationId); // FromUserClaims
        Task<bool> EditSpecializationAsync(int doctorId, int specializationId, SpecializationRequest request); // FromUserClaims
        Task<PaginatedResponse<ExperienceResponse>> GetExperiences(int doctorId, int pageNumber = 1, int pageSize = 10);
        Task<bool> AddExperienceAsync(int doctorId, ExperienceRequest request, CancellationToken cancellationToken = default); // FromUserClaims
        Task<bool> EditExperienceAsync(int doctorId, int experienceId, ExperienceRequest request, CancellationToken cancellationToken = default); // FromUserClaims
        Task<bool> RemoveExperienceAsync(int doctorId, int experienceId); // FromUserClaims


        // 🔹 Awards & Achievements
        Task<PaginatedResponse<AwardResponse>> GetAwards(int doctorId, int pageNumber = 1, int pageSize = 10);
        Task<bool> AddAwardAsync(int doctorId, AwardRequest request, CancellationToken cancellationToken = default); // FromUserClaims
        Task<bool> EditAwardAsync(int doctorId, int awardId, AwardRequest request); // FromUserClaims
        Task<bool> RemoveAwardAsync(int doctorId, int awardId); // FromUserClaims


        // 🔹 Education
        Task<PaginatedResponse<EducationResponse>> GetEducations(int doctorId, int pageNumber = 1, int pageSize = 10);
        Task<bool> AddEducationAsync(int doctorId, EducationRequest request, CancellationToken cancellationToken = default); // FromUserClaims
        Task<bool> EditEducationAsync(int doctorId, int educationId, EducationRequest request, CancellationToken cancellationToken = default); // FromUserClaims
        Task<bool> RemoveEducationAsync(int doctorId, int educationId); // FromUserClaims


        // 🔹 Clinic Management
        Task<PaginatedResponse<ClinicResponse>> GetDoctorClinicsAsync(int doctorId, int pageNumber = 1, int pageSize = 10); 
        Task<PaginatedResponse<ClinicResponse>> GetDoctorClinicsForDoctorAsync(int doctorId, int pageNumber = 1, int pageSize = 10); // FromUserClaims
        Task<bool> AddClinicAsync(int doctorId, ClinicRequest request, CancellationToken cancellationToken = default); // FromUserClaims
        Task<bool> EditClinicAsync(int doctorId, int clinicId, ClinicRequest request, CancellationToken cancellationToken = default); // FromUserClaims
        Task<bool> RemoveClinicAsync(int doctorId, int clinicId); // FromUserClaims


        // 🔹 Doctor Reviews
        Task<double> GetAverageRatingAsync(int doctorId); // Pass doctorId explicitly not claims

        // 🔹 Doctor Analytics & Statistics
        Task<long> GetTotalPatientsServedAsync(int doctorId); // FromUserClaims
        Task<DoctorEarningsResponse> GetEarningsReportAsync(int doctorId, DateTimeOffset startDate, DateTimeOffset endDate); // FromUserClaims


        // 🔹 Admin Controls (For Managing Doctors)
        Task<PaginatedResponse<DoctorResponse>> GetAllDoctors(int pageNumber = 1, int pageSize = 10);
        Task AddDoctor(DoctorRequest request); // for Admin, must pass appUserId explicitly
        Task DeleteDoctor(int doctorId); // for Admin, must pass appUserId explicitly

    }
}

/*
 
Note, there are differece between AddDoctor and CompleteProfile or updateProfile because
someone that responsible about addDoctor is admin while someone that responsible about CompleteProfile or Update it is doctor
so admin did't have AppUserId FromUserClaims because it's has different token and must pass it exceplicitly inverse doctor
that has AppUserId FromUserClaims and didn't need to pass it explicitly...

Admin Actions:

Pass the AppUserId explicitly in the request when managing doctors.
Example use cases:
1-Adding a doctor.
2-Editing doctor-specific fields.
3-Viewing a list of doctors.

User Actions:

Use the AppUserId from (User.Claims) to ensure the action is tied to the logged-in user.
Example use cases:
1-Completing a doctor profile action.
2-Viewing their own doctor profile.
3-updating their own doctor profile.
 
 */
