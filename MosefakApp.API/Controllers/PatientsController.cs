using static MosefakApp.Infrastructure.constants.Permissions;

namespace MosefakApp.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
 //   [Cached(duration: 600)] // 10 minutes
    [EnableRateLimiting(policyName: RateLimiterType.Concurrency)]
    public class PatientsController : ControllerBase
    {
        private readonly IPatientService _patientService;
        private readonly IIdProtectorService _idProtectorService;


        public PatientsController(IPatientService patientService, IIdProtectorService idProtectorService)
        {
            _patientService = patientService;
            _idProtectorService = idProtectorService;
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

        [HttpGet]
        [RequiredPermission(Permissions.Patients.View)]
        public async Task<PaginatedResponse<PatientViewModel>> GetPatientsAsync(int pageNumber = 1, int pageSize = 10)
        {
            var patients = await _patientService.GetPatientsAsync(pageNumber, pageSize);

            if(patients is not null)
            {
                foreach (var item in patients.Data)
                {
                    item.Id = ProtectId(item.Id);
                }
            }

            return patients;
        }

        private int? UnprotectId(string protectedId) => _idProtectorService.UnProtect(protectedId);

        private string ProtectId(string id) => _idProtectorService.Protect(int.Parse(id));
    }
}
