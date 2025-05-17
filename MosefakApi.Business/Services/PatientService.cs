
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

namespace MosefakApi.Business.Services
{
    public class PatientService : IPatientService
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly IImageService _imageService;
        private readonly ICacheService _cacheService;
        private readonly ILoggerService _loggerService;
        private readonly IMapper _mapper;
        private const string CacheKey_PatientProfile = "/api/Patients/profile"; // Centralized cache key

        public PatientService(UserManager<AppUser> userManager, IImageService imageService,
                              ICacheService cacheService, ILoggerService loggerService, IMapper mapper)
        {
            _userManager = userManager;
            _imageService = imageService;
            _cacheService = cacheService;
            _loggerService = loggerService;
            _mapper = mapper;
        }

        public async Task<UserProfileResponse?> PatientProfile(int userIdFromClaims)
        {
            var user = await _userManager.Users
                                         .AsNoTracking() // Optimization for read-only operation
                                         .Include(x => x.Address)
                                         .FirstOrDefaultAsync(x => x.Id == userIdFromClaims);

            if (user is null)
            {
                _loggerService.LogWarning($"Patient profile not found for ID: {userIdFromClaims}");
                return null;
            }

            return _mapper.Map<UserProfileResponse>(user);
        }

        public async Task<UserProfileResponse> UpdatePatientProfile(int userIdFromClaims,
                                                                    UpdatePatientProfileRequest request,
                                                                    CancellationToken cancellationToken = default)
        {
            var user = await CheckPatientExist(userIdFromClaims);

            // Update user properties only if they have changed
            user.FirstName = request.FirstName ?? user.FirstName;
            user.LastName = request.LastName ?? user.LastName;
            user.PhoneNumber = request.PhoneNumber ?? user.PhoneNumber;
            user.DateOfBirth = request.DateOfBirth ?? user.DateOfBirth;
            user.Gender = request.Gender ?? user.Gender;

            if (request.Address != null)
            {
                user.Address ??= new Address(); // Ensure address object exists
                user.Address.Country = request.Address.Country;
                user.Address.City = request.Address.City;
                user.Address.Street = request.Address.Street;
            }

            var result = await _userManager.UpdateAsync(user);

            if (!result.Succeeded)
            {
                var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                _loggerService.LogError($"Failed to update profile for ID {userIdFromClaims}: {errors}");
                throw new BadRequest($"Profile update failed: {errors}");
            }

            // Invalidate cache since profile has changed
            await _cacheService.RemoveCachedResponseAsync(CacheKey_PatientProfile);

            return _mapper.Map<UserProfileResponse>(user);
        }

        public async Task<bool> UploadProfileImageAsync(int patientId, IFormFile imageFile, CancellationToken cancellationToken = default)
        {
            if (imageFile == null || imageFile.Length == 0)
                throw new BadRequest("Invalid image file.");

            // 🔹 Validate Image File (Size & Format)
            ValidateImageFile(imageFile);

            var patient = await CheckPatientExist(patientId);

            string oldPath = patient.ImagePath ?? string.Empty;
            string? newPath = null;

            try
            {
                if (imageFile != null)
                {
                    newPath = await _imageService.UploadImageOnServer(imageFile, false, oldPath, cancellationToken);
                    patient.ImagePath = newPath;
                }

                var result = await _userManager.UpdateAsync(patient);

                if (!result.Succeeded)
                {
                    var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                    _loggerService.LogError($"Failed to update profile image for ID {patientId}: {errors}");

                    // Rollback: remove newly uploaded image if user update fails
                    if (!string.IsNullOrEmpty(newPath))
                        await _imageService.RemoveImage($"{newPath}");

                    throw new BadRequest($"Profile image update failed: {errors}");
                }

                // If image update successful, remove the old image safely
                if (!string.IsNullOrEmpty(oldPath))
                    await _imageService.RemoveImage($"{oldPath}");

                // Invalidate profile cache since image changed
                await _cacheService.RemoveCachedResponseAsync(CacheKey_PatientProfile);

                return true;
            }
            catch (Exception ex)
            {
                _loggerService.LogError($"Unexpected error in UploadProfileImageAsync for ID {patientId}: {ex.Message}");
                throw new Exception("An unexpected error occurred while updating profile image.", ex);
            }
        }

        public async Task<PaginatedResponse<PatientViewModel>> GetPatientsAsync(int pageNumber = 1, int pageSize = 10)
        {
            var patinets = _userManager.Users.AsNoTracking()
                                             .Where(x => x.UserType == UserType.Patient)
                                             .AsQueryable();

            int totalCount = await patinets.CountAsync();

            pageNumber = Math.Max(1, pageNumber);

            var items = await patinets
             .Skip((pageNumber - 1) * pageSize)
             .Take(pageSize)
             .ToListAsync();

            var response = new PaginatedResponse<PatientViewModel>()
            {
                CurrentPage = pageNumber,
                PageSize = pageSize,
                TotalPages = (int)Math.Ceiling((double)totalCount / pageSize),
            };

            if (items is null)
            {
                response.Data = [];
                return response;
            }

            var itemsVm = _mapper.Map<List<PatientViewModel>>(items);

            response.Data = itemsVm;

            return response;
        }
        private async Task<AppUser> CheckPatientExist(int patientId)
        {
            var user = await _userManager.FindByIdAsync(patientId.ToString());

            if (user is null)
            {
                _loggerService.LogWarning($"Patient not found for ID {patientId}");
                throw new ItemNotFound("User does not exist.");
            }

            return user;
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

       
    }
}
