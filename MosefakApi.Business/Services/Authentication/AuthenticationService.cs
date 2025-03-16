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

            var (roles, permissions) = await GetRolesAndPermissions(user);
            var jwtProviderResponse = _jwtProvider.GenerateToken(user, roles, permissions);

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
                UserName = registerRequest.Email.Split('@')[0], // Bilal48@gmail.com  --> Bilal48
                PhoneNumber = registerRequest.PhoneNumber,
            };

            var result = await _userManager.CreateAsync(appUser, registerRequest.Password);

            if (result.Succeeded)
            {
                if (registerRequest.IsDoctor)
                {
                    await _userManager.AddToRoleAsync(appUser, DefaultRole.PendingDoctor);
                    // ❌ Do NOT create Doctor record yet - will be created later in profile completion
                }
                else
                {
                    await _userManager.AddToRoleAsync(appUser, DefaultRole.Patient);
                }

                var code = await _userManager.GenerateEmailConfirmationTokenAsync(appUser);
                code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));

                _logger.LogInformation(code); // will remove it before production

                await SendConfirmationEmail(appUser, code);
            }
            else
            {
                var errors = result.Errors.Select(x => x.Description).ToList();

                throw new BadRequest($"{string.Join(",", errors)}");
            }
        }

        public async Task ConfirmEmailAsync(ConfirmEmailRequest request)
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

            //// Assign Registered Users to Default Role

            //await _userManager.AddToRoleAsync(user, DefaultRole.Patient);
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
            string? imageFromGoogle = "https://encrypted-tbn0.gstatic.com/images?q=tbn:ANd9GcTc9APxkj0xClmrU3PpMZglHQkx446nQPG6lA&s";

            var origin = _HttpContextAccessor?.HttpContext?.Request.Headers.Origin;
            var imagePath = $"{origin}/images/198340114.jpeg";

            var body = await _emailBuilder.GenerateEmailBody(
                templateName: "emailTamplate.html",
                imageUrl: imageFromGoogle,
                header: $"Hi, {appUser.FirstName}",
                TextBody: "Please Confirm your email",
                link: $"{origin}/api/Authentication/confirm-email?userId={appUser.Id}&Code={code}", // will be Url that belong component(confirm-email component) that when open (OnInit) will send this Url and go to url that belong it..
                linkTitle: "Activate Account");

            await _emailSender.SendEmailAsync(appUser.Email!, "✅ Mosefak: Confirmation Email", body);
        }

        private async Task SendResetPasswordEmail(AppUser user, string code)
        {
            // Full URL for image
            string? imageFromGoogle = "https://encrypted-tbn0.gstatic.com/images?q=tbn:ANd9GcTc9APxkj0xClmrU3PpMZglHQkx446nQPG6lA&s";

            var origin = _HttpContextAccessor.HttpContext?.Request.Headers.Origin;
            var imagePath = $"{origin}/images/198340114.jpeg";

            var body = await _emailBuilder.GenerateEmailBody(
                templateName: "forgetPasswordTemplate.html",
                imageUrl: imageFromGoogle,
                header: $"Hi, {user.FirstName}",
                TextBody: "We received a request to reset your password. Click the button below to create a new password:",
                link: $"{origin}/reset-password?code={code}", // will be "reset-password component url and sent to it code as query string" and after fill fields(email,pass,conf) then click on reset will call reset-password action and send 4 info (email,code,pass,confpass)
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


        private async Task<(IEnumerable<string> roles, IEnumerable<string> permissions)> GetRolesAndPermissions(AppUser user)
        {
            var roles = await _userManager.GetRolesAsync(user);
            var permissions = new List<string>();

            foreach (var item in roles)
            {
                var role = await _roleManger.Roles.FirstOrDefaultAsync(x => x.Name!.ToLower() == item.ToLower());

                if (role != null)
                {
                    var claimsRole = await _roleManger.GetClaimsAsync(role);

                    if (claimsRole != null)
                    {
                        foreach (var claim in claimsRole)
                        {
                            permissions.Add(claim.Value);
                        }
                    }
                }
            }

            return (roles, permissions);
        }

    }
}
