namespace MosefakApp.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
  //  [Cached(duration: 600)] // 600/60 = 10 minutes
    [EnableRateLimiting(policyName: RateLimiterType.Concurrency)]
    public class UsersController : ControllerBase
    {
        private readonly IUserService _userService;
        private readonly IIdProtectorService _idProtectorService;

        public UsersController(IUserService userService, IIdProtectorService idProtectorService)
        {
            _userService = userService;
            _idProtectorService = idProtectorService;
        }

        [HttpGet]
        [RequiredPermission(Permissions.Users.View)]
        public async Task<ActionResult<PaginatedResponse<UserResponse>>> GetAllUsers(
        [FromQuery] bool includeDeleted = false,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10)
        {
            // Fetch paginated users
            var (users, totalPages) = await _userService.GetUsersAsync(includeDeleted, pageNumber, pageSize);

            if (users == null || !users.Any())
            {
                return Ok(new PaginatedResponse<UserResponse>
                {
                    Data = new List<UserResponse>(),
                    TotalPages = totalPages,
                    CurrentPage = pageNumber,
                    PageSize = pageSize
                });
            }

            // Protect sensitive IDs
            users.ForEach(user => user.Id = ProtectId(user.Id));

            return Ok(new PaginatedResponse<UserResponse>
            {
                Data = users,
                TotalPages = totalPages,
                CurrentPage = pageNumber,
                PageSize = pageSize
            });
        }


        // ✅ Get User By Encrypted ID
        [HttpGet("{id}")]
        [RequiredPermission(Permissions.Users.ViewById)]
        public async Task<ActionResult<UserResponse>> GetById(string id)
        {
            var unprotectedId = UnprotectId(id);
            if (unprotectedId == null) return BadRequest("Invalid user ID");

            var user = await _userService.GetUserByIdAsync(unprotectedId.Value);

            user.Id = ProtectId(user.Id);
            return Ok(user);
        }

        // ✅ Add User
        [HttpPost]
        [RequiredPermission(Permissions.Users.Create)]
        public async Task<ActionResult<UserResponse>> AddUser(UserRequest user)
        {
            var newUser = await _userService.CreateUserAsync(user);
            newUser.Id = ProtectId(newUser.Id);
            return CreatedAtAction(nameof(GetById), new { id = newUser.Id }, newUser);
        }

        // ✅ Update User
        [HttpPut("{id}")]
        [RequiredPermission(Permissions.Users.Edit)]
        public async Task<ActionResult<UserResponse>> UpdateUser(string id, UserRequest user)
        {
            var unprotectedId = UnprotectId(id);

            if (unprotectedId == null) 
                return BadRequest("Invalid user ID");

            var updatedUser = await _userService.UpdateUserAsync(unprotectedId.Value, user);
            updatedUser.Id = ProtectId(updatedUser.Id);

            return Ok(updatedUser);
        }

        // ✅ Delete User
        [HttpDelete("{id}")]
        [RequiredPermission(Permissions.Users.Delete)]
        public async Task<IActionResult> DeleteUser(string id)
        {
            var unprotectedId = UnprotectId(id);

            if (unprotectedId == null) 
                return BadRequest("Invalid user ID");

            await _userService.DeleteUserAsync(unprotectedId.Value);
            return NoContent();
        }

        // ✅ Unlock User
        [HttpPut("unlock/{id}")]
        [RequiredPermission(Permissions.Users.UnLock)]
        public async Task<IActionResult> UnlockUser(string id)
        {
            var unprotectedId = UnprotectId(id);

            if (unprotectedId == null) 
                return BadRequest("Invalid user ID");

            await _userService.UnLock(unprotectedId.Value);
            return Ok();
        }

        [HttpPost("update-fcm-token")]
        [Authorize]
        public async Task<IActionResult> UpdateFcmToken([FromBody] UpdateFcmTokenDto model)
        {
            var userId = User.GetUserId();

            var query = await _userService.UpdateFcmToken(userId, model);

            return Ok(query);
        }


        // 🔥 Reusable Helper Method for ID Protection
        private int? UnprotectId(string protectedId) => _idProtectorService.UnProtect(protectedId);

        private string ProtectId(string id) => _idProtectorService.Protect(int.Parse(id));
    }

}
