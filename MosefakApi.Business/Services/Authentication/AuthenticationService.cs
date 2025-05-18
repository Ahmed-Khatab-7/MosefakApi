using System.Web;

namespace MosefakApi.Business.Services.Authentication
{
    public class AuthenticationService : IAuthenticationService
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly RoleManager<AppRole> _roleManger;
        private readonly SignInManager<AppUser> _signInManager;
        private readonly IEmailSender _emailSender;
        private readonly IEmailBodyBuilder _emailBuilder;
        private readonly IJwtProvider _jwtProvider;
        private readonly ILogger<AuthenticationService> _logger;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private const int MaxVerificationAttempts = 5;

        public AuthenticationService(UserManager<AppUser> userManger, SignInManager<AppUser> signInManager, IJwtProvider jwtProvider, RoleManager<AppRole> roleManger, IEmailSender emailSender, IEmailBodyBuilder emailBuilder, IHttpContextAccessor httpContextAccessor, ILogger<AuthenticationService> logger)
        {
            _userManager = userManger;
            _signInManager = signInManager;
            _jwtProvider = jwtProvider;
            _roleManger = roleManger;
            _emailSender = emailSender;
            _emailBuilder = emailBuilder;
            _httpContextAccessor = httpContextAccessor;
            _logger = logger;
        }

        private string GenerateVerificationCode()
        {
            Random random = new Random();
            return random.Next(1000, 10000).ToString("D4");
        }


        public async Task<LoginResponse> Login(LoginRequest request)
        {
            var user = await _userManager.FindByEmailAsync(request.Email);

            if (user is null)
                throw new BadRequest("Invalid UserName or Password");

            if (user.IsDisabled)
                throw new BadRequest("Disabled User, please contact your admin");

            var result = await _signInManager.PasswordSignInAsync(user, request.Password, false, true);

            if (!result.Succeeded)
            {
                throw result.IsNotAllowed ? new BadRequest("You must confirm your email.")
                     : result.IsLockedOut ? new BadRequest("You are Locked Out")
                     : new BadRequest("Invalid Email Or Password");
            }

            // ✅ Update FCM Token if provided
            if (!string.IsNullOrEmpty(request.FcmToken) && user.FcmToken != request.FcmToken)
            {
                user.FcmToken = request.FcmToken;
                await _userManager.UpdateAsync(user);  // Save the new token
            }

            var roles = await GetRoles(user);
            var jwtProviderResponse = _jwtProvider.GenerateToken(user, roles);

            var response = GetLoginResponse(user, jwtProviderResponse);

            return response ?? throw new BadRequest("Invalid UserName or Password");
        }

        public async Task Register(RegisterRequest registerRequest)
        {
            if (registerRequest is null)
                throw new BadRequest("Data is null");

            var CheckEmailExist = await ValidateEmailExist(registerRequest.Email);

            if (CheckEmailExist)
                throw new ItemAlreadyExist("Email Already Exist");

            var appUser = new AppUser()
            {
                FirstName = registerRequest.FirstName,
                LastName = registerRequest.LastName,
                Email = registerRequest.Email,
                UserName = registerRequest.Email.Split('@')[0],
                PhoneNumber = registerRequest.PhoneNumber,
                UserType = registerRequest.IsDoctor ? UserType.PendingDoctor : UserType.Patient,
            };

            var result = await _userManager.CreateAsync(appUser, registerRequest.Password);

            if (result.Succeeded)
            {
                // Assign roles
                if (registerRequest.IsDoctor)
                {
                    await _userManager.AddToRoleAsync(appUser, DefaultRole.PendingDoctor);
                }
                else
                {
                    await _userManager.AddToRoleAsync(appUser, DefaultRole.Patient);
                }

                // Generate a 4-digit verification code
                string fourDigitCode = GenerateVerificationCode();
                appUser.VerificationCode = fourDigitCode;
                appUser.VerificationCodeExpiry = DateTime.UtcNow.AddMinutes(5); // 24-hour expiry
                appUser.VerificationAttempts = 0;
                await _userManager.UpdateAsync(appUser);

                _logger.LogInformation($"User {appUser.Email} registered. Verification code: {fourDigitCode}");
                await SendConfirmationEmail(appUser, fourDigitCode);
            }
            else
            {
                var errors = result.Errors.Select(x => x.Description).ToList();
                throw new BadRequest($"{string.Join(",", errors)}");
            }
        }

