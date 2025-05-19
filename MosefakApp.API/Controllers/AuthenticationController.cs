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

        [HttpPost("resend-confirmation-email")]
        [AllowAnonymousPermission]
        public async Task<IActionResult> ResendConfirmationEmail(ResendConfirmationEmailRequest request)
        {
            await _authenticationService.ResendConfirmationEmail(request);

            return Ok();
        }


        [HttpPost("forget-password")]
        [AllowAnonymousPermission] // Or your preferred authorization
        public async Task<IActionResult> ForgetPassword([FromBody] ForgetPasswordRequest request)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            try
            {
                await _authenticationService.ForgetPasswordAsync(request);
                return Ok(new { message = "If your email address is registered, you will receive a password reset code shortly." });
            }
            catch (BadHttpRequestException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            // Catch other exceptions as needed
        }

        [HttpPost("verify-reset-code")]
        [AllowAnonymousPermission]
        public async Task<IActionResult> VerifyResetCode([FromBody] VerifyResetCodeRequest request)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            try
            {
                await _authenticationService.VerifyResetCodeAsync(request);
                // Upon success, the client should be redirected/allowed to go to the "enter new password" form.
                return Ok(new { message = "Verification successful. You can now set a new password." });
            }
            catch (BadHttpRequestException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            // Catch other exceptions
        }



        [HttpPost("reset-password")]
        [AllowAnonymousPermission]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordRequest request) // DTO is updated (no 4-digit code)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            try
            {
                await _authenticationService.ResetPasswordAsync(request);
                return Ok(new { message = "Your password has been reset successfully." });
            }
            catch (BadHttpRequestException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            // Catch other exceptions
        }

        // 🔥 Reusable Helper Method for ID Protection
        private int? UnprotectId(string protectedId) => _idProtectorService.UnProtect(protectedId);

        private string ProtectId(string id) => _idProtectorService.Protect(int.Parse(id));
    }
}
