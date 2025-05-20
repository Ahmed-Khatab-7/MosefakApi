namespace MosefakApi.Business.Services
{
    public class DoctorService : IDoctorService
    {

        private readonly IUnitOfWork _unitOfWork;
        private readonly IUserRepository _userRepository;
        private readonly ICacheService _cacheService;
        private readonly IImageService _imageService;
        private readonly IConfiguration _configuration;
        private readonly UserManager<AppUser> _userManager;
        private readonly IMapper _mapper;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly string _baseUrl;

        public DoctorService(IUnitOfWork unitOfWork, IUserRepository userRepository, ICacheService cacheService, IImageService imageService, IConfiguration configuration, UserManager<AppUser> userManager, IMapper mapper, IHttpContextAccessor httpContextAccessor)
        {
            _unitOfWork = unitOfWork;
            _userRepository = userRepository;
            _cacheService = cacheService;
            _imageService = imageService;
            _configuration = configuration;
            _userManager = userManager;
            _mapper = mapper;
            _httpContextAccessor = httpContextAccessor;
            _baseUrl = _configuration["BaseUrl"] ?? "https://default-url.com/";
        }

        public async Task<PaginatedResponse<DoctorResponse>> GetAllDoctors(int pageNumber = 1, int pageSize = 10)
        {
            var doctorRepo = _unitOfWork.GetCustomRepository<IDoctorRepositoryAsync>();

            // Fetch paginated doctors with related entities
            (var doctors, var totalCount) = await doctorRepo.GetAllAsync(
                query => query.Include(x => x.Experiences).Include(x => x.Specializations).Include(x => x.Reviews),
                pageNumber,
                pageSize);

            // Calculate total pages
            int totalPages = (int)Math.Ceiling((double)totalCount / pageSize);

            if (!doctors.Any()) // No need for doctors == null check
            {
                return new PaginatedResponse<DoctorResponse>
                {
                    Data = new List<DoctorResponse>(),
                    CurrentPage = pageNumber,
                    PageSize = pageSize,
                    TotalPages = totalPages
                };
            }

            // Extract unique AppUser IDs for batch fetching user details
            var appUserIds = doctors.Select(d => d.AppUserId).ToHashSet();

            // Fetch user details in a single batch request
            var userDetails = await doctorRepo.GetUserDetailsAsync(appUserIds);

            // Map results and return paginated response
            return new PaginatedResponse<DoctorResponse>
            {
                Data = doctors.Select(d => MapDoctorResponse(d, userDetails)).ToList(),
                TotalPages = totalPages,
                PageSize = pageSize,
                CurrentPage = pageNumber
            };
        }


        public async Task<DoctorDetail> GetDoctorById(int doctorId)
        {
            var doctor = await _unitOfWork.GetCustomRepository<IDoctorRepositoryAsync>().GetDoctorById(doctorId);

            if (doctor is null)
                throw new ItemNotFound("Doctor is not exist");

            var reviewerIds = doctor.Reviews.Select(x => x.AppUserId).ToHashSet();
            reviewerIds.Add(doctor.AppUserId);

            var userDetails = await _unitOfWork.GetCustomRepository<IDoctorRepositoryAsync>().GetUserDetailsAsync(reviewerIds);

            var numberPatinets = await _unitOfWork.Repository<Appointment>()
                .GetCountWithConditionAsync(x => x.DoctorId == doctorId &&
                x.AppointmentStatus != AppointmentStatus.CanceledByDoctor &&
                x.AppointmentStatus != AppointmentStatus.CanceledByPatient);

            // Map the results
            return MapDoctorDetailResponse(doctor, userDetails, numberPatinets);
        }

        public async Task<List<DoctorResponse>?> TopTenDoctors()
        {
            var query = await _unitOfWork.GetCustomRepository<IDoctorRepositoryAsync>().GetTopTenDoctors();

            if (query is null)
                return new List<DoctorResponse>();

            return query;
        }

        public async Task AddDoctor(DoctorRequest request)
        {
            if (request is null)
                throw new BadRequest("Data is null");

            // check if AppUserId is exist or not

            var user = await _userRepository.GetUserByIdAsync(request.AppUserId);

            if (user is null)
                throw new ItemNotFound("this doctor is not registered!");

            var doctor = _mapper.Map<Doctor>(request);

            await _unitOfWork.GetCustomRepository<IDoctorRepositoryAsync>().AddEntityAsync(doctor);
            await _unitOfWork.CommitAsync();


            // ✅ Remove relevant cache entries after adding a doctor
            await RemoveCachedDoctorData(doctor.Id);
        }

        public async Task CompleteDoctorProfile(int appUserIdFromClaims, CompleteDoctorProfileRequest doctorRequest, CancellationToken cancellationToken = default)
        {
            var strategy = _unitOfWork.CreateExecutionStrategy();

            await strategy.ExecuteAsync(async () =>
            {
                await using var transaction = await _unitOfWork.BeginTransactionAsync();

                var user = await _userRepository.GetUserByIdAsync(appUserIdFromClaims)
                    ?? throw new ItemNotFound("This doctor is not registered!");

                if (await _unitOfWork.Repository<Doctor>().AnyAsync(d => d.AppUserId == appUserIdFromClaims))
                    throw new BadRequest("Doctor profile has already been completed!");

                var doctor = _mapper.Map<Doctor>(doctorRequest);
                doctor.AppUserId = appUserIdFromClaims;

                List<string> uploadedImages = new();

                try
                {
                    // 🔹 Validate & Upload Clinic Images
                    doctor.Clinics = new List<Clinic>();
                    foreach (var clinicRequest in doctorRequest.Clinics)
                    {
                        string? clinicImagePath = null, logoPath = null;

                        if (clinicRequest.ClinicImage != null)
                        {
                            ValidateImageFile(clinicRequest.ClinicImage);
                            clinicImagePath = await _imageService.UploadImageOnServer(clinicRequest.ClinicImage, false, null, cancellationToken);
                            uploadedImages.Add(clinicImagePath);
                        }

                        if (clinicRequest.LogoPath != null)
                        {
                            ValidateImageFile(clinicRequest.LogoPath);
                            logoPath = await _imageService.UploadImageOnServer(clinicRequest.LogoPath, false, null, cancellationToken);
                            uploadedImages.Add(logoPath);
                        }

                        doctor.Clinics.Add(new Clinic
                        {
                            Name = clinicRequest.Name,
                            Street = clinicRequest.Street,
                            City = clinicRequest.City,
                            Country = clinicRequest.Country,
                            ApartmentOrSuite = clinicRequest.ApartmentOrSuite,
                            Landmark = clinicRequest.Landmark,
                            PhoneNumber = clinicRequest.PhoneNumber,
                            ClinicImage = clinicImagePath,
                            LogoPath = logoPath,
                            WorkingTimes = clinicRequest.WorkingTimes?.Select(w => new WorkingTime
                            {
                                Day = w.Day,
                                Periods = w.Periods.Select(p => new Period { StartTime = p.StartTime, EndTime = p.EndTime }).ToList()
                            }).ToList() ?? new()
                        });
                    }

                    // 🔹 Validate & Upload University Logos (Education)
                    doctor.Educations = new List<Education>();
                    foreach (var educationRequest in doctorRequest.Educations ?? new())
                    {
                        string? universityLogoPath = null;

                        if (educationRequest.UniversityLogoPath != null)
                        {
                            ValidateImageFile(educationRequest.UniversityLogoPath);
                            universityLogoPath = await _imageService.UploadImageOnServer(educationRequest.UniversityLogoPath,false,null, cancellationToken);
                            uploadedImages.Add(universityLogoPath);
                        }

                        doctor.Educations.Add(new Education
                        {
                            Degree = educationRequest.Degree,
                            Major = educationRequest.Major,
                            UniversityName = educationRequest.UniversityName,
                            UniversityLogoPath = universityLogoPath,
                            Location = educationRequest.Location,
                            StartDate = educationRequest.StartDate,
                            EndDate = educationRequest.EndDate,
                            CurrentlyStudying = educationRequest.CurrentlyStudying
                        });
                    }

                    // 🔹 Validate & Upload Hospital Logos (Experience)
                    doctor.Experiences = new List<Experience>();
                    foreach (var experienceRequest in doctorRequest.Experiences ?? new())
                    {
                        string? hospitalLogoPath = null;

                        if (experienceRequest.HospitalLogo != null)
                        {
                            ValidateImageFile(experienceRequest.HospitalLogo);
                            hospitalLogoPath = await _imageService.UploadImageOnServer(experienceRequest.HospitalLogo, false, null, cancellationToken);
                            uploadedImages.Add(hospitalLogoPath);
                        }

                        doctor.Experiences.Add(new Experience
                        {
                            Title = experienceRequest.Title,
                            HospitalName = experienceRequest.HospitalName,
                            HospitalLogo = hospitalLogoPath,
                            Location = experienceRequest.Location,
                            EmploymentType = experienceRequest.EmploymentType,
                            JobDescription = experienceRequest.JobDescription,
                            StartDate = experienceRequest.StartDate,
                            EndDate = experienceRequest.EndDate,
                            CurrentlyWorkingHere = experienceRequest.CurrentlyWorkingHere
                        });
                    }

                    await _unitOfWork.Repository<Doctor>().AddEntityAsync(doctor);
                    await _unitOfWork.CommitAsync();

                    var roles = await _userManager.GetRolesAsync(user);
                    await _userManager.RemoveFromRolesAsync(user, roles);
                    await _userManager.AddToRoleAsync(user, DefaultRole.Doctor);

                    // convert usertype from PendingDoctor to Doctor after complete
                    user.UserType = UserType.Doctor;
                    await _userManager.UpdateAsync(user);

                    await transaction.CommitAsync();
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();

                    // 🔹 Rollback Uploaded Images if Failure Occurs
                    foreach (var imagePath in uploadedImages)
                    {
                        await _imageService.RemoveImage(imagePath);
                    }

                    throw new BadRequest("Failed to complete doctor profile.", ex);
                }
            });
        }



        public async Task<DoctorProfileResponse> GetDoctorProfile(int appUserIdFromClaims)
        {
            var profile = await _unitOfWork.GetCustomRepository<IDoctorRepositoryAsync>().GetDoctorProfile(appUserIdFromClaims);

            if (profile is null)
                return new DoctorProfileResponse();

            return profile;
        }

        public async Task UpdateDoctorProfile(DoctorProfileUpdateRequest request, int appUserIdFromClaims, CancellationToken cancellationToken = default)
        {
            var strategy = _unitOfWork.CreateExecutionStrategy(); // ✅ Now using UnitOfWork method

            await strategy.ExecuteAsync(async () =>
            {
                await using var transaction = await _unitOfWork.BeginTransactionAsync();

                try
                {
                    // 1) Fetch User from Identity Database
                    var user = await _userRepository.GetUserByIdAsync(appUserIdFromClaims);
                    if (user is null)
                        throw new ItemNotFound("This doctor is not registered!");

                    // 2) Fetch Doctor from Business Database
                    var doctorRepo = _unitOfWork.GetCustomRepository<IDoctorRepositoryAsync>();
                    var doctor = await doctorRepo.FirstOrDefaultAsync(x => x.AppUserId == appUserIdFromClaims);
                    if (doctor is null)
                        throw new ItemNotFound("This profile does not exist!");

                    bool userUpdated = false, doctorUpdated = false;

                    // 3) Update User Properties (only if changed)
                    if (!string.Equals(user.FirstName, request.FirstName, StringComparison.OrdinalIgnoreCase))
                    {
                        user.FirstName = request.FirstName!;
                        userUpdated = true;
                    }

                    if (!string.Equals(user.LastName, request.LastName, StringComparison.OrdinalIgnoreCase))
                    {
                        user.LastName = request.LastName!;
                        userUpdated = true;
                    }

                    if (!string.Equals(user.PhoneNumber, request.PhoneNumber, StringComparison.OrdinalIgnoreCase))
                    {
                        user.PhoneNumber = request.PhoneNumber!;
                        userUpdated = true;
                    }

                    if (request.DateOfBirth.HasValue && user.DateOfBirth != request.DateOfBirth)
                    {
                        user.DateOfBirth = request.DateOfBirth.Value;
                        userUpdated = true;
                    }

                    if (request.Gender.HasValue && user.Gender != request.Gender)
                    {
                        user.Gender = request.Gender.Value;
                        userUpdated = true;
                    }

                    if (request.Address != null)
                    {
                        user.Address ??= new Address();
                        if (!string.Equals(user.Address.Country, request.Address.Country, StringComparison.OrdinalIgnoreCase) ||
                            !string.Equals(user.Address.City, request.Address.City, StringComparison.OrdinalIgnoreCase) ||
                            !string.Equals(user.Address.Street, request.Address.Street, StringComparison.OrdinalIgnoreCase))
                        {
                            user.Address.Country = request.Address.Country;
                            user.Address.City = request.Address.City;
                            user.Address.Street = request.Address.Street;
                            userUpdated = true;
                        }
                    }

                    // 4) Update Doctor Properties (only if changed)
                    if (!string.Equals(doctor.AboutMe, request.AboutMe, StringComparison.OrdinalIgnoreCase))
                    {
                        doctor.AboutMe = request.AboutMe!;
                        doctorUpdated = true;
                    }

                    if (!string.Equals(doctor.LicenseNumber, request.LicenseNumber, StringComparison.OrdinalIgnoreCase))
                    {
                        doctor.LicenseNumber = request.LicenseNumber!;
                        doctorUpdated = true;
                    }

                    // 5) Save changes only if needed
                    if (userUpdated)
                    {
                        await _userRepository.UpdateUser(user);
                        var rowsAffected = await _userRepository.Save();
                        if (rowsAffected <= 0)
                            throw new Exception("Failed to update user profile.");
                    }

                    if (doctorUpdated)
                    {
                        await doctorRepo.UpdateEntityAsync(doctor);
                        await _unitOfWork.CommitAsync();
                    }

                    await transaction.CommitAsync(); // ✅ Commit EF Transaction

                    // ✅ Remove relevant cache entries after updating a doctor
                    await RemoveCachedDoctorData(doctor.Id);
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync(); // ✅ Rollback on failure
                    throw new Exception("Failed to update doctor profile", ex);
                }
            });
        }



        public async Task DeleteDoctor(int doctorId)
        {
            var doctor = await _unitOfWork.GetCustomRepository<IDoctorRepositoryAsync>().GetByIdAsync(doctorId);

            if (doctor is null)
                throw new ItemNotFound("Doctor is not exist");

            await _unitOfWork.GetCustomRepository<IDoctorRepositoryAsync>().DeleteEntityAsync(doctor);
            await _unitOfWork.CommitAsync();

            // ✅ Remove relevant cache entries after deleting a doctor
            await RemoveCachedDoctorData(doctorId);
        }


        public async Task<List<TimeSlot>> GetAvailableTimeSlots(int doctorId, int clinicId, int appointmentTypeId, DayOfWeek selectedDay)
        {
            // Step 1: Fetch Doctor's Working Periods for the Selected Clinic & Day
            var workingTime = await _unitOfWork.Repository<WorkingTime>()
                .FirstOrDefaultAsync(wt => wt.ClinicId == clinicId && wt.Day == selectedDay, query => query.Include(x => x.Periods));

            if (workingTime is null || !workingTime.Periods.Any())
                return new List<TimeSlot>(); // No working periods available.

            // Step 2: Fetch Appointment Type Duration (in minutes)
            var appointmentType = await _unitOfWork.Repository<AppointmentType>()
                .FirstOrDefaultAsync(at => at.Id == appointmentTypeId);

            if (appointmentType is null)
                throw new BadRequest("Invalid appointment type");

            var appointmentDuration = appointmentType.Duration; // Example: 30 minutes

            // Step 3: Fetch **All** Booked Appointments for this Doctor on Selected Day (Any Type)
            // Step 1: Fetch all appointments for the doctor (query-friendly condition)
            var allAppointments = await _unitOfWork.Repository<Appointment>()
                .GetAllAsync(a => a.DoctorId == doctorId);

            // Step 2: Filter in memory to match the selected day
            var bookedAppointments = allAppointments
                .Where(a => a.StartDate.DayOfWeek == selectedDay)
                .ToList();

            var bookedTimes = bookedAppointments
                .Select(a => new
                {
                    StartTime = TimeOnly.FromDateTime(a.StartDate.DateTime),
                    EndTime = TimeOnly.FromDateTime(a.EndDate.DateTime)
                }).ToList();

            var availableSlots = new List<TimeSlot>();

            // Step 4: Generate Time Slots Based on **Selected Appointment Duration**
            foreach (var period in workingTime.Periods)
            {
                var slotStart = period.StartTime;
                var slotEnd = slotStart.Add(appointmentDuration);

                while (slotEnd <= period.EndTime)
                {
                    // ❌ Check if this slot **overlaps with ANY booked appointment**
                    bool isBooked = bookedTimes.Any(appt =>
                        (slotStart < appt.EndTime && slotEnd > appt.StartTime)); // Overlapping condition

                    if (!isBooked)
                    {
                        availableSlots.Add(new TimeSlot
                        {
                            StartTime = slotStart,
                            EndTime = slotEnd
                        });
                    }

                    // Move to next slot
                    slotStart = slotEnd;
                    slotEnd = slotStart.Add(appointmentDuration);
                }
            }

            return availableSlots;
        }


        public async Task<bool> UploadProfileImageAsync(int doctorId, IFormFile imageFile, CancellationToken cancellationToken = default) // [Done]
        {
            if (imageFile == null || imageFile.Length == 0)
                throw new BadRequest("Invalid image file.");

            // 🔹 Validate Image File (Size & Format)
            ValidateImageFile(imageFile);

            // 🔹 Fetch Doctor (From Primary Database)
            var doctor = await _unitOfWork.GetCustomRepository<IDoctorRepositoryAsync>().AnyAsync(x=> x.AppUserId == doctorId);
            if (!doctor)
                throw new ItemNotFound("Doctor does not exist.");

            // 🔹 Fetch User Details (From Secondary Database)
            var doctorDetails = await _userRepository.GetUserByIdAsync(doctorId);
            if (doctorDetails is null)
                throw new ItemNotFound("Doctor user details not found.");

            var oldImagePath = doctorDetails.ImagePath ?? string.Empty;
            string? newImagePath = null;

            try
            {
                // 🔹 Upload New Image
                newImagePath = await _imageService.UploadImageOnServer(imageFile, false, oldImagePath, cancellationToken);
                doctorDetails.ImagePath = newImagePath;

                // 🔹 Use Transaction to Ensure Atomicity
                using var transaction = await _unitOfWork.BeginTransactionAsync();
                
                await _userRepository.UpdateUser(doctorDetails);
              
                var rowsAffected = await _userRepository.Save(); // ✅ Update in Secondary DB
                if (rowsAffected > 0)
                {
                    await transaction.CommitAsync();

                    // 🔹 Delete Old Image (Only If DB Commit is Successful)
                    if (!string.IsNullOrEmpty(oldImagePath))
                        await _imageService.RemoveImage($"{oldImagePath}");

                    return true;
                }
                else
                {
                    await transaction.RollbackAsync();

                    // 🔹 Rollback Image Upload if DB Update Fails
                    if (!string.IsNullOrEmpty(newImagePath))
                        await _imageService.RemoveImage($"{newImagePath}");

                    throw new BadRequest("Failed to update image.");
                }
            }
            catch (Exception ex)
            {
                throw new BadRequest("Failed to upload doctor profile image.", ex);
            }
        }
        

        public async Task<bool> UpdateWorkingTimesAsync(int doctorId, int clinicId, IEnumerable<WorkingTimeRequest> workingTimes)
        {
            // 🔹 Fetch clinic along with WorkingTimes & Periods
            var clinic = await _unitOfWork.GetCustomRepository<IClinicRepository>().FirstOrDefaultAsync(
                x => x.Id == clinicId && x.Doctor.AppUserId == doctorId,
                query => query.Include(x => x.WorkingTimes).ThenInclude(x => x.Periods).Include(x => x.Doctor));

            if (clinic is null)
                throw new ItemNotFound("Clinic does not exist or you do not have permission to modify it.");

            var strategy = _unitOfWork.CreateExecutionStrategy(); // ✅ Now using UnitOfWork method

            return await strategy.ExecuteAsync(async () =>
            {
                using var transaction = await _unitOfWork.BeginTransactionAsync();

                try
                {
                    var existingWorkingTimes = clinic.WorkingTimes.ToList();

                    foreach (var request in workingTimes)
                    {
                        var existingWorkingTime = existingWorkingTimes.FirstOrDefault(x => x.Day == request.Day);

                        if (existingWorkingTime == null)
                        {
                            // 🔹 If No Existing WorkingTime → Add New
                            var newWorkingTime = new WorkingTime
                            {
                                ClinicId = clinicId,
                                Day = request.Day,
                                Periods = request.Periods.Select(p => new Period
                                {
                                    StartTime = p.StartTime,
                                    EndTime = p.EndTime
                                }).ToHashSet()
                            };

                            clinic.WorkingTimes.Add(newWorkingTime);
                        }
                        else
                        {
                            // 🔹 If WorkingTime Exists → Update Periods
                            // Remove existing periods before adding new ones
                            var periodsToRemove = existingWorkingTime.Periods.ToList();
                            foreach (var period in periodsToRemove)
                            {
                                await _unitOfWork.Repository<Period>().DeleteEntityAsync(period);
                            }

                            // Add new periods
                            foreach (var periodRequest in request.Periods)
                            {
                                existingWorkingTime.Periods.Add(new Period
                                {
                                    StartTime = periodRequest.StartTime,
                                    EndTime = periodRequest.EndTime
                                });
                            }
                        }
                    }

                    // Commit changes
                    var rowsAffected = await _unitOfWork.CommitAsync();

                    // If rowsAffected is greater than 0, return true, otherwise false
                    if (rowsAffected > 0)
                    {
                        await transaction.CommitAsync();
                        return true;
                    }

                    // If no rows were affected, rollback and return false
                    await transaction.RollbackAsync();
                    return false;
                }
                catch (Exception ex)
                {
                    // Rollback in case of error
                    await transaction.RollbackAsync();
                    // Log exception if needed
                    throw new Exception("Failed to update working times.", ex);
                }
            });
        }



        public async Task<PaginatedResponse<DoctorResponse>> SearchDoctorsAsync(DoctorSearchFilter filter, int pageNumber = 1, int pageSize = 10)
        {
            if (string.IsNullOrWhiteSpace(filter.Name))
                return new PaginatedResponse<DoctorResponse>
                {
                    CurrentPage = pageNumber,
                    Data = [],
                    PageSize = pageSize,
                    TotalPages = 0
                };

            var normalizedName = filter.Name.Trim().ToLower();

            // Fetch matching users
            var users = await _userRepository.GetAllUsersAsync(x =>
                EF.Functions.Like(x.FirstName + " " + x.LastName, $"%{normalizedName}%"));

            if (!users.Any())
                return new PaginatedResponse<DoctorResponse>
                {
                    CurrentPage = pageNumber,
                    Data = [],
                    PageSize = pageSize,
                    TotalPages = 0
                };

            // Create dictionary for fast lookup
            var userDictionary = users.ToDictionary(
                u => u.Id,
                u => new
                {
                    FullName = $"{u.FirstName} {u.LastName}",
                    ImagePath = _baseUrl + (!string.IsNullOrWhiteSpace(u.ImagePath) ? u.ImagePath : "default.jpg")
                });

            var userIds = users.Select(x => x.Id);

            // Retrieve doctor details with paging
            var (doctors, totalCount) = await _unitOfWork
                .GetCustomRepository<DoctorRepositoryAsync>()
                .GetAllAsync(
                    d => userIds.Contains(d.AppUserId),
                    query => query.Include(x => x.Specializations).Include(x => x.Experiences),
                    pageNumber,
                    pageSize);

            var totalPages = (int)Math.Ceiling((double)totalCount / pageSize);

            // Map the final result
            var doctorResponse = doctors.Select(d => new DoctorResponse
            {
                Id = d.Id.ToString(),
                FullName = userDictionary[d.AppUserId].FullName,
                ImagePath = userDictionary[d.AppUserId].ImagePath,
                TotalYearsOfExperience = d.TotalYearsOfExperience,
                Specializations = d.Specializations.Select(x => new SpecializationResponse
                {
                    Id = x.Id.ToString(),
                    Name = x.Name,
                    Category = x.Category
                }).ToList(),
                NumberOfReviews = d.NumberOfReviews,
            }).ToList();

            return new PaginatedResponse<DoctorResponse>
            {
                Data = doctorResponse,
                CurrentPage = pageNumber,
                PageSize = pageSize,
                TotalPages = totalPages
            };
        }

        public async Task<PaginatedResponse<DoctorResponse>> SearchDoctorsBySpecialityAsync(DoctorSearchBySpecialityCategoryFilter filter, int pageNumber = 1, int pageSize = 10)
        {

            (var specializations, var totalCount) = await _unitOfWork.Repository<Specialization>()
                .GetAllAsync(s => s.Category == filter.Category,
                query =>
                query.Include(x => x.Doctor)
                .ThenInclude(x=> x.Experiences),
                pageNumber, pageSize);

            var totalPages = (int)((double)totalCount / pageSize);

            if (!specializations.Any())
                return new PaginatedResponse<DoctorResponse>
                {
                    CurrentPage = pageNumber,
                    Data = [],
                    PageSize = pageSize,
                    TotalPages = totalPages
                };

            // Extract unique user IDs
            var userIds = specializations.Select(s => s.Doctor.AppUserId).Distinct().ToList();

            // Fetch user details in one query and store in a dictionary
            var userDictionary = await _userManager.Users
                .Where(u => userIds.Contains(u.Id))
                .Select(u => new
                {
                    u.Id,
                    FullName = $"{u.FirstName} {u.LastName}",
                    ImagePath = _baseUrl + (!string.IsNullOrWhiteSpace(u.ImagePath) ? u.ImagePath : "default.jpg")
                })
                .ToDictionaryAsync(u => u.Id);

          
            // Map the final result
            var doctorResponse = new List<DoctorResponse>();

            foreach (var res in specializations)
            {

                if (userDictionary.TryGetValue(res.Doctor.AppUserId, out var user))
                {
                    doctorResponse.Add(new DoctorResponse
                    {
                        Id = res.Doctor.Id.ToString(),
                        FullName = user.FullName,
                        ImagePath = user.ImagePath,
                        TotalYearsOfExperience = res.Doctor.TotalYearsOfExperience,
                        NumberOfReviews = res.Doctor.NumberOfReviews,
                        Specializations = res.Doctor.Specializations.Select(s => new SpecializationResponse
                        {
                            Id = s.Id.ToString(),
                            Name = s.Name,
                            Category = s.Category
                        }).ToList()
                    });
                }
            }

            return new PaginatedResponse<DoctorResponse>
            {
                Data = doctorResponse,
                CurrentPage = pageNumber,
                PageSize = pageSize,
                TotalPages = totalPages
            };
        }

        public async Task<PaginatedResponse<AppointmentDto>> GetUpcomingAppointmentsAsync(int doctorId, int pageNumber = 1, int pageSize = 10)
        {
            (var response,var totalPages) = await GetAppointmentsAsync(doctorId, isUpcoming: true, pageNumber, pageSize);

            return new PaginatedResponse<AppointmentDto>
            {
                Data = response,
                CurrentPage = pageNumber,
                PageSize = pageSize,
                TotalPages = totalPages
            };
        }

        public async Task<PaginatedResponse<AppointmentDto>> GetPastAppointmentsAsync(int doctorId, int pageNumber = 1, int pageSize = 10)
        {
            (var response, var totalPages) = await GetAppointmentsAsync(doctorId, isUpcoming: false, pageNumber, pageSize);

            return new PaginatedResponse<AppointmentDto>
            {
                Data = response,
                CurrentPage = pageNumber,
                PageSize = pageSize,
                TotalPages = totalPages
            };
        }

        public async Task<long> GetTotalAppointmentsAsync(int doctorId)
        {
            // 🔹 Step 1: Ensure Doctor Exists
            var doctorExists = await _unitOfWork.Repository<Doctor>().AnyAsync(x => x.AppUserId == doctorId);
            if (!doctorExists)
                throw new ItemNotFound("Doctor does not exist.");

            var count = await _unitOfWork.GetCustomRepository<AppointmentRepositoryAsync>()
                .GetCountWithConditionAsync(a => a.Doctor.AppUserId == doctorId, query => query.Include(x => x.Doctor));

            return count;
        }

        public async Task<bool> AddSpecializationAsync(int doctorId, SpecializationRequest request)
        {
            var doctor = await _unitOfWork.GetCustomRepository<IDoctorRepositoryAsync>()
                .FirstOrDefaultAsync(x => x.AppUserId == doctorId, query => query.Include(x => x.Specializations));

            if (doctor is null)
                throw new ItemNotFound("Doctor does not exist.");

            if (doctor.Specializations.Any(x => x.Name == request.Name && x.Category == request.Category))
                throw new BadRequest("Specialization already exists for this doctor.");

            doctor.Specializations.Add(new Specialization
            {
                DoctorId = doctor.Id,
                Name = request.Name,
                Category = request.Category,
            });

            return await _unitOfWork.CommitAsync() > 0;
        }


        public async Task<bool> RemoveSpecializationAsync(int doctorId, int specializationId)
        {
            var specialization = await _unitOfWork.Repository<Specialization>().FirstOrDefaultAsync(x => x.Id == specializationId && x.Doctor.AppUserId == doctorId, query => query.Include(x => x.Doctor));

            if (specialization == null)
                throw new ItemNotFound("Specialization is not exist");

            await _unitOfWork.Repository<Specialization>().DeleteEntityAsync(specialization);
            var rowsAffected = await _unitOfWork.CommitAsync();

            if (rowsAffected <= 0)
                throw new BadRequest("Failed to remove specialization.");

            return true;
        }

        public async Task<bool> EditSpecializationAsync(int doctorId, int specializationId, SpecializationRequest request)
        {
            var specialization = await _unitOfWork.Repository<Specialization>().FirstOrDefaultAsync(x => x.Id == specializationId &&
                                                                                                          x.Doctor.AppUserId == doctorId, query => query.Include(x => x.Doctor));

            if (specialization == null)
                throw new ItemNotFound("Specialization is not exist");


            specialization.Name = request.Name;
            specialization.Category = request.Category;

            await _unitOfWork.Repository<Specialization>().UpdateEntityAsync(specialization);
            var rowsAffected = await _unitOfWork.CommitAsync();

            if (rowsAffected <= 0)
            {
                throw new BadRequest("Failed to Edit specialization.");
            }

            return true;
        }

        public async Task<bool> AddExperienceAsync(int doctorId, ExperienceRequest request, CancellationToken cancellationToken = default)
        {
            var strategy = _unitOfWork.CreateExecutionStrategy(); // ✅ Use EF Core Execution Strategy

            return await strategy.ExecuteAsync(async () =>
            {
                // 🔹 Fetch Doctor 
                var doctor = await _unitOfWork.GetCustomRepository<IDoctorRepositoryAsync>().FirstOrDefaultAsync(x => x.AppUserId == doctorId, query => query.Include(x => x.Experiences));
                if (doctor is null)
                    throw new ItemNotFound("Doctor does not exist.");

                // 🔹 Check for Duplicate Experience
                var experienceExists = doctor.Experiences.Any(e =>
                    e.DoctorId == doctor.Id && e.Title == request.Title && e.StartDate == request.StartDate && e.EndDate == request.EndDate);

                if (experienceExists)
                    throw new BadRequest("Experience already exists for this doctor.");

                // 🔹 Validate & Upload Hospital Logo (If Exists)
                string? logoPath = null;
                if (request.HospitalLogo is not null)
                {
                    ValidateImageFile(request.HospitalLogo);
                    logoPath = await _imageService.UploadImageOnServer(request.HospitalLogo, false, null!, cancellationToken);
                }

                doctor.Experiences.Add(new Experience
                {
                    DoctorId = doctor.Id,
                    Title = request.Title,
                    EmploymentType = request.EmploymentType,
                    CurrentlyWorkingHere = request.CurrentlyWorkingHere,
                    HospitalName = request.HospitalName,
                    EndDate = request.EndDate,
                    StartDate = request.StartDate,
                    HospitalLogo = logoPath,
                    Location = request.Location,
                    JobDescription = request.JobDescription,
                });

                await using var transaction = await _unitOfWork.BeginTransactionAsync();

                try
                {
                    await _unitOfWork.GetCustomRepository<IDoctorRepositoryAsync>().UpdateEntityAsync(doctor);
                    if (await _unitOfWork.CommitAsync() <= 0)
                        throw new BadRequest("Failed to add experience.");

                    await transaction.CommitAsync();
                    return true;
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();

                    // 🔥 Remove uploaded image if DB transaction fails
                    if (!string.IsNullOrEmpty(logoPath))
                        _ = _imageService.RemoveImage(logoPath);

                    throw new Exception("Failed to add experience.", ex);
                }
            });
        }


        public async Task<bool> EditExperienceAsync(int doctorId, int experienceId, ExperienceRequest request, CancellationToken cancellationToken = default)
        {
            var strategy = _unitOfWork.CreateExecutionStrategy(); // ✅ Use EF Core Execution Strategy

            return await strategy.ExecuteAsync(async () =>
            {
                // 🔹 Ensure Experience Exists for the Given Doctor
                var experience = await _unitOfWork.Repository<Experience>()
                    .FirstOrDefaultAsync(x => x.Id == experienceId && x.Doctor.AppUserId == doctorId, query => query.Include(x => x.Doctor));

                if (experience is null)
                    throw new ItemNotFound("Experience does not exist.");

                // 🔹 Check for Duplicate Experience
                var isDuplicate = await _unitOfWork.Repository<Experience>().AnyAsync(e =>
                    e.Id != experienceId && e.DoctorId == experience.DoctorId &&
                    e.Title == request.Title && e.StartDate == request.StartDate && e.EndDate == request.EndDate);

                if (isDuplicate)
                    throw new BadRequest("An experience with the same details already exists for this doctor.");

                // 🔹 Validate & Upload Hospital Logo (If Exists)
                string? newLogoPath = null;
                string oldLogoPath = experience.HospitalLogo ?? string.Empty;

                try
                {
                    if (request.HospitalLogo is not null)
                    {
                        ValidateImageFile(request.HospitalLogo);
                        newLogoPath = await _imageService.UploadImageOnServer(request.HospitalLogo, false, null!, cancellationToken);
                    }

                    // 🔹 Update Experience Entity
                    experience.Title = request.Title;
                    experience.EmploymentType = request.EmploymentType;
                    experience.CurrentlyWorkingHere = request.CurrentlyWorkingHere;
                    experience.HospitalName = request.HospitalName;
                    experience.EndDate = request.EndDate;
                    experience.StartDate = request.StartDate;
                    experience.HospitalLogo = newLogoPath ?? oldLogoPath;
                    experience.Location = request.Location;
                    experience.JobDescription = request.JobDescription;

                    await using var transaction = await _unitOfWork.BeginTransactionAsync();

                    await _unitOfWork.Repository<Experience>().UpdateEntityAsync(experience);
                    if (await _unitOfWork.CommitAsync() <= 0)
                        throw new BadRequest("Failed to edit experience.");

                    await transaction.CommitAsync();

                    // 🔹 Remove Old Image After Successful Update
                    if (!string.IsNullOrEmpty(newLogoPath) && !string.IsNullOrEmpty(oldLogoPath) && newLogoPath != oldLogoPath)
                        _ = _imageService.RemoveImage(oldLogoPath);

                    return true;
                }
                catch (Exception ex)
                {
                    await _unitOfWork.RollbackTransactionAsync();

                    // 🔥 Remove new image if an error occurred
                    if (!string.IsNullOrEmpty(newLogoPath))
                        _ = _imageService.RemoveImage(newLogoPath);

                    throw new Exception("Failed to edit experience.", ex);
                }
            });
        }


        public async Task<bool> RemoveExperienceAsync(int doctorId, int experienceId)
        {
            var strategy = _unitOfWork.CreateExecutionStrategy(); // ✅ Use EF Core Execution Strategy

            return await strategy.ExecuteAsync(async () =>
            {
                var experience = await _unitOfWork.Repository<Experience>()
                    .FirstOrDefaultAsync(x => x.Id == experienceId && x.Doctor.AppUserId == doctorId, query => query.Include(x => x.Doctor));

                if (experience is null)
                    throw new ItemNotFound("Experience does not exist.");

                await using var transaction = await _unitOfWork.BeginTransactionAsync();

                try
                {
                    await _unitOfWork.Repository<Experience>().DeleteEntityAsync(experience);
                    if (await _unitOfWork.CommitAsync() <= 0)
                        throw new BadRequest("Failed to remove experience.");

                    await transaction.CommitAsync();
                    return true;
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    throw new Exception("Failed to remove experience.", ex);
                }
            });
        }


        public async Task<bool> AddAwardAsync(int doctorId, AwardRequest request, CancellationToken cancellationToken = default)
        {
            // 🔹 Step 1: Ensure Doctor Exists
            var doctor = await _unitOfWork.Repository<Doctor>().FirstOrDefaultAsync(x => x.AppUserId == doctorId);
            if (doctor is null)
                throw new ItemNotFound("Doctor does not exist.");

            // 🔹 Step 2: Normalize & Validate Award Data
            var normalizedTitle = request.Title?.Trim().ToLower();
            var normalizedDescription = request.Description?.Trim().ToLower();
            var normalizedOrganization = request.Organization?.Trim().ToLower();

            if (string.IsNullOrWhiteSpace(normalizedTitle) || string.IsNullOrWhiteSpace(normalizedOrganization))
                throw new BadRequest("Title and Organization fields are required.");

            // 🔹 Step 3: Check for Duplicate Award
            var isDuplicated = await _unitOfWork.Repository<Award>().AnyAsync(a =>
                a.DoctorId == doctor.Id && 
                a.Title.ToLower() == normalizedTitle &&
                a.Organization.ToLower() == normalizedOrganization &&
                (a.Description == null || a.Description.ToLower() == normalizedDescription));

            if (isDuplicated)
                throw new ItemAlreadyExist("This award already exists.");

            // 🔹 Step 4: Insert New Award
            var award = new Award
            {
                DoctorId = doctor.Id,
                Title = request.Title.Trim(),
                Description = request.Description?.Trim(),
                Organization = request.Organization.Trim(),
                DateReceived = request.DateReceived
            };

            try
            {
                await _unitOfWork.Repository<Award>().AddEntityAsync(award);
                var rowsAffected = await _unitOfWork.CommitAsync();

                if (rowsAffected <= 0)
                {
                    throw new BadRequest("Failed to add award.");
                }

                return true;
            }
            catch (Exception ex)
            {
                throw new Exception("Failed to add award.", ex);
            }
        }


        public async Task<bool> EditAwardAsync(int doctorId, int awardId, AwardRequest request)
        {
            // 🔹 Step 1: Normalize & Validate Award Data
            var (normalizedTitle, normalizedDescription, normalizedOrganization) = NormalizeAwardRequest(request);

            if (string.IsNullOrWhiteSpace(normalizedTitle) || string.IsNullOrWhiteSpace(normalizedOrganization))
                throw new BadRequest("Title and Organization fields are required.");

            // 🔹 Step 2: Fetch Award with Doctor Validation
            var award = await _unitOfWork.Repository<Award>().FirstOrDefaultAsync(a => a.Id == awardId && a.Doctor.AppUserId == doctorId, query => query.Include(x => x.Doctor));
            if (award == null)
                throw new ItemNotFound("Award does not exist or does not belong to this doctor.");

            // 🔹 Step 3: Check for Duplicate Award
            var isDuplicate = await _unitOfWork.Repository<Award>().AnyAsync(a =>
                a.Id != awardId &&
                a.DoctorId == award.DoctorId &&
                a.Title.ToLower() == normalizedTitle &&
                a.Organization.ToLower() == normalizedOrganization &&
                a.DateReceived == award.DateReceived &&
                (a.Description == null || a.Description.ToLower() == normalizedDescription));

            if (isDuplicate)
                throw new ItemAlreadyExist("An award with the same details already exists for this doctor.");

            // 🔹 Step 4: Update Award Details
            award.Title = request.Title.Trim();
            award.Description = request.Description?.Trim();
            award.Organization = request.Organization.Trim();
            award.DateReceived = request.DateReceived;

            await _unitOfWork.Repository<Award>().UpdateEntityAsync(award);
            var rowsAffected = await _unitOfWork.CommitAsync();

            if (rowsAffected <= 0)
                throw new BadRequest("Failed to update award.");

            return true;
        }


        public async Task<bool> RemoveAwardAsync(int doctorId, int awardId)
        {
            // 🔹 Step 1: Fetch Award with Doctor Validation
            var award = await _unitOfWork.Repository<Award>().FirstOrDefaultAsync(a => a.Id == awardId && a.Doctor.AppUserId == doctorId, query => query.Include(x => x.Doctor));

            if (award == null)
                throw new ItemNotFound("Award does not exist or does not belong to this doctor.");

            await _unitOfWork.Repository<Award>().DeleteEntityAsync(award);
            var rowsAffected = await _unitOfWork.CommitAsync();

            if (rowsAffected <= 0)
                throw new BadRequest("Failed to delete award.");

            return true;
        }


        public async Task<bool> AddEducationAsync(int doctorId, EducationRequest request, CancellationToken cancellationToken = default)
        {
            var strategy = _unitOfWork.CreateExecutionStrategy(); // ✅ Use EF Core Execution Strategy

            return await strategy.ExecuteAsync(async () =>
            {
                // 🔹 Step 1: Ensure Doctor Exists
                var doctor = await _unitOfWork.Repository<Doctor>().FirstOrDefaultAsync(x => x.AppUserId == doctorId);
                if (doctor is null)
                    throw new ItemNotFound("Doctor does not exist.");

                // 🔹 Step 2: Check for Duplicate Education Entry
                var isDuplicated = await _unitOfWork.Repository<Education>().AnyAsync(x =>
                    x.DoctorId == doctor.Id &&
                    x.UniversityName == request.UniversityName &&
                    x.Degree == request.Degree &&
                    x.StartDate == request.StartDate &&
                    x.Major == request.Major &&
                    x.Location == request.Location);

                if (isDuplicated)
                    throw new ItemAlreadyExist("This education record already exists.");

                string? newImagePath = null;

                await using var transaction = await _unitOfWork.BeginTransactionAsync(); // ✅ Transaction Handling

                try
                {
                    // 🔹 Step 3: Upload University Logo (if provided)
                    if (request.UniversityLogoPath is not null)
                    {
                        ValidateImageFile(request.UniversityLogoPath);
                        newImagePath = await _imageService.UploadImageOnServer(request.UniversityLogoPath, false, null!, cancellationToken);
                    }

                    // 🔹 Step 4: Create & Add New Education Entry
                    var education = new Education
                    {
                        DoctorId = doctor.Id,
                        UniversityName = request.UniversityName.Trim(),
                        Degree = request.Degree.Trim(),
                        Major = request.Major?.Trim(),
                        StartDate = request.StartDate,
                        EndDate = request.EndDate,
                        CurrentlyStudying = request.CurrentlyStudying,
                        Location = request.Location?.Trim(),
                        AdditionalNotes = request.AdditionalNotes?.Trim(),
                        UniversityLogoPath = newImagePath
                    };

                    await _unitOfWork.Repository<Education>().AddEntityAsync(education);

                    if (await _unitOfWork.CommitAsync() <= 0)
                        throw new BadRequest("Failed to add education.");

                    await transaction.CommitAsync(cancellationToken);
                    return true;
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync(cancellationToken);

                    // 🔹 Step 5: Remove Uploaded Image If Transaction Fails
                    if (!string.IsNullOrEmpty(newImagePath))
                        _ = _imageService.RemoveImage(newImagePath); // Fire & Forget

                    throw new Exception("Failed to add education.", ex);
                }
            });
        }



        public async Task<bool> EditEducationAsync(int doctorId, int educationId, EducationRequest request, CancellationToken cancellationToken = default)
        {
            var strategy = _unitOfWork.CreateExecutionStrategy(); // ✅ Use EF Core Execution Strategy

            return await strategy.ExecuteAsync(async () =>
            {
                // 🔹 Step 1: Ensure Education Exists & Belongs to Doctor
                var education = await _unitOfWork.Repository<Education>()
                    .FirstOrDefaultAsync(x => x.Id == educationId && x.Doctor.AppUserId == doctorId, query => query.Include(x => x.Doctor));

                if (education is null)
                    throw new ItemNotFound("Education record does not exist or you have no permission.");

                string oldImagePath = education.UniversityLogoPath ?? string.Empty;
                string? newImagePath = null;

                // 🔹 Step 2: Upload New Image (if provided)
                if (request.UniversityLogoPath != null)
                {
                    ValidateImageFile(request.UniversityLogoPath);
                    newImagePath = await _imageService.UploadImageOnServer(request.UniversityLogoPath, false, oldImagePath, cancellationToken);
                }

                // 🔹 Step 3: Update Education Entity
                education.StartDate = request.StartDate;
                education.EndDate = request.EndDate;
                education.Location = request.Location?.Trim();
                education.AdditionalNotes = request.AdditionalNotes?.Trim();
                education.CurrentlyStudying = request.CurrentlyStudying;
                education.Degree = request.Degree.Trim();
                education.Major = request.Major?.Trim();
                education.UniversityName = request.UniversityName.Trim();
                education.UniversityLogoPath = newImagePath ?? oldImagePath;

                await using var transaction = await _unitOfWork.BeginTransactionAsync(); // ✅ Transaction Handling

                try
                {
                    await _unitOfWork.Repository<Education>().UpdateEntityAsync(education);

                    if (await _unitOfWork.CommitAsync() <= 0)
                        throw new BadRequest("Failed to edit Education.");

                    await transaction.CommitAsync(cancellationToken);

                    // 🔹 Step 4: Remove Old Image Only After Successful Update
                    if (!string.IsNullOrEmpty(newImagePath) && !string.IsNullOrEmpty(oldImagePath) && newImagePath != oldImagePath)
                    {
                        _ = _imageService.RemoveImage(oldImagePath); // Fire & Forget
                    }

                    return true;
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync(cancellationToken);

                    // 🔥 Remove newly uploaded image if the transaction fails
                    if (!string.IsNullOrEmpty(newImagePath))
                        _ = _imageService.RemoveImage(newImagePath); // Fire & Forget

                    throw new Exception("Failed to edit Education.", ex);
                }
            });
        }



        public async Task<bool> RemoveEducationAsync(int doctorId, int educationId)
        {
            var strategy = _unitOfWork.CreateExecutionStrategy(); // ✅ Use EF Core Execution Strategy

            return await strategy.ExecuteAsync(async () =>
            {
                // 🔹 Step 1: Ensure Education Exists & Belongs to Doctor
                var education = await _unitOfWork.Repository<Education>()
                    .FirstOrDefaultAsync(x => x.Id == educationId && x.Doctor.AppUserId == doctorId, query => query.Include(x => x.Doctor));

                if (education is null)
                    throw new ItemNotFound("Doctor does not exist or you have no permission.");

                await using var transaction = await _unitOfWork.BeginTransactionAsync(); // ✅ Transaction Handling

                try
                {
                    await _unitOfWork.Repository<Education>().DeleteEntityAsync(education);

                    if (await _unitOfWork.CommitAsync() <= 0)
                        throw new BadRequest("Failed to delete education.");

                    await transaction.CommitAsync();
                    return true;
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    throw new Exception("Failed to delete education.", ex);
                }
            });
        }


        public async Task<bool> AddClinicAsync(int doctorId, ClinicRequest request, CancellationToken cancellationToken = default)
        {
            var strategy = _unitOfWork.CreateExecutionStrategy(); // ✅ Use EF Core Execution Strategy

            return await strategy.ExecuteAsync(async () =>
            {
                // 🔹 Step 1: Ensure Doctor Exists
                var doctor = await _unitOfWork.Repository<Doctor>().FirstOrDefaultAsync(x => x.AppUserId == doctorId);
                if (doctor is null)
                    throw new ItemNotFound("Doctor does not exist.");

                // 🔹 Step 2: Check Duplication
                var isDuplicated = await _unitOfWork.Repository<Clinic>().AnyAsync(
                    x => x.Street.Trim().ToLower() == request.Street.Trim().ToLower() &&
                         x.City.Trim().ToLower() == request.City.Trim().ToLower() &&
                         x.Country.Trim().ToLower() == request.Country.Trim().ToLower() &&
                         x.DoctorId == doctor.Id &&
                         x.Name.Trim().ToLower() == request.Name.Trim().ToLower() &&
                         x.PhoneNumber.Trim() == request.PhoneNumber.Trim());

                if (isDuplicated)
                    throw new ItemAlreadyExist("Clinic already exists.");

                string? clinicNewPath = null;
                string? clinicNewLogoPath = null;

                await using var transaction = await _unitOfWork.BeginTransactionAsync(); // ✅ Transaction Handling

                try
                {
                    // 🔹 Step 3: Upload Images (If Provided)
                    if (request.ClinicImage != null)
                    {
                        ValidateImageFile(request.ClinicImage);
                        clinicNewPath = await _imageService.UploadImageOnServer(request.ClinicImage, false, null!, cancellationToken);
                    }

                    if (request.LogoPath != null)
                    {
                        ValidateImageFile(request.LogoPath);
                        clinicNewLogoPath = await _imageService.UploadImageOnServer(request.LogoPath, false, null!, cancellationToken);
                    }

                    // 🔹 Step 4: Create Clinic Entity
                    var clinic = new Clinic
                    {
                        DoctorId = doctor.Id,
                        Name = request.Name.Trim(),
                        Street = request.Street.Trim(),
                        City = request.City.Trim(),
                        Country = request.Country.Trim(),
                        ApartmentOrSuite = request.ApartmentOrSuite?.Trim(),
                        Landmark = request.Landmark?.Trim(),
                        PhoneNumber = request.PhoneNumber.Trim(),
                        LogoPath = clinicNewLogoPath,
                        ClinicImage = clinicNewPath,
                        WorkingTimes = request.WorkingTimes
                            .Select(w => new WorkingTime
                            {
                                Day = w.Day,
                                Periods = w.Periods
                                    .Select(p => new Period
                                    {
                                        StartTime = p.StartTime,
                                        EndTime = p.EndTime,
                                    })
                                    .ToList()
                            })
                            .ToList(),
                    };

                    await _unitOfWork.Repository<Clinic>().AddEntityAsync(clinic);

                    if (await _unitOfWork.CommitAsync() <= 0)
                        throw new BadRequest("Failed to add clinic.");

                    await transaction.CommitAsync(cancellationToken);

                    return true;
                }
                catch (DbUpdateException dbEx)
                {
                    await transaction.RollbackAsync(cancellationToken);

                    // 🔥 Remove uploaded images if transaction fails
                    if (!string.IsNullOrEmpty(clinicNewLogoPath))
                        _ = _imageService.RemoveImage(clinicNewLogoPath); // Fire & Forget

                    if (!string.IsNullOrEmpty(clinicNewPath))
                        _ = _imageService.RemoveImage(clinicNewPath); // Fire & Forget

                    throw new Exception("Database error occurred while adding clinic.", dbEx);
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync(cancellationToken);

                    if (!string.IsNullOrEmpty(clinicNewLogoPath))
                        _ = _imageService.RemoveImage(clinicNewLogoPath); // Fire & Forget

                    if (!string.IsNullOrEmpty(clinicNewPath))
                        _ = _imageService.RemoveImage(clinicNewPath); // Fire & Forget

                    throw new Exception("Failed to add clinic.", ex);
                }
            });
        }

        public async Task<bool> EditClinicAsync(int doctorId, int clinicId, ClinicRequest request, CancellationToken cancellationToken = default)
        {
            var strategy = _unitOfWork.CreateExecutionStrategy(); // ✅ Use EF Core Execution Strategy

            return await strategy.ExecuteAsync(async () =>
            {
                // 🔹 Step 1: Retrieve Clinic with Related Entities using UnitOfWork
                var clinicRepo = _unitOfWork.Repository<Clinic>();
                var clinic = await clinicRepo.FirstOrDefaultAsync(
                    x => x.Id == clinicId && x.Doctor.AppUserId == doctorId,
                     query => query.Include(x => x.WorkingTimes).ThenInclude(x => x.Periods).Include(x => x.Doctor));

                if (clinic == null)
                    throw new ItemNotFound("Clinic does not exist or you have no permission.");

                // 🔹 Step 2: Normalize Input & Check for Duplicates
                var normalizedStreet = request.Street.Trim().ToLower();
                var normalizedCity = request.City.Trim().ToLower();
                var normalizedCountry = request.Country.Trim().ToLower();
                var normalizedName = request.Name.Trim().ToLower();
                var normalizedPhone = request.PhoneNumber.Trim();

                var isDuplicated = await clinicRepo.AnyAsync(
                    x => x.Id != clinicId && // Exclude the current clinic
                         x.Street.Trim().ToLower() == normalizedStreet &&
                         x.City.Trim().ToLower() == normalizedCity &&
                         x.Country.Trim().ToLower() == normalizedCountry &&
                         x.DoctorId == clinic.DoctorId &&
                         x.Name.Trim().ToLower() == normalizedName &&
                         x.PhoneNumber.Trim() == normalizedPhone);

                if (isDuplicated)
                    throw new ItemAlreadyExist("Clinic already exists.");

                string? clinicNewPath = null;
                string? clinicNewLogoPath = null;

                await using var transaction = await _unitOfWork.BeginTransactionAsync(); // ✅ Transaction Handling

                try
                {
                    // 🔹 Step 3: Upload New Images (Only if Provided) and Track Old Paths for Deletion
                    string? oldLogoPath = clinic.LogoPath;
                    string? oldClinicImagePath = clinic.ClinicImage;

                    if (request.ClinicImage != null)
                    {
                        ValidateImageFile(request.ClinicImage);
                        clinicNewPath = await _imageService.UploadImageOnServer(request.ClinicImage, false, oldClinicImagePath, cancellationToken);
                        clinic.ClinicImage = clinicNewPath;
                    }

                    if (request.LogoPath != null)
                    {
                        ValidateImageFile(request.LogoPath);
                        clinicNewLogoPath = await _imageService.UploadImageOnServer(request.LogoPath, false, oldLogoPath, cancellationToken);
                        clinic.LogoPath = clinicNewLogoPath;
                    }

                    // 🔹 Step 4: Update Clinic Fields
                    clinic.Name = request.Name.Trim();
                    clinic.Street = request.Street.Trim();
                    clinic.City = request.City.Trim();
                    clinic.Country = request.Country.Trim();
                    clinic.Landmark = request.Landmark?.Trim();
                    clinic.PhoneNumber = request.PhoneNumber.Trim();
                    clinic.ApartmentOrSuite = request.ApartmentOrSuite?.Trim();

                    // 🔹 Step 5: Soft Delete Existing WorkingTimes and Add New Ones
                    if (request.WorkingTimes.Any())
                    {
                        var workingTimeRepo = _unitOfWork.Repository<WorkingTime>();
                        var periodRepo = _unitOfWork.Repository<Period>();

                        // Soft delete existing WorkingTimes and their Periods
                        foreach (var existingWorkingTime in clinic.WorkingTimes.ToList())
                        {
                            existingWorkingTime.MarkAsDeleted(int.Parse(GetCurrentUserId()));
                            await workingTimeRepo.UpdateEntityAsync(existingWorkingTime);

                            foreach (var period in existingWorkingTime.Periods.ToList())
                            {
                                period.MarkAsDeleted(int.Parse(GetCurrentUserId()));
                                await periodRepo.UpdateEntityAsync(period);
                            }
                        }

                        // Add new WorkingTimes
                        foreach (var w in request.WorkingTimes)
                        {
                            var workingTime = new WorkingTime
                            {
                                Day = w.Day,
                                ClinicId = clinic.Id,
                                Clinic = clinic
                            };

                            foreach (var p in w.Periods)
                            {
                                workingTime.Periods.Add(new Period
                                {
                                    StartTime = p.StartTime,
                                    EndTime = p.EndTime,
                                    WorkingTimeId = workingTime.Id,
                                    WorkingTime = workingTime
                                });
                            }

                            await workingTimeRepo.AddEntityAsync(workingTime);
                            clinic.WorkingTimes.Add(workingTime);
                        }
                    }

                    await clinicRepo.UpdateEntityAsync(clinic);

                    if (await _unitOfWork.CommitAsync(cancellationToken) <= 0)
                        throw new BadRequest("Failed to edit clinic.");

                    await _unitOfWork.CommitTransactionAsync();

                    // 🔹 Step 6: Remove Old Images Only After Successful Update and Commit
                    var deletionTasks = new List<Task>();
                    if (!string.IsNullOrEmpty(oldLogoPath) && clinicNewLogoPath != oldLogoPath)
                    {
                        deletionTasks.Add(_imageService.RemoveImage(oldLogoPath));
                    }

                    if (!string.IsNullOrEmpty(oldClinicImagePath) && clinicNewPath != oldClinicImagePath)
                    {
                        deletionTasks.Add(_imageService.RemoveImage(oldClinicImagePath));
                    }

                    // Wait for all image deletions to complete, but don't block the main response
                    if (deletionTasks.Any())
                    {
                        await Task.WhenAll(deletionTasks);
                    }

                    return true;
                }
                catch (Exception ex)
                {
                    await _unitOfWork.RollbackTransactionAsync();

                    // 🔥 Remove newly uploaded images if the transaction fails
                    if (!string.IsNullOrEmpty(clinicNewLogoPath))
                    {
                        await _imageService.RemoveImage(clinicNewLogoPath);
                    }

                    if (!string.IsNullOrEmpty(clinicNewPath))
                    {
                        await _imageService.RemoveImage(clinicNewPath);
                    }

                    throw new Exception("Failed to edit clinic.", ex);
                }
            });
        }

        // Helper method to get the current user ID
        private string GetCurrentUserId()
        {
            var currentUserIdClaim = _httpContextAccessor?.HttpContext?.User?.FindFirst(ClaimTypes.NameIdentifier);
            return currentUserIdClaim?.Value ?? throw new UnauthorizedAccessException("Current user ID not found.");
        }

        public async Task<bool> RemoveClinicAsync(int doctorId, int clinicId)
        {
            var strategy = _unitOfWork.CreateExecutionStrategy(); // ✅ Use EF Core Execution Strategy

            return await strategy.ExecuteAsync(async () =>
            {
                // 🔹 Step 1: Retrieve the Clinic
                var clinic = await _unitOfWork.Repository<Clinic>()
                    .FirstOrDefaultAsync(x => x.Id == clinicId && x.Doctor.AppUserId == doctorId, query => query.Include(x => x.Doctor));

                if (clinic is null)
                    throw new ItemNotFound("Clinic does not exist or you have no permission.");

                var clinicImagePath = clinic.ClinicImage;
                var clinicLogoPath = clinic.LogoPath;

                await using var transaction = await _unitOfWork.BeginTransactionAsync(); // ✅ Transaction Handling

                try
                {
                    // 🔹 Step 2: Remove the Clinic
                    await _unitOfWork.Repository<Clinic>().DeleteEntityAsync(clinic);

                    if (await _unitOfWork.CommitAsync() <= 0)
                        throw new BadRequest("Failed to remove clinic.");

                    await transaction.CommitAsync();

                    // 🔹 Step 3: Remove Images After Successful Deletion
                    if (!string.IsNullOrEmpty(clinicImagePath))
                        _ = _imageService.RemoveImage(clinicImagePath); // Fire & Forget

                    if (!string.IsNullOrEmpty(clinicLogoPath))
                        _ = _imageService.RemoveImage(clinicLogoPath); // Fire & Forget

                    return true;
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    throw new Exception("Failed to remove clinic.", ex);
                }
            });
        }


        public async Task<PaginatedResponse<ClinicResponse>> GetDoctorClinicsAsync(int doctorId, int pageNumber = 1, int pageSize = 10)
        {
            (var clinics, var totalCount) = await _unitOfWork.Repository<Clinic>()
                .GetAllAsync(c => c.DoctorId == doctorId,
                 query => query.Include(x => x.WorkingTimes).ThenInclude(x => x.Periods).Include(x => x.Doctor),
                 pageNumber,
                 pageSize);

            var totalPages = (int)Math.Ceiling((double)totalCount / pageSize);

            if(clinics?.Count > 0)
            {
                var map = _mapper.Map<List<ClinicResponse>>(clinics.ToList());

                map.ForEach(x => x.ClinicImage = !string.IsNullOrEmpty(x.ClinicImage) ? $"{_baseUrl}{x.ClinicImage}" : $"{_baseUrl}default.jpg");
                map.ForEach(x => x.LogoPath = !string.IsNullOrEmpty(x.LogoPath) ? $"{_baseUrl}{x.LogoPath}" : $"{_baseUrl}default.jpg");

                return new PaginatedResponse<ClinicResponse>
                {
                    Data = map,
                    CurrentPage = pageNumber,
                    PageSize = pageSize,
                    TotalPages = totalPages,
                };
            }

            return new PaginatedResponse<ClinicResponse>
            {
                CurrentPage = pageNumber,
                PageSize = pageSize,
                TotalPages = totalPages,
                Data = new List<ClinicResponse>()
            };
        }

        public async Task<PaginatedResponse<ClinicResponse>> GetDoctorClinicsForDoctorAsync(int doctorId, int pageNumber = 1, int pageSize = 10)
        {
            (var clinics, var totalCount) = await _unitOfWork.Repository<Clinic>()
                .GetAllAsync(c => c.Doctor.AppUserId == doctorId,
                query => query.Include(x => x.WorkingTimes).ThenInclude(x => x.Periods).Include(x => x.Doctor),
                pageNumber,
                pageSize);

            var totalPages = (int)Math.Ceiling((double)totalCount / pageSize);

            if (clinics?.Count > 0)
            {
                var map = _mapper.Map<List<ClinicResponse>>(clinics.ToList());

                map.ForEach(x => x.ClinicImage = !string.IsNullOrEmpty(x.ClinicImage) ? $"{_baseUrl}{x.ClinicImage}" : $"{_baseUrl}default.jpg");
                map.ForEach(x => x.LogoPath = !string.IsNullOrEmpty(x.LogoPath) ? $"{_baseUrl}{x.LogoPath}" : $"{_baseUrl}default.jpg");

                return new PaginatedResponse<ClinicResponse>
                {
                    CurrentPage = pageNumber,
                    PageSize = pageSize,
                    TotalPages = totalPages,
                    Data = map
                };
            }

            return new PaginatedResponse<ClinicResponse>
            {
                CurrentPage = pageNumber,
                PageSize = pageSize,
                TotalPages = totalPages,
                Data = new List<ClinicResponse>()
            };
        }

        public async Task<double> GetAverageRatingAsync(int doctorId)
        {
            return await _unitOfWork.Repository<Review>().GetAverageAsync(r => r.Rate, r => r.DoctorId == doctorId);
        }

        public async Task<long> GetTotalPatientsServedAsync(int doctorId)
        {
            // 🔹 Step 1: Ensure Doctor Exists
            var doctor = await _unitOfWork.Repository<Doctor>().FirstOrDefaultAsync(x => x.AppUserId == doctorId);
            if (doctor is null)
                throw new ItemNotFound("Doctor does not exist.");

            // 🔹 Step 2: Query Efficiently Using Indexed Columns
            return await _unitOfWork.Repository<Appointment>().GetCountWithConditionAsync(
                x => x.DoctorId == doctor.Id && x.AppointmentStatus == AppointmentStatus.Completed);
        }

        public async Task<PaginatedResponse<SpecializationResponse>> GetSpecializations(int doctorId, int pageNumber = 1, int pageSize = 10)
        {
            (var query,var totalCount) = await _unitOfWork.Repository<Specialization>()
                .GetAllAsync(x => x.Doctor.AppUserId == doctorId,
                query => query.Include(x => x.Doctor),
                pageNumber,
                pageSize);

            var totalPages = (int)Math.Ceiling((double)totalCount / pageSize);
          
            if (query is null)
                return new PaginatedResponse<SpecializationResponse>
                {
                    Data = new List<SpecializationResponse>(),
                    PageSize = pageSize,
                    CurrentPage = pageNumber,
                    TotalPages = totalPages
                };

            var response = _mapper.Map<List<SpecializationResponse>>(query);

            return new PaginatedResponse<SpecializationResponse>
            {
                Data = response,
                PageSize = pageSize,
                CurrentPage = pageNumber,
                TotalPages = totalPages
            };
        }

        public async Task<PaginatedResponse<ExperienceResponse>> GetExperiences(int doctorId, int pageNumber = 1, int pageSize = 10)
        {
            (var query,var totalCount) = await _unitOfWork.Repository<Experience>()
                .GetAllAsync(x => x.Doctor.AppUserId == doctorId,
                query => query.Include(x => x.Doctor),
                pageNumber,
                pageSize);

            var totalPages = (int)Math.Ceiling((double)totalCount / pageSize);

            if (query is null)
                return new PaginatedResponse<ExperienceResponse>
                {
                    CurrentPage = pageNumber,
                    PageSize = pageSize,
                    TotalPages = totalPages,
                    Data = new List<ExperienceResponse>()
                };

            var response = _mapper.Map<List<ExperienceResponse>>(query);
            response.ForEach(ex => ex.HospitalLogo = !string.IsNullOrEmpty(ex.HospitalLogo) ? $"{_baseUrl}{ex.HospitalLogo}" : $"{_baseUrl}default.jpg");

            return new PaginatedResponse<ExperienceResponse>
            {
                Data = response,
                CurrentPage = pageNumber,
                PageSize = pageSize,
                TotalPages = totalPages,
            };
        }

        public async Task<PaginatedResponse<AwardResponse>> GetAwards(int doctorId, int pageNumber = 1, int pageSize = 10)
        {
            (var query, var totalCount) = await _unitOfWork.Repository<Award>()
                                                   .GetAllAsync(x => x.Doctor.AppUserId == doctorId, query => query.Include(x => x.Doctor),
                                                   pageNumber,
                                                   pageSize);

            var totalPages = (int)Math.Ceiling((double)totalCount / pageSize);

            if (query is null)
                return new PaginatedResponse<AwardResponse>
                {
                    Data = new List<AwardResponse>(),
                    CurrentPage = pageNumber,
                    PageSize = pageSize,
                    TotalPages = totalPages,
                };

            var response = _mapper.Map<List<AwardResponse>>(query);

            return new PaginatedResponse<AwardResponse>
            {
                Data = response,
                CurrentPage = pageNumber,
                PageSize = pageSize,
                TotalPages = totalPages,
            };
        }

        public async Task<PaginatedResponse<EducationResponse>> GetEducations(int doctorId, int pageNumber = 1, int pageSize = 10)
        {
            (var query, var totalCount) = await _unitOfWork.Repository<Education>()
                            .GetAllAsync(x => x.Doctor.AppUserId == doctorId, query => query.Include(x => x.Doctor),
                            pageNumber,
                            pageSize);

            var totalPages = (int)Math.Ceiling((double)totalCount / pageSize);

            if (query is null)
                return new PaginatedResponse<EducationResponse>()
                {
                    Data = new List<EducationResponse>(),
                    CurrentPage = pageNumber,
                    PageSize = pageSize,
                    TotalPages = totalPages,
                };

            var response = _mapper.Map<List<EducationResponse>>(query);
            response.ForEach(ex => ex.UniversityLogoPath = !string.IsNullOrEmpty(ex.UniversityLogoPath) ? $"{_baseUrl}{ex.UniversityLogoPath}" : $"{_baseUrl}default.jpg");

            return new PaginatedResponse<EducationResponse>
            {
                Data = response,
                CurrentPage = pageNumber,
                PageSize = pageSize,
                TotalPages = totalPages,
            };
        }

        public async Task<DoctorEarningsResponse> GetEarningsReportAsync(int doctorId, DateTimeOffset startDate, DateTimeOffset endDate)
        {
            var query = await _unitOfWork.GetCustomRepository<DoctorRepositoryAsync>().GetEarningsReportAsync(doctorId, startDate, endDate);

            if (query is null)
                return new DoctorEarningsResponse();

            return query;
        }

        private void ValidateImageFile(IFormFile file)
        {
            var allowedExtensions = new[] { ".jpg", ".jpeg", ".png" };
            var maxFileSize = 2 * 1024 * 1024; // 2MB

            var fileExtension = Path.GetExtension(file.FileName).ToLower();
            if (!allowedExtensions.Contains(fileExtension))
                throw new BadRequest($"Invalid file type. Allowed types: {string.Join(", ", allowedExtensions)}");

            if (file.Length > maxFileSize)
                throw new BadRequest("File size exceeds the 2MB limit.");
        }

        private (string Title, string Description, string Organization) NormalizeAwardRequest(AwardRequest request)
        {
            return (
                request.Title?.Trim().ToLower() ?? string.Empty,
                request.Description?.Trim().ToLower() ?? string.Empty,
                request.Organization?.Trim().ToLower() ?? string.Empty
            );
        }

        private async Task<(List<AppointmentDto> response,int totalPages)> GetAppointmentsAsync(int doctorId, bool isUpcoming, int pageNumber = 1, int pageSize = 10)
        {
            var now = DateTimeOffset.UtcNow;

            // 🔹 Fetch Appointments & Join with Patients in One Query
            (var appointments,var totalCount) = await _unitOfWork.GetCustomRepository<AppointmentRepositoryAsync>()
                .GetAllAsync(
                    x => x.Doctor.AppUserId == doctorId && x.AppointmentStatus != AppointmentStatus.CanceledByDoctor &&
                    x.AppointmentStatus != AppointmentStatus.CanceledByPatient 
                    && (isUpcoming ? x.StartDate > now : x.StartDate < now),
                    query => query.Include(x => x.AppointmentType).Include(x => x.Doctor),
                    pageNumber,
                    pageSize
                );

            // 🔹 Extract Patient IDs & Fetch Patients in a Single Call
            var patientIds = appointments.Select(x => x.PatientId).ToHashSet(); // ⚡ Use HashSet for O(1) lookup
            var patients = await _userManager.Users
                .Where(x => patientIds.Contains(x.Id))
                .Select(x => new { x.Id, x.PhoneNumber, x.FirstName, x.LastName })
                .ToListAsync();

            var totalPages = (int)Math.Ceiling((double)totalCount / pageSize);

            // 🔹 Convert Patients List to Dictionary for Fast Lookups
            var patientDict = patients.ToDictionary(x => x.Id, x => x);

            // 🔹 Map Appointments to DTOs Efficiently
            var response = appointments.Select(item => new AppointmentDto
            {
                Id = item.Id.ToString(),
                ConsultationFee = item.AppointmentType.ConsultationFee,
                VisitType = item.AppointmentType.VisitType,
                Duration = item.AppointmentType.Duration,
                StartTime = item.StartDate,
                EndTime = item.EndDate,
                ProblemDescription = item.ProblemDescription,
                Status = item.AppointmentStatus,
                PatientId = patientDict.TryGetValue(item.PatientId, out var user) ? user.Id.ToString() : null!,
                PatientName = user?.FirstName!+" "+user?.LastName, // ✅ No need for empty string checks
                PatientPhone = user?.PhoneNumber
            }).ToList();

            return (response, totalPages);
        }


        // 🔥 **Only removes cache; doesn't generate or set cache keys**
        private async Task RemoveCachedDoctorData(int doctorId)
        {
            string _base = "/api/doctors";

            await _cacheService.RemoveCachedResponseAsync(_base);
            await _cacheService.RemoveCachedResponseAsync($"{_base}/{doctorId}");
            await _cacheService.RemoveCachedResponseAsync($"{_base}/top-ten");
            await _cacheService.RemoveCachedResponseAsync($"{_base}/{doctorId}/available-timeslots");
            await _cacheService.RemoveCachedResponseAsync($"{_base}/{doctorId}/appointment-types");
        }

        /// <summary>
        /// Maps a Doctor entity to a DoctorResponse DTO efficiently.
        /// </summary>
        private DoctorResponse MapDoctorResponse(
            Doctor doctor,
            Dictionary<int, (string FullName, string ImagePath)> userDetails)
        {
            var doctorFullName = userDetails.TryGetValue(doctor.AppUserId, out var doctorInfo)
                ? doctorInfo.FullName
                : "Unknown";

            var doctorImagePath = userDetails.TryGetValue(doctor.AppUserId, out var doctorImg)
                ? $"{_baseUrl}{doctorImg.ImagePath}"
                : $"{_baseUrl}default.jpg";

            return new DoctorResponse
            {
                Id = doctor.Id.ToString(),

                FullName = doctorFullName,
                ImagePath = doctorImagePath,
                NumberOfReviews = doctor.NumberOfReviews,
                TotalYearsOfExperience = doctor.TotalYearsOfExperience,
                Rating = doctor.Reviews.Any() ? doctor.Reviews.Average(r => r.Rate) : 0,
                Specializations = doctor.Specializations.Select(x => new SpecializationResponse
                {
                    Id = x.Id.ToString(),
                    Name = x.Name,
                    Category = x.Category,
                }).ToList(),
            };

        }

        private DoctorDetail MapDoctorDetailResponse(
           Doctor doctor,
           Dictionary<int, (string FullName, string ImagePath)> userDetails,
           long count)
        {

            var doctorFullName = userDetails.TryGetValue(doctor.AppUserId, out var doctorInfo)
                ? doctorInfo.FullName
                : "Unknown";

            var doctorImagePath = userDetails.TryGetValue(doctor.AppUserId, out var doctorImg)
                ? $"{_baseUrl}{doctorImg.ImagePath}"
                : $"{_baseUrl}default.jpg";

            return new DoctorDetail
            {
                Id = doctor.Id.ToString(),
                FullName = doctorFullName,
                ImagePath = doctorImagePath,
                NumberOfReviews = doctor.NumberOfReviews,
                TotalYearsOfExperience = doctor.TotalYearsOfExperience,
                AboutMe = doctor.AboutMe,
                TotalPatinets = count,
                Rating = doctor.Reviews.Any() ? doctor.Reviews.Average(r => r.Rate) : 0,
                AppointmentTypes = doctor.AppointmentTypes.Select(x => new AppointmentTypeResponse
                {
                    ConsultationFee = x.ConsultationFee,
                    Duration = x.Duration,
                    Id = x.Id.ToString(),
                    VisitType = x.VisitType,
                })
                .ToList(),
                Experiences = doctor.Experiences.Select(e=> new ExperienceResponse
                {
                    CurrentlyWorkingHere = e.CurrentlyWorkingHere,
                    EmploymentType = e.EmploymentType,
                    StartDate = e.StartDate,
                    EndDate = e.EndDate,
                    HospitalName = e.HospitalName,
                    JobDescription = e.JobDescription,
                    Title = e.Title,
                    Location = e.Location,
                    Id = e.Id.ToString(),
                    HospitalLogo = $"{_baseUrl}{e.HospitalLogo}"
                })
                .ToList(),
                Awards = doctor.Awards.Select(a => new AwardResponse
                {
                    Id = a.Id.ToString(),
                    DateReceived = a.DateReceived,
                    Description = a.Description,
                    Organization = a.Organization,
                    Title = a.Title,
                })
                .ToList(),
                Educations = doctor.Educations.Select(e => new EducationResponse
                {
                    AdditionalNotes = e.AdditionalNotes,
                    CurrentlyStudying = e.CurrentlyStudying,
                    Degree = e.Degree,
                    EndDate = e.EndDate,
                    Id = e.Id.ToString(),
                    Location = e.Location,
                    Major = e.Major,
                    StartDate = e.StartDate,
                    UniversityLogoPath = $"{_baseUrl}{e.UniversityLogoPath}",
                    UniversityName = e.UniversityName
                })
                .ToList(),
                Clinics = doctor.Clinics.Select(x => new ClinicResponse
                {
                    Id = x.Id.ToString(),
                    City = x.City,
                    Country = x.Country,
                    Street = x.Street,
                    ApartmentOrSuite = x.ApartmentOrSuite,
                    ClinicImage = $"{_baseUrl}{x.ClinicImage}",
                    Landmark = x.Landmark,
                    LogoPath = $"{_baseUrl}{x.LogoPath}",
                    Name = x.Name,
                    PhoneNumber = x.PhoneNumber,
                    WorkingTimes = x.WorkingTimes.Select(w => new WorkingTimeResponse
                    {
                        Id = w.Id.ToString(),
                        Day = w.Day,
                        IsAvailable = w.IsAvailable,
                        Periods = w.Periods.Select(p => new PeriodResponse
                        {
                            Id = p.Id.ToString(),
                            IsAvailable = p.IsAvailable,
                            EndTime = p.EndTime,
                            StartTime = p.StartTime,
                        }).ToList(),
                    }).ToList()
                }).ToList(),
                Specializations = doctor.Specializations.Select(x => new SpecializationResponse
                {
                    Id = x.Id.ToString(),
                    Name = x.Name,
                    Category = x.Category,
                }).ToList(),
                Reviews = doctor.Reviews.Select(review =>
                {
                    var reviewerFullName = userDetails.TryGetValue(review.AppUserId, out var reviewerInfo)
                        ? reviewerInfo.FullName
                        : "Unknown";

                    var reviewerImagePath = userDetails.TryGetValue(review.AppUserId, out var reviewerImg)
                        ? $"{_baseUrl}{reviewerImg.ImagePath}"
                        : $"{_baseUrl}default.jpg";

                    return new ReviewResponse
                    {
                        Id = review.Id.ToString(),
                        FullName = reviewerFullName,
                        ImagePath = reviewerImagePath,
                        Comment = review.Comment,
                        Rate = review.Rate,
                    };
                }).ToList(),
            };
        }

        public async Task<PaginatedResponse<AppointmentPatinetDetail>> GetToDayAppointmentsAsync(int doctorId, int pageNumber = 1, int pageSize = 10)
        {

            // First, get today's date range (from start of day to end of day)
            var today = DateTime.Today;
            var tomorrow = today.AddDays(1);

            // Get appointments for today
            var (appointments, totalCount) = await _unitOfWork.GetCustomRepository<AppointmentRepositoryAsync>()
                .GetAllAsync(
                    x => x.Doctor.AppUserId == doctorId &&
                         x.AppointmentStatus != AppointmentStatus.CanceledByDoctor &&
                         x.AppointmentStatus != AppointmentStatus.CanceledByPatient &&
                         x.StartDate >= today &&
                         x.EndDate < tomorrow,
                    query => query
                        .Include(x => x.AppointmentType)
                        .Include(x => x.Doctor)
                        .OrderBy(x => x.StartDate),  // Added ordering for better UX
                    pageNumber,
                    pageSize
                );

            // 🔹 Extract Patient IDs & Fetch Patients in a Single Call
            var patientIds = appointments.Select(x => x.PatientId).ToHashSet(); // ⚡ Use HashSet for O(1) lookup
            var patients = await _userManager.Users
                .Where(x => patientIds.Contains(x.Id))
                .Select(x => new { x.Id, x.PhoneNumber, x.FirstName, x.LastName, x.ImagePath })
                .ToListAsync();

            var totalPages = (int)Math.Ceiling((double)totalCount / pageSize);

            // 🔹 Convert Patients List to Dictionary for Fast Lookups
            var patientDict = patients.ToDictionary(x => x.Id, x => x);

            // 🔹 Map Appointments to DTOs Efficiently
            var response = appointments.Select(item => new AppointmentPatinetDetail
            {
                Id = item.Id.ToString(),
                ConsultationFee = item.AppointmentType.ConsultationFee,
                VisitType = item.AppointmentType.VisitType,
                Duration = item.AppointmentType.Duration,
                StartTime = item.StartDate,
                EndTime = item.EndDate,
                ProblemDescription = item.ProblemDescription,
                Status = item.AppointmentStatus,
                PatientId = patientDict.TryGetValue(item.PatientId, out var user) ? user.Id.ToString() : null!,
                PatientName = user?.FirstName! + " " + user?.LastName, // ✅ No need for empty string checks
                PatientPhone = user?.PhoneNumber,
                image = $"{_baseUrl}{user?.ImagePath}"

            }).ToList();

            return new PaginatedResponse<AppointmentPatinetDetail>
            {
                Data = response,
                CurrentPage = pageNumber,
                PageSize = pageSize,
                TotalPages = totalPages
            };
        }
    }

}