        public async Task<int> ConfirmEmailAsync(ConfirmEmailRequest request) // DTO is already updated
        {
            // Find user by email instead of ID
            var user = await _userManager.FindByEmailAsync(request.Email);

            if (user == null) throw new BadHttpRequestException("User not found or invalid email."); // Or your custom "Invalid request"
            if (user.EmailConfirmed) throw new BadHttpRequestException("Email already confirmed.");

            if (string.IsNullOrEmpty(user.VerificationCode) || !user.VerificationCodeExpiry.HasValue || user.VerificationCodeExpiry < DateTime.UtcNow)
            {
                _logger.LogWarning($"Invalid or expired verification code attempt for email {request.Email}. Code: {user.VerificationCode}, Expiry: {user.VerificationCodeExpiry}");
                throw new BadHttpRequestException("Verification code is invalid or has expired.");
            }
            if (user.VerificationAttempts >= MaxVerificationAttempts) // MaxVerificationAttempts = 5 (as defined before)
            {
                _logger.LogWarning($"Max verification attempts reached for email {request.Email}.");
                throw new BadHttpRequestException("Maximum verification attempts reached. Please request a new code.");
            }
            if (user.VerificationCode != request.Code)
            {
                user.VerificationAttempts++;
                await _userManager.UpdateAsync(user);
                _logger.LogWarning($"Invalid verification code for email {request.Email}. Attempt {user.VerificationAttempts}/{MaxVerificationAttempts}. Code provided: {request.Code}, Expected: {user.VerificationCode}");
                throw new BadHttpRequestException($"Invalid verification code. Attempts left: {MaxVerificationAttempts - user.VerificationAttempts}");
            }

            // If 4-digit code is valid, then confirm email with a standard Identity token
            // This is a crucial step for Identity to correctly mark the email as confirmed internally.
            var identityToken = await _userManager.GenerateEmailConfirmationTokenAsync(user);
            var result = await _userManager.ConfirmEmailAsync(user, identityToken);

            if (!result.Succeeded)
            {
                var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                _logger.LogError($"Email confirmation failed for {user.Email} with Identity token: {errors}");
                throw new BadHttpRequestException($"Email confirmation failed: {errors}");
            }

            user.VerificationCode = null; // Clear the code after successful use
            user.VerificationCodeExpiry = null;
            user.VerificationAttempts = 0;
            // user.EmailConfirmed = true; // ConfirmEmailAsync should handle this, but good to be aware.
            await _userManager.UpdateAsync(user);

            _logger.LogInformation($"Email for user {user.Email} confirmed successfully using 4-digit code.");
            return user.Id; // Still returning userId, which can be useful for the caller
        }


        public async Task ResendConfirmationEmail(ResendConfirmationEmailRequest request) // DTO with Email
        {
            var user = await _userManager.FindByEmailAsync(request.Email);
            if (user == null)
            {
                _logger.LogWarning($"Resend confirmation attempt for non-existent email: {request.Email}");
                return; // Don't reveal if user exists
            }
            if (user.EmailConfirmed) throw new BadHttpRequestException("Email is already confirmed.");

            string fourDigitCode = GenerateVerificationCode();
            user.VerificationCode = fourDigitCode;
            user.VerificationCodeExpiry = DateTime.UtcNow.AddHours(24);
            user.VerificationAttempts = 0;
            await _userManager.UpdateAsync(user);

            _logger.LogInformation($"Resending confirmation email to {user.Email}. Verification code: {fourDigitCode}");
            await SendConfirmationEmail(user, fourDigitCode);
        }


        public async Task ForgetPasswordAsync(ForgetPasswordRequest request) // DTO with Email
        {
            var user = await _userManager.FindByEmailAsync(request.Email);
            if (user == null)
            {
                _logger.LogWarning($"Password reset attempt for non-existent email: {request.Email}");
                return; // Don't reveal if user exists for security
            }
            if (!user.EmailConfirmed) throw new BadHttpRequestException("Please confirm your email before resetting password.");


            string fourDigitCode = GenerateVerificationCode();
            user.VerificationCode = fourDigitCode;
            user.VerificationCodeExpiry = DateTime.UtcNow.AddHours(1); // 1-hour expiry for password reset
            user.VerificationAttempts = 0;
            await _userManager.UpdateAsync(user);

            _logger.LogInformation($"Password reset initiated for {user.Email}. Verification code: {fourDigitCode}");
            await SendResetPasswordEmail(user, fourDigitCode);
        }

