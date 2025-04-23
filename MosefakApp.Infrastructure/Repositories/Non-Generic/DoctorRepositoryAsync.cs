namespace MosefakApp.Infrastructure.Repositories.Non_Generic
{
    public class DoctorRepositoryAsync : GenericRepositoryAsync<Doctor>, IDoctorRepositoryAsync
    {
        private readonly AppDbContext _context;
        private readonly AppIdentityDbContext _identityDbContext;
        private readonly IConfiguration _config;
        private readonly string _baseUrl;
        public DoctorRepositoryAsync(AppDbContext context, AppIdentityDbContext identityDbContext, IConfiguration config) : base(context)
        {
            _context = context;
            _identityDbContext = identityDbContext;
            _config = config;
            _baseUrl = _config["BaseUrl"] ?? "https://default-url.com/";
        }

        public async Task<List<DoctorResponse>?> GetTopTenDoctors()
        {
            var query = await _context.Doctors
                .Include(d => d.Specializations) // Include related specializations
                .Include(d => d.Reviews) // Include related specializations
                .Include(d => d.Experiences) // Include related specializations
                .Select(d => new
                {
                    d.Id,
                    d.AppUserId, // Fetch AppUserId for linking to the Users table
                    d.TotalYearsOfExperience,
                    Specializations = d.Specializations.Select(s => new SpecializationResponse
                    {
                        Id = s.Id.ToString(),
                        Name = s.Name,
                        Category = s.Category,
                    })
                    .ToList(),
                    AverageRate = d.Reviews.Any() ? d.Reviews.Average(r => r.Rate) : 0, // Pre-calculate average rate
                    ReviewCount = d.Reviews.Count(),
                })
                .OrderByDescending(d => d.AverageRate)
                .ThenByDescending(d => d.ReviewCount)
                .Take(10)
                .AsSplitQuery()
                .ToListAsync();

           if(query is not null)
            {
                // Fetch user details using AppUserId
                var appUserIds = query.Select(d => d.AppUserId).ToList();

                var userDetails = await _identityDbContext.Users
                    .Where(u => appUserIds.Contains(u.Id)) // Match AppUserId
                    .Select(u => new
                    {
                        u.Id,
                        FullName = u.FirstName + " " + u.LastName,
                        u.ImagePath
                    })
                    .ToDictionaryAsync(u => u.Id, u => new { u.FullName, u.ImagePath });

                // Map the final result
                return query.Select(d => new DoctorResponse
                {
                    Id = d.Id.ToString(),
                    FullName = userDetails.ContainsKey(d.AppUserId) ? userDetails[d.AppUserId].FullName : "Unknown",
                    ImagePath = userDetails.ContainsKey(d.AppUserId) && !string.IsNullOrEmpty(userDetails[d.AppUserId].ImagePath)
                            ? $"{_baseUrl}{userDetails[d.AppUserId].ImagePath}"
                            : $"{_baseUrl}/images/default.jpg",
                    TotalYearsOfExperience = d.TotalYearsOfExperience,
                    Specializations = d.Specializations.ToList(),
                    NumberOfReviews = d.ReviewCount,
                    Rating = d.AverageRate,
                })
                .ToList();
            }

            return null!;
        }

        public async Task<Dictionary<int, (string FullName, string ImagePath)>> GetUserDetailsAsync(IEnumerable<int> appUserIds)
        {
            return await _identityDbContext.Users
                .Where(u => appUserIds.Contains(u.Id)) // Match AppUserId
                .Select(u => new
                {
                    u.Id,
                    FullName = u.FirstName + " " + u.LastName,
                    u.ImagePath
                })
                .ToDictionaryAsync(u => u.Id, u => (u.FullName!, u.ImagePath!));
        }

        public async Task<Dictionary<int, (string FullName, string ImagePath)>> GetUserDetailsAsync(int appUserId)
        {
            return await _identityDbContext.Users
                .Where(u => u.Id == appUserId) // Match AppUserId
                .Select(u => new
                {
                    u.Id,
                    FullName = u.FirstName + " " + u.LastName,
                    u.ImagePath
                })
                .ToDictionaryAsync(u => u.Id, u => (u.FullName!, u.ImagePath!));
        }

        // I did it for better performance to select specific columns that I need it only...


        public async Task<Doctor> GetDoctorById(int doctorId)
        {
            var query = await _context.Doctors.AsNoTracking()
                                                   .AsSplitQuery()
                                                   .Include(i => i.Clinics)
                                                   .ThenInclude(i => i.WorkingTimes)
                                                   .ThenInclude(i => i.Periods)
                                                   .Include(x => x.Reviews)
                                                   .Include(x => x.Specializations)
                                                   .Include(x => x.AppointmentTypes)
                                                   .Include(x => x.Educations)
                                                   .Include(x => x.Experiences)
                                                   .Include(x => x.Awards)
                                                   .FirstOrDefaultAsync(x => x.Id == doctorId);

            return query!;
        }

       
        public async Task<DoctorProfileResponse> GetDoctorProfile(int appUserIdFromClaims)
        {
            var doctorQuery = await (
                from doctor in _context.Doctors
                where doctor.AppUserId == appUserIdFromClaims
                select new
                {   
                    Doctor = doctor,
                    Rating = doctor.Reviews.Any() ? doctor.Reviews.Average(r => r.Rate) : 0,
                    Specializations = doctor.Specializations.Select(s => new SpecializationResponse
                    {
                        Id = s.Id.ToString(),
                        Name = s.Name,
                        Category = s.Category
                    }).ToList(),
                    Awards = doctor.Awards.Select(a => new AwardResponse
                    {
                        DateReceived = a.DateReceived,
                        Description = a.Description,
                        Organization = a.Organization,
                        Title = a.Title,
                        Id = a.Id.ToString()
                    }).ToList(),
                    Education = doctor.Educations.Select(e => new EducationResponse
                    {
                        AdditionalNotes = e.AdditionalNotes,
                        CurrentlyStudying = e.CurrentlyStudying,
                        Degree = e.Degree,
                        EndDate = e.EndDate,
                        Id = e.Id.ToString(),
                        Location = e.Location,
                        Major = e.Major,
                        StartDate = e.StartDate,
                        UniversityLogoPath = e.UniversityLogoPath != null ? $"{_baseUrl}{e.UniversityLogoPath}" : string.Empty,
                        UniversityName = e.UniversityName
                    }).ToList(),
                    Experiences = doctor.Experiences.Select(e => new ExperienceResponse
                    {
                        StartDate = e.StartDate,
                        CurrentlyWorkingHere = e.CurrentlyWorkingHere,
                        EmploymentType = e.EmploymentType,
                        EndDate = e.EndDate,
                        HospitalLogo = e.HospitalLogo != null ? $"{_baseUrl}{e.HospitalLogo}" : string.Empty,
                        HospitalName = e.HospitalName,
                        Id = e.Id.ToString(),
                        JobDescription = e.JobDescription,
                        Location = e.Location,
                        Title = e.Title
                    }).ToList()
                }).FirstOrDefaultAsync();

            if (doctorQuery == null)
                return null!; // Handle not found case properly

            var user = await _identityDbContext.Users
                .Where(u => u.Id == appUserIdFromClaims)
                .Select(u => new
                {
                    u.FirstName,
                    u.LastName,
                    Email = u.Email ?? string.Empty,
                    ImageUrl = u.ImagePath != null ? $"{_baseUrl}{u.ImagePath}" : string.Empty,
                    PhoneNumber = u.PhoneNumber ?? string.Empty,
                    u.Age,
                    u.DateOfBirth,
                    u.Gender,
                    Address = u.Address != null ? new AddressUserResponse
                    {
                        City = u.Address.City,
                        Id = u.Address.Id,
                        Country = u.Address.Country,
                        Street = u.Address.Street
                    } : null
                })
                .FirstOrDefaultAsync();

            if (user == null)
                return null!; // Handle user not found case

            return new DoctorProfileResponse
            {
                FirstName = user.FirstName,
                LastName = user.LastName,
                Email = user.Email,
                ImageUrl = user.ImageUrl,
                PhoneNumber = user.PhoneNumber,
                TotalYearsOfExperience = doctorQuery.Experiences.Sum(exp =>
                (exp.CurrentlyWorkingHere ? DateTime.UtcNow.Year : exp.EndDate?.Year ?? DateTime.UtcNow.Year) - exp.StartDate.Year),
                Rating = doctorQuery.Rating,
                Specializations = doctorQuery.Specializations,
                AboutMe = doctorQuery.Doctor.AboutMe,
                Address = user.Address,
                NumberOfReviews = doctorQuery.Doctor.NumberOfReviews,
                Id = doctorQuery.Doctor.Id.ToString(),
                Age = user.Age,
                DateOfBirth = user.DateOfBirth,
                Gender = user.Gender,
                LicenseNumber = doctorQuery.Doctor.LicenseNumber,
                Awards = doctorQuery.Awards,
                Education = doctorQuery.Education,
                Experiences = doctorQuery.Experiences
            };
        
        }

        public async Task<DoctorEarningsResponse> GetEarningsReportAsync(int doctorId, DateTimeOffset startDate, DateTimeOffset endDate)
        {
            var id = await _context.Doctors.Where(x => x.AppUserId == doctorId).Select(x => x.Id).FirstOrDefaultAsync();

            // Filter Payments by Doctor & Date Range
            var paymentsQuery = _context.Payments.Include(x=> x.Appointment)
                .Where(x => x.Appointment.DoctorId == id && x.CreatedAt >= startDate && x.CreatedAt <= endDate);

            var totalEarnings = await paymentsQuery.SumAsync(x => x.Amount);
            var totalPaidEarnings = await paymentsQuery.Where(x => x.Status == PaymentStatus.Paid).SumAsync(x => x.Amount);
            var totalPendingEarnings = await paymentsQuery.Where(x => x.Status == PaymentStatus.Pending).SumAsync(x => x.Amount);
            var totalRefundedEarnings = await paymentsQuery.Where(x => x.Status == PaymentStatus.Refunded).SumAsync(x => x.Amount);

            // Filter Appointments by Doctor & Date Range
            var appointmentsQuery = _context.Appointments
                .Where(x => x.Doctor.AppUserId == doctorId && x.CreatedAt >= startDate && x.CreatedAt <= endDate);

            var appointmentStats = await appointmentsQuery
                .GroupBy(x => x.AppointmentStatus)
                .Select(group => new
                {
                    Status = group.Key,
                    Count = group.Count()
                }).ToListAsync();

            // Extract counts from grouped query results
            var completedAppointmentsCount = appointmentStats.FirstOrDefault(x => x.Status == AppointmentStatus.Completed)?.Count ?? 0;
            var pendingAppointmentsCount = appointmentStats.FirstOrDefault(x => x.Status == AppointmentStatus.PendingPayment)?.Count ?? 0;
            var cancelledAppointmentsCount = appointmentStats
                .Where(x => x.Status == AppointmentStatus.CanceledByPatient || x.Status == AppointmentStatus.CanceledByDoctor)
                .Sum(x => x.Count);
            var totalAppointments = appointmentStats.Sum(x => x.Count);

            return new DoctorEarningsResponse
            {
                TotalEarnings = totalEarnings,
                TotalPaidEarnings = totalPaidEarnings,
                TotalPendingEarnings = totalPendingEarnings,
                TotalRefundedEarnings = totalRefundedEarnings,
                TotalAppointments = totalAppointments,
                CompletedAppointmentsCount = completedAppointmentsCount,
                PendingAppointmentsCount = pendingAppointmentsCount,
                CancelledAppointmentsCount = cancelledAppointmentsCount
            };
        }

    }
}

/*
 
 app ==> 1/1/2022
 app ==> 11/1/2022
 app ==> 19/1/2022
 app ==> 23/1/2022
 
1/1/2022

23/1/2022
 
 */