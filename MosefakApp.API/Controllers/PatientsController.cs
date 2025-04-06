namespace MosefakApp.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Cached(duration: 600)] // 10 minutes
    [EnableRateLimiting(policyName: RateLimiterType.Concurrency)]
    public class PatientsController : ControllerBase
    {
        private readonly IPatientService _patientService;

        public PatientsController(IPatientService patientService)
        {
            _patientService = patientService;
        }

        [HttpGet("profile")]
        [RequiredPermission(Permissions.Patients.ViewProfile)]
        public async Task<ActionResult<UserProfileResponse>> PatientProfile()
        {
            int userId = User.GetUserId();

            var response = await _patientService.PatientProfile(userId);

            return Ok(response);
        }

        [HttpPut]
        [RequiredPermission(Permissions.Patients.EditProfile)]
        public async Task<ActionResult<UserProfileResponse>> UpdatePatientProfile([FromBody] UpdatePatientProfileRequest request)
        {
            int userId = User.GetUserId();

            var response = await _patientService.UpdatePatientProfile(userId, request);

            return Ok(response);
        }

        [HttpPost("profile/image")]
        [RequiredPermission(Permissions.Patients.UploadProfileImage)]
        public async Task<ActionResult<bool>> UploadProfileImageAsync(IFormFile imageFile, CancellationToken cancellationToken = default)
        {
            int patientId = User.GetUserId();

            var response = await _patientService.UploadProfileImageAsync(patientId, imageFile, cancellationToken);

            return Ok(response);
        }
    }
}
