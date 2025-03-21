using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using System.Security.Claims;

namespace MosefakApp.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AccountController : ControllerBase
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly SignInManager<AppUser> _signInManager;

        public AccountController(UserManager<AppUser> userManager, SignInManager<AppUser> signInManager)
        {
            _userManager = userManager;
            _signInManager = signInManager;
        }

        // GET /api/account/google-login
        [HttpGet("google-login")]
        public IActionResult GoogleLogin(string returnUrl = "/")
        {
            var redirectUrl = Url.Action(nameof(GoogleLoginCallback), "Account", new { returnUrl });
            var properties = new AuthenticationProperties { RedirectUri = redirectUrl };
            return Challenge(properties, "Google");
        }

        // GET /api/account/google-callback
        [HttpGet("google-callback")]
        public async Task<IActionResult> GoogleLoginCallback(string returnUrl = "/")
        {
            var authResult = await HttpContext.AuthenticateAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            if (!authResult.Succeeded)
            {
                // External login failed
                return Redirect("/login?error=external_auth_failed");
            }

            // Extract user claims from Google
            var externalUser = authResult.Principal;
            var email = externalUser.FindFirstValue(ClaimTypes.Email);
            var name = externalUser.FindFirstValue(ClaimTypes.Name);

            // Check if user exists in local Identity DB
            var user = await _userManager.FindByEmailAsync(email);
            if (user == null)
            {
                // Register new user with Google account
                user = new AppUser
                {
                    UserName = email,
                    Email = email
                    // Optionally store name in user fields if you have them
                };
                var createResult = await _userManager.CreateAsync(user);
                if (!createResult.Succeeded)
                {
                    return Redirect("/login?error=create_user_failed");
                }

                // Optionally assign a role (Patient, etc.)
                // await _userManager.AddToRoleAsync(user, "Patient");
            }

            // Log the user in
            await _signInManager.SignInAsync(user, isPersistent: false);
            return Redirect(returnUrl);
        }
    }
}
