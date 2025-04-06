namespace MosefakApp.API.Controllers
{
    [Route("api/doctor/appointment-types")]
    [ApiController]
    public class AppointmentTypesController : ControllerBase
    {
        private readonly IAppointmentTypeService _appointmentTypeService;
        private readonly IIdProtectorService _idProtectorService;
        public AppointmentTypesController(IAppointmentTypeService appointmentTypeService, IIdProtectorService idProtectorService)
        {
            _appointmentTypeService = appointmentTypeService;
            _idProtectorService = idProtectorService;
        }

        // ✅ Get appointment types for a doctor
        [HttpGet]
        [RequiredPermission(Permissions.AppointmentTypes.View)]
        public async Task<ActionResult<PaginatedResponse<AppointmentTypeResponse>>> GetAppointmentTypes(
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10)
        {
            int doctorId = User.GetUserId();

            // Fetch paginated appointment types
            var (appointmentTypes, totalPages) = await _appointmentTypeService.GetAppointmentTypes(
                doctorId, pageNumber, pageSize);

            if (appointmentTypes == null || !appointmentTypes.Any())
            {
                return Ok(new PaginatedResponse<AppointmentTypeResponse>
                {
                    Data = new List<AppointmentTypeResponse>(),
                    TotalPages = totalPages,
                    CurrentPage = pageNumber,
                    PageSize = pageSize
                });
            }

            // Protect sensitive IDs
            appointmentTypes.ForEach(a => a.Id = ProtectId(a.Id));

            // Return a paginated response
            return Ok(new PaginatedResponse<AppointmentTypeResponse>
            {
                Data = appointmentTypes,
                TotalPages = totalPages,
                CurrentPage = pageNumber,
                PageSize = pageSize
            });
        }


        [HttpPost]
        [RequiredPermission(Permissions.AppointmentTypes.Add)]
        public async Task<ActionResult<bool>> AddAppointmentType(AppointmentTypeRequest request)
        {
            var doctorId = User.GetUserId();

            var result = await _appointmentTypeService.AddAppointmentType(doctorId, request);
            return result ? Ok(result) : BadRequest("Failed to add appointment type.");
        }

        [HttpPut("{protectedId}")]
        [RequiredPermission(Permissions.AppointmentTypes.Edit)]
        public async Task<ActionResult<bool>> EditAppointmentType(string protectedId, AppointmentTypeRequest request)
        {
            var unprotectedId = UnprotectId(protectedId);
            if (unprotectedId == null)
                return BadRequest("Invalid appointment type ID.");

            var result = await _appointmentTypeService.EditAppointmentType(unprotectedId.Value, request);
            return result ? Ok(result) : BadRequest("Failed to edit appointment type.");
        }

        [HttpDelete("{protectedId}")]
        [RequiredPermission(Permissions.AppointmentTypes.Delete)]
        public async Task<ActionResult<bool>> DeleteAppointmentType(string protectedId, CancellationToken cancellationToken)
        {
            var unprotectedId = UnprotectId(protectedId);
            if (unprotectedId == null)
                return BadRequest("Invalid appointment type ID.");

            var result = await _appointmentTypeService.DeleteAppointmentType(unprotectedId.Value);
            return result ? Ok(result) : BadRequest("Failed to delete appointment type.");
        }

        // 🔥 Utility Methods for ID Protection
        private string ProtectId(string id) => _idProtectorService.Protect(int.Parse(id));
        private int? UnprotectId(string id) => _idProtectorService.UnProtect(id);
    }
}
