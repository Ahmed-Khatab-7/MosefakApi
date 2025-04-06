namespace MosefakApp.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Cached(duration: 600)]
    [EnableRateLimiting(policyName: RateLimiterType.Concurrency)]
    public class RolesController : ControllerBase
    {
        private readonly IRoleService _roleService;
        private readonly IIdProtectorService _idProtectorService;

        public RolesController(IRoleService roleService, IIdProtectorService idProtectorService)
        {
            _roleService = roleService;
            _idProtectorService = idProtectorService;
        }

        [HttpGet]
        [RequiredPermission(Permissions.Roles.View)]
        public async Task<ActionResult<PaginatedResponse<RoleResponse>>> Get(
        [FromQuery] bool IncludeDeleted = false,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10)
        {
            var (roles, totalPages) = await _roleService.GetRolesWithPermissionsAsync(IncludeDeleted, pageNumber, pageSize);

            if (!roles.Any()) // No need for `roles is null`
            {
                return Ok(new PaginatedResponse<RoleResponse>
                {
                    Data = new List<RoleResponse>(),
                    CurrentPage = pageNumber,
                    PageSize = pageSize,
                    TotalPages = totalPages
                });
            }

            // Protect sensitive IDs
            roles.ToList().ForEach(r => r.Id = ProtectId(r.Id));

            return Ok(new PaginatedResponse<RoleResponse>
            {
                Data = roles,
                TotalPages = totalPages,
                PageSize = pageSize,
                CurrentPage = pageNumber
            });
        }


        [HttpGet("{id}")]
        [RequiredPermission(Permissions.Roles.ViewById)]
        public async Task<ActionResult<RoleResponse>> GetById(string id)
        {
            var unprotectedId = UnprotectId(id);

            if (unprotectedId == null)
                return BadRequest("Invalid ID");

            var response = await _roleService.GetRoleByIdWithPermissionsAsync(unprotectedId.Value);
            response.Id = ProtectId(response.Id);

            return Ok(response);
        }

        [HttpPost]
        [RequiredPermission(Permissions.Roles.Create)]
        public async Task<RoleResponse> Add(RoleRequest request)
        {
            return await _roleService.AddRoleAsync(request);
        }

        [HttpPut("{id}")]
        [RequiredPermission(Permissions.Roles.Edit)]
        public async Task<ActionResult<RoleResponse>> Edit(string id, RoleRequest request)
        {
            var unprotectedId = UnprotectId(id);

            if (unprotectedId == null)
                return BadRequest("Invalid ID");

            var response = await _roleService.EditRoleAsync(unprotectedId.Value, request);
            response.Id = ProtectId(response.Id);

            return Ok(response);
        }

        [HttpDelete("{id}")]
        [RequiredPermission(Permissions.Roles.Delete)]
        public async Task<ActionResult> Delete(string id)
        {
            var unprotectedId = UnprotectId(id);

            if (unprotectedId == null)
                return BadRequest("Invalid ID");

            await _roleService.DeleteRoleAsync(unprotectedId.Value);

            return NoContent();
        }

        [HttpPut("assign-permission/{roleId}")]
        [RequiredPermission(Permissions.Roles.AssignPermissionToRole)]
        public async Task<ActionResult<bool>> AssignPermissionToRoleAsync(string roleId, AssignPermissionsRequest request)
        {
            var query = await _roleService.AssignPermissionToRoleAsync(roleId, request);

            return Ok(query);
        }

        // 🔥 Reusable Helper Method for ID Protection
        private int? UnprotectId(string protectedId) => _idProtectorService.UnProtect(protectedId);

        private string ProtectId(string id) => _idProtectorService.Protect(int.Parse(id));
    }
}
