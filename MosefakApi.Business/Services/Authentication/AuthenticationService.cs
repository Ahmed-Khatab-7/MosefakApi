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
        private readonly IHttpContextAccessor _HttpContextAccessor;
        private readonly ILogger<AuthenticationService> _logger;

        public AuthenticationService(UserManager<AppUser> userManger, SignInManager<AppUser> signInManager, IJwtProvider jwtProvider, RoleManager<AppRole> roleManger, IEmailSender emailSender, IEmailBodyBuilder emailBuilder, IHttpContextAccessor httpContextAccessor, ILogger<AuthenticationService> logger)
        {
            _userManager = userManger;
            _signInManager = signInManager;
            _jwtProvider = jwtProvider;
            _roleManger = roleManger;
            _emailSender = emailSender;
            _emailBuilder = emailBuilder;
            _HttpContextAccessor = httpContextAccessor;
            _logger = logger;
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
                if (registerRequest.IsDoctor)
                {
                    await _userManager.AddToRoleAsync(appUser, DefaultRole.PendingDoctor);
                }
                else
                {
                    await _userManager.AddToRoleAsync(appUser, DefaultRole.Patient);
                }

                var code = await _userManager.GenerateEmailConfirmationTokenAsync(appUser);
                code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));
                _logger.LogInformation(code);

                await SendConfirmationEmail(appUser, code);


                // Added: wait 10 minutes; if user still not confirmed, delete them
                _ = Task.Run(async () =>
                {
                    await Task.Delay(TimeSpan.FromMinutes(10));
                    var freshUser = await _userManager.FindByIdAsync(appUser.Id.ToString());
                    if (freshUser != null && !freshUser.EmailConfirmed)
                    {
                        await _userManager.DeleteAsync(freshUser);
                    }
                });
            }
            else
            {
                var errors = result.Errors.Select(x => x.Description).ToList();
                throw new BadRequest($"{string.Join(",", errors)}");
            }
        }

        public async Task<int> ConfirmEmailAsync(ConfirmEmailRequest request)
        {
            var user = await _userManager.FindByIdAsync(request.userId.ToString());

            if (user is null)
                throw new BadRequest("Invalid Code"); // من باب لخداع

            if (user.EmailConfirmed)
                throw new BadRequest("Email is already confirmed");

            var code = request.Code;

            try
            {
                code = Encoding.UTF8.GetString(WebEncoders.Base64UrlDecode(request.Code));
            }
            catch (FormatException)
            {
                throw new BadRequest("Invalid Code");
            }

            var result = await _userManager.ConfirmEmailAsync(user, code);

            if (!result.Succeeded)
            {
                var errors = result.Errors.Select(e => e.Description).ToList();
                throw new BadRequest(string.Join(',', errors));
            }

            return user.Id;
        }

        public async Task ResendConfirmationEmail(ResendConfirmationEmailRequest request)
        {
            var user = await _userManager.FindByEmailAsync(request.Email);

            if (user is null)
                return; // من باب الخداع

            if (user.EmailConfirmed)
                throw new BadRequest("Email is already confirmed");

            var code = await _userManager.GenerateEmailConfirmationTokenAsync(user);
            code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));

            _logger.LogInformation(code);

            await SendConfirmationEmail(user, code);
        }


        public async Task<bool> ValidateEmailExist(string email)
        {
            var user = await _userManager.FindByEmailAsync(email);

            return user is null ? false : true;
        }

        public async Task ForgetPasswordAsync(ForgetPasswordRequest request)
        {
            var user = await _userManager.FindByEmailAsync(request.Email);

            if (user == null)
                return; // من باب الخداع

            if (!user.EmailConfirmed)
                throw new BadRequest("Email is not confirmed");

            var code = await _userManager.GeneratePasswordResetTokenAsync(user);
            code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));

            // send email body

            await SendResetPasswordEmail(user, code);
        }


        public async Task ResetPasswordAsync(ResetPasswordRequest request)
        {
            var user = await _userManager.FindByEmailAsync(request.Email);

            if (user == null || !user.EmailConfirmed)
                throw new BadRequest("Invalid Code"); // من باب الخداع

            IdentityResult result;

            try
            {
                var code = Encoding.UTF8.GetString(WebEncoders.Base64UrlDecode(request.code));
                result = await _userManager.ResetPasswordAsync(user, code, request.NewPassword);
            }
            catch (FormatException)
            {
                result = IdentityResult.Failed(_userManager.ErrorDescriber.InvalidToken());
            }

            if (!result.Succeeded)
            {
                var error = result.Errors.Select(e => e.Description).FirstOrDefault();

                throw new BadRequest(error);
            }
        }

        private async Task SendConfirmationEmail(AppUser appUser, string code)
        {
            var origin = _HttpContextAccessor?.HttpContext?.Request.Headers.Origin;
            var imagePath = $"{origin}/images/198340114.jpeg";

            var body = await _emailBuilder.GenerateEmailBody(
                templateName: "emailTamplate.html",
                imageUrl: imagePath,
                header: $"Hi, {appUser.FirstName}",
                TextBody: "Please Confirm your email",
                link: $"{origin}/redirect/confirm-email?userId={appUser.Id}&code={code}",
                linkTitle: "Activate Account");

            await _emailSender.SendEmailAsync(appUser.Email!, "✅ Mosefak: Confirmation Email", body);
        }

        private async Task SendResetPasswordEmail(AppUser user, string code)
        {
            var origin = _HttpContextAccessor.HttpContext?.Request.Headers.Origin;
            var imagePath = $"{origin}/images/198340114.jpeg";

            var body = await _emailBuilder.GenerateEmailBody(
                templateName: "forgetPasswordTemplate.html",
                imageUrl: imagePath,
                header: $"Hi, {user.FirstName}",
                TextBody: "We received a request to reset your password. Click the button below to create a new password:",
                link: $"{origin}/redirect/reset-password?code={code}",
                linkTitle: "Reset"
                );

            await _emailSender.SendEmailAsync(user.Email!, "✅ Mosefak: Reset Your Password", body);
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
