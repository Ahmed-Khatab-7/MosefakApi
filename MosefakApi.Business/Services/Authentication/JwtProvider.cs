namespace MosefakApi.Business.Services.Authentication
{
    public class JwtProvider : IJwtProvider
    {
        private readonly JwtSetting _jwtSetting;

        public JwtProvider(IOptions<JwtSetting> jwtSetting)
        {
            _jwtSetting = jwtSetting.Value;
        }

        public JwtProviderResponse GenerateToken(AppUser applicationUser, IEnumerable<string> roles, IEnumerable<string> permissions)
        {
            // 1) Create signing credentials
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSetting.Key));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            // 2) Build the core claims you really need
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, applicationUser.Id.ToString()),
                new Claim(ClaimTypes.GivenName, applicationUser.FirstName ?? string.Empty),
                new Claim(ClaimTypes.Name, applicationUser.UserName ?? string.Empty),
                new Claim(ClaimTypes.Email, applicationUser.Email ?? string.Empty)
            };

            // 3) If you truly need user roles in the token, add them individually:
            foreach (var role in roles)
            {
                claims.Add(new Claim(ClaimTypes.Role, role));
            }

            // 4) Consider NOT storing permissions in the token, or store them individually if truly needed:
            // foreach (var permission in permissions)
            // {
            //     claims.Add(new Claim("perm", permission));
            // }
            // Or better yet, handle them in the database rather than in the JWT.

            var descriptor = new SecurityTokenDescriptor
            {
                Issuer = _jwtSetting.Issuer,
                Audience = _jwtSetting.Audience,
                Expires = DateTime.UtcNow.AddMinutes(_jwtSetting.lifeTime),
                SigningCredentials = creds,
                Subject = new ClaimsIdentity(claims)
            };

            // 5) Create the token
            var tokenHandler = new JwtSecurityTokenHandler();
            var createToken = tokenHandler.CreateToken(descriptor);
            var token = tokenHandler.WriteToken(createToken);

            return new JwtProviderResponse
            {
                Token = token,
                ExpireIn = _jwtSetting.lifeTime
            };
        }

        public string? ValidateToken(string token)
        {
            var handler = new JwtSecurityTokenHandler();
            var symmetricKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSetting.Key));

            try
            {
                var validationParams = new TokenValidationParameters
                {
                    IssuerSigningKey = symmetricKey,
                    ValidateIssuerSigningKey = true,
                    ValidateIssuer = false,
                    ValidateAudience = false,
                    ClockSkew = TimeSpan.Zero, // no extra time window
                };

                handler.ValidateToken(token, validationParams, out SecurityToken validatedToken);
                var jwtToken = (JwtSecurityToken)validatedToken;

                // The user’s ID is stored under ClaimTypes.NameIdentifier => "nameid"
                var userId = jwtToken.Claims.First(c => c.Type == ClaimTypes.NameIdentifier).Value;

                return userId;
            }
            catch
            {
                return null;
            }
        }
    }
}
