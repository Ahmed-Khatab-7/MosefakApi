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
        private const string MosefakLogoUrl = "https://mosefakapiss.runasp.net/images/mosefak-logo.png";
        private const string PasswordResetPurposeToken = "ResetPassword";


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


        public async Task ForgetPasswordAsync(ForgetPasswordRequest request)
        {
            var user = await _userManager.FindByEmailAsync(request.Email);
            if (user == null)
            {
                _logger.LogWarning($"Password reset attempt for non-existent email: {request.Email}");
                // For security, don't reveal if the user exists. Silently return or throw a generic error if your API always returns something.
                // For this example, we'll proceed as if an email would be sent (but it won't if user is null).
                // A common practice is to always return a success-like message to prevent email enumeration.
                return;
            }

            if (!user.EmailConfirmed)
            {
                _logger.LogWarning($"Password reset attempt for unconfirmed email: {request.Email}");
                throw new BadHttpRequestException("Please confirm your email address before attempting a password reset.");
            }

            string fourDigitCode = GenerateVerificationCode();
            user.VerificationCode = fourDigitCode;
            user.VerificationCodeExpiry = DateTime.UtcNow.AddHours(1); // Standard 1-hour expiry for the code itself
            user.VerificationAttempts = 0;
            await _userManager.UpdateAsync(user);

            _logger.LogInformation($"Password reset initiated for {user.Email}. Verification code: {fourDigitCode}");
            await SendResetPasswordEmail(user, fourDigitCode); // This email now just contains the code
        }


        private async Task SendResetPasswordEmail(AppUser user, string verificationCode)
        {
            var origin = _httpContextAccessor?.HttpContext?.Request.Headers["Origin"].ToString() ??
                         $"{_httpContextAccessor?.HttpContext?.Request.Scheme}://{_httpContextAccessor?.HttpContext?.Request.Host}";

            string currentUtc = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss");
            string userLogin = HttpUtility.HtmlEncode(user.FirstName ?? user.Email?.Split('@')[0] ?? "N/A");

            string textBody = "We received a request to reset your password. Please use the following 4-digit code on our platform to proceed:";

            var emailBody = await _emailBuilder.GenerateEmailBody(
                templateName: "forgetPasswordTemplate.html", // This template shows the 4-digit code
                imageUrl: MosefakLogoUrl,
                header: $"Hi, {HttpUtility.HtmlEncode(user.FirstName ?? "User")}",
                TextBody: textBody,
                verificationCode: verificationCode,
                currentDate: currentUtc,
                username: userLogin
            );

            await _emailSender.SendEmailAsync(user.Email, "Mosefak: Your Password Reset Code", emailBody);
            _logger.LogInformation($"Password reset code email sent to {user.Email}.");
        }


        public async Task VerifyResetCodeAsync(VerifyResetCodeRequest request)
        {
            var user = await _userManager.FindByEmailAsync(request.Email);
            if (user == null)
            {
                _logger.LogWarning($"Verify reset code attempt for non-existent email: {request.Email}");
                throw new BadHttpRequestException("Invalid email or verification code.");
            }

            if (string.IsNullOrEmpty(user.VerificationCode) || !user.VerificationCodeExpiry.HasValue)
            {
                _logger.LogWarning($"Verify reset code attempt for {request.Email} with no active code stored.");
                throw new BadHttpRequestException("No active verification code found. Please request a new one.");
            }

            if (user.VerificationCodeExpiry < DateTime.UtcNow)
            {
                _logger.LogWarning($"Verify reset code attempt for {request.Email} with expired code. Stored Expiry: {user.VerificationCodeExpiry}");
                throw new BadHttpRequestException("Verification code has expired. Please request a new one.");
            }

            if (user.VerificationAttempts >= MaxVerificationAttempts)
            {
                _logger.LogWarning($"Max verification attempts reached for reset code for email {request.Email}.");
                throw new BadHttpRequestException("Maximum verification attempts reached. Please request a new code.");
            }

            if (user.VerificationCode != request.Code)
            {
                user.VerificationAttempts++;
                await _userManager.UpdateAsync(user);
                _logger.LogWarning($"Invalid reset code for email {request.Email}. Attempt {user.VerificationAttempts}/{MaxVerificationAttempts}.");
                throw new BadHttpRequestException($"Invalid verification code. Attempts left: {MaxVerificationAttempts - user.VerificationAttempts}");
            }

            // Code is valid. Mark it as "verified" for the next step.
            // We'll re-purpose VerificationCode to store the actual Identity Password Reset Token.
            // This token is what _userManager.ResetPasswordAsync needs.
            var identityPasswordResetToken = await _userManager.GeneratePasswordResetTokenAsync(user);

            user.VerificationCode = identityPasswordResetToken; // Store the REAL token now
            user.VerificationCodeExpiry = DateTime.UtcNow.AddMinutes(15); // User has 15 mins to complete the actual reset
            user.VerificationAttempts = 0; // Reset attempts
            await _userManager.UpdateAsync(user);

            _logger.LogInformation($"4-digit reset code verified for {request.Email}. Identity reset token generated and stored temporarily.");
            // No need to send another email here. Frontend proceeds to the "new password" form.
        }


        public async Task ResetPasswordAsync(ResetPasswordRequest request)
        {
            var user = await _userManager.FindByEmailAsync(request.Email);
            if (user == null)
            {
                _logger.LogWarning($"Actual password reset attempt for non-existent email: {request.Email}");
                throw new BadHttpRequestException("Invalid request or user not found.");
            }

            // Check if the VerifyResetCode step was completed successfully and recently
            // The user.VerificationCode should now hold the Identity Password Reset Token
            if (string.IsNullOrEmpty(user.VerificationCode) || !user.VerificationCodeExpiry.HasValue || user.VerificationCodeExpiry < DateTime.UtcNow)
            {
                _logger.LogWarning($"Password reset attempt for {request.Email} without prior code verification or window expired. Stored Expiry: {user.VerificationCodeExpiry}");
                throw new BadHttpRequestException("Password reset window has expired or code was not verified. Please start over.");
            }

            // The user.VerificationCode now holds the actual token generated in VerifyResetCodeAsync
            var identityResetToken = user.VerificationCode;
            var result = await _userManager.ResetPasswordAsync(user, identityResetToken, request.NewPassword);

            if (!result.Succeeded)
            {
                var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                // Do NOT increment VerificationAttempts here as this stage is past the 4-digit code.
                _logger.LogError($"Password reset failed for {user.Email} using Identity token: {errors}");
                throw new BadHttpRequestException($"Password reset failed: {errors}. Please try the process again.");
            }

            // Password reset was successful, clean up.
            user.VerificationCode = null;
            user.VerificationCodeExpiry = null;
            user.VerificationAttempts = 0;
            // Consider if you need to update security stamp or force logout other sessions
            // await _userManager.UpdateSecurityStampAsync(user); 
            await _userManager.UpdateAsync(user);

            _logger.LogInformation($"Password for user {user.Email} has been reset successfully.");
            // Optionally send a confirmation email that password was changed.
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
                imageUrl: MosefakLogoUrl,
                header: $"Hi, {HttpUtility.HtmlEncode(appUser.FirstName ?? "User")}",
                TextBody: textBody,
                verificationCode: verificationCode,
                currentDate: currentUtc,
                username: userLogin
            );


            await _emailSender.SendEmailAsync(appUser.Email, "Mosefak: Confirm Your Email", emailBody);
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
