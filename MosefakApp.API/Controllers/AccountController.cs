using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using System.Security.Claims;

namespace MosefakApp.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class accountController : ControllerBase
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly SignInManager<AppUser> _signInManager;
        private readonly IJwtProvider _jwtProvider;

        public accountController(UserManager<AppUser> userManager, SignInManager<AppUser> signInManager, IJwtProvider jwtProvider)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _jwtProvider = jwtProvider;
        }

        // GET /api/account/google-login
        [HttpGet("google-login")]
        [AllowAnonymousPermission]
        public IActionResult GoogleLogin(string returnUrl = "/")
        {
            var redirectUrl = Url.Action(nameof(GoogleLoginCallback), "account", new { returnUrl });
            var properties = new AuthenticationProperties { RedirectUri = redirectUrl };
            return Challenge(properties, "Google");
        }

        // GET /api/account/google-callback
        [HttpGet("google-callback")]
        [AllowAnonymousPermission]
        public async Task<IActionResult> GoogleLoginCallback(string returnUrl = "/")
        {
            // Ensure the return URL is local to prevent open redirect vulnerabilities
            if (!Url.IsLocalUrl(returnUrl))
            {
                returnUrl = "/";
            }

            // Get the external login information from Google
            var externalLoginInfo = await _signInManager.GetExternalLoginInfoAsync();
            if (externalLoginInfo == null)
            {
                return BadRequest(new { error = "External authentication failed." });
            }

            // Extract the email from the Google login
            var email = externalLoginInfo.Principal.FindFirstValue(ClaimTypes.Email);
            if (string.IsNullOrEmpty(email))
            {
                return BadRequest(new { error = "Email not found in Google account." });
            }

            // Check if the user already exists
            var user = await _userManager.FindByEmailAsync(email);
            if (user == null)
            {
                // Create a new user if it doesn't exist
                user = new AppUser
                {
                    UserName = email,
                    Email = email
                };
                var createResult = await _userManager.CreateAsync(user);
                if (!createResult.Succeeded)
                {
                    return BadRequest(new { error = "Failed to create user." });
                }
            }

            // Generate JWT token for the user
            var roles = await _userManager.GetRolesAsync(user);
            var token = _jwtProvider.GenerateToken(user, roles);

            // Return the generated token as JSON response
            return Ok(new { token });
        }
    }
}
