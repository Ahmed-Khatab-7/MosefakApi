namespace MosefakApp.API.Controllers
{
    [EnableRateLimiting(policyName: RateLimiterType.Concurrency)]
    public class AuthenticationController : ApiBaseController
    {
        private readonly IAuthenticationService _authenticationService;
        public AuthenticationController(IAuthenticationService authenticationService)
        {
            _authenticationService = authenticationService;
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
        public async Task<IActionResult> ConfirmEmailAsync([FromQuery] ConfirmEmailRequest request)
        {
            await _authenticationService.ConfirmEmailAsync(request);

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

        [HttpPost("reset-password")]
        [AllowAnonymousPermission]
        public async Task<IActionResult> ResetPasswordAsync(ResetPasswordRequest request)
        {
            await _authenticationService.ResetPasswordAsync(request);

            return Ok();
        }

    }
}