        private async Task SendResetPasswordEmail(AppUser user, string verificationCode)
        {
            var origin = _httpContextAccessor?.HttpContext?.Request.Headers["Origin"].ToString() ??
                         $"{_httpContextAccessor?.HttpContext?.Request.Scheme}://{_httpContextAccessor?.HttpContext?.Request.Host}";

            string currentUtc = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss");
            string userLogin = HttpUtility.HtmlEncode(user.UserName);

            string textBody = "We received a request to reset your password. Please use the following 4-digit code:";

            var emailBody = await _emailBuilder.GenerateEmailBody(
                templateName: "forgetPasswordTemplate.html",
                imageUrl: $"{origin}/images/198340114.jpeg",
                header: $"Hi, {HttpUtility.HtmlEncode(user.FirstName)}",
                TextBody: textBody,
                link: $"{origin}/reset-password-page?email={HttpUtility.UrlEncode(user.Email)}", // Link to your frontend reset page
                linkTitle: "Reset Password",
                verificationCode: verificationCode,
                currentDate: currentUtc,
                username: userLogin
            );

            await _emailSender.SendEmailAsync(user.Email, "🔑 Mosefak: Reset Your Password", emailBody);
            _logger.LogInformation($"Password reset email sent to {user.Email}.");
        }

        public async Task ResetPasswordAsync(ResetPasswordRequest request) // DTO with Email, Code, NewPassword
        {
            var user = await _userManager.FindByEmailAsync(request.Email);
            if (user == null) throw new BadHttpRequestException("User not found or invalid request.");

            if (string.IsNullOrEmpty(user.VerificationCode) || user.VerificationCodeExpiry < DateTime.UtcNow)
            {
                throw new BadHttpRequestException("Password reset code is invalid or has expired.");
            }
            if (user.VerificationAttempts >= MaxVerificationAttempts)
            {
                throw new BadHttpRequestException("Maximum verification attempts reached. Please request a new code.");
            }
            if (user.VerificationCode != request.code) // 'code' from your DTO
            {
                user.VerificationAttempts++;
                await _userManager.UpdateAsync(user);
                throw new BadHttpRequestException($"Invalid reset code. Attempts left: {MaxVerificationAttempts - user.VerificationAttempts}");
            }

            // If 4-digit code is valid, then reset password with a standard Identity token
            var identityToken = await _userManager.GeneratePasswordResetTokenAsync(user);
            var result = await _userManager.ResetPasswordAsync(user, identityToken, request.NewPassword);

            if (!result.Succeeded)
            {
                var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                throw new BadHttpRequestException($"Password reset failed: {errors}");
            }

            user.VerificationCode = null; // Clear the code
            user.VerificationCodeExpiry = null;
            user.VerificationAttempts = 0;
            await _userManager.UpdateAsync(user);

            _logger.LogInformation($"Password for user {user.Email} reset successfully.");
        }


        public async Task<bool> ValidateEmailExist(string email)
        {
            var user = await _userManager.FindByEmailAsync(email);

            return user is null ? false : true;
        }


        private async Task SendConfirmationEmail(AppUser appUser, string verificationCode)
        {
            var origin = _httpContextAccessor?.HttpContext?.Request.Headers["Origin"].ToString() ??
                         $"{_httpContextAccessor?.HttpContext?.Request.Scheme}://{_httpContextAccessor?.HttpContext?.Request.Host}";

            string currentUtc = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss");
            string userLogin = HttpUtility.HtmlEncode(appUser.FirstName); // Or appUser.Email if UserName might be different

            string textBody = "Please use the following 4-digit code to confirm your email:";

            var emailBody = await _emailBuilder.GenerateEmailBody(
                templateName: "emailTamplate.html",
                imageUrl: $"{origin}/images/198340114.jpeg",
                header: $"Hi, {HttpUtility.HtmlEncode(appUser.FirstName ?? "User")}",
                TextBody: textBody,
                // Updated link to carry email - your frontend page at /confirm-email-page needs to handle this query parameter
                link: $"{origin}/confirm-email-page?email={HttpUtility.UrlEncode(appUser.Email)}",
                linkTitle: "Confirm Email",
                verificationCode: verificationCode,
                currentDate: currentUtc,
                username: userLogin
            );

            await _emailSender.SendEmailAsync(appUser.Email, "✅ Mosefak: Confirm Your Email", emailBody);
            _logger.LogInformation($"Confirmation email sent to {appUser.Email} with link containing email.");
        }
        private LoginResponse GetLoginResponse(AppUser user, JwtProviderResponse response)
        {

            return new LoginResponse()
            {
                Email = user.Email,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Token = response.Token,
                ExpireIn = response.ExpireIn * 60,
            };
        }


        private async Task<IEnumerable<string>> GetRoles(AppUser user)
        {
            var roles = await _userManager.GetRolesAsync(user);

            return roles;
        }

    }
}
