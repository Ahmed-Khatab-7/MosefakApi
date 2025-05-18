namespace MosefakApp.API.Controllers
{
    [EnableRateLimiting(policyName: RateLimiterType.Concurrency)]
    public class AuthenticationController : ApiBaseController
    {
        private readonly IAuthenticationService _authenticationService;
        private readonly IIdProtectorService _idProtectorService;

        public AuthenticationController(IAuthenticationService authenticationService, IIdProtectorService idProtectorService)
        {
            _authenticationService = authenticationService;
            _idProtectorService = idProtectorService;
        }

        [HttpPost("Login")]
        [AllowAnonymousPermission]
        [ProducesResponseType(type: typeof(LoginResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(type: typeof(CustomResponse), StatusCodes.Status400BadRequest)]
        public async Task<LoginResponse> Login(LoginRequest loginRequest)
        {
            return await _authenticationService.Login(loginRequest);
        }

        [HttpPost("Register")]
        [AllowAnonymousPermission]
        public async Task<IActionResult> Register(RegisterRequest registerRequest)
        {
            await _authenticationService.Register(registerRequest);

            return Ok();
        }

        [HttpPost("confirm-email")]
        [AllowAnonymousPermission]
        public async Task<IActionResult> ConfirmEmailAsync([FromBody] ConfirmEmailRequest request) // Changed from [FromQuery] to [FromBody]
        {
            var userId = await _authenticationService.ConfirmEmailAsync(request);

            var protectedUserId = ProtectId(userId.ToString());

            return Ok(protectedUserId);
        }

        [HttpPost("reset-password")]
        [AllowAnonymousPermission]
        public async Task<IActionResult> ResetPasswordAsync([FromBody] ResetPasswordRequest request) // Changed to [FromBody] if not already
        {
            await _authenticationService.ResetPasswordAsync(request);

            return Ok();
        }

        [HttpPost("resend-confirmation-email")]
        [AllowAnonymousPermission]
        public async Task<IActionResult> ResendConfirmationEmail(ResendConfirmationEmailRequest request)
        {
            await _authenticationService.ResendConfirmationEmail(request);

            return Ok();
        }


        [HttpPost("forgot-password")]
        [AllowAnonymousPermission]
        public async Task<IActionResult> ForgetPasswordAsync(ForgetPasswordRequest request)
        {
            await _authenticationService.ForgetPasswordAsync(request);

            return Ok();
        }

        // 🔥 Reusable Helper Method for ID Protection
        private int? UnprotectId(string protectedId) => _idProtectorService.UnProtect(protectedId);

        private string ProtectId(string id) => _idProtectorService.Protect(int.Parse(id));
    }
}
