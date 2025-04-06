namespace MosefakApi.Business.Services.Authentication
{
    public class JwtProvider : IJwtProvider
    {
        private readonly JwtSetting _jwtSetting;

        public JwtProvider(IOptions<JwtSetting> jwtSetting)
        {
            _jwtSetting = jwtSetting.Value;
        }

        public JwtProviderResponse GenerateToken(AppUser applicationUser, IEnumerable<string> roles)
        {
            var descriptor = new SecurityTokenDescriptor()
            {
                Issuer = _jwtSetting.Issuer,
                Audience = _jwtSetting.Audience,
                Expires = DateTime.Now.AddMinutes(_jwtSetting.lifeTime),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSetting.Key)), SecurityAlgorithms.HmacSha256),
                Subject = new ClaimsIdentity(new List<Claim>()
                {
                    new Claim(ClaimTypes.NameIdentifier, applicationUser.Id.ToString()),
                    new Claim(ClaimTypes.GivenName, applicationUser.FirstName),
                    new Claim(ClaimTypes.Name, applicationUser.UserName!),
                    new Claim(ClaimTypes.Email, applicationUser.Email!),
                    new Claim(nameof(roles), System.Text.Json.JsonSerializer.Serialize(roles),JsonClaimValueTypes.JsonArray),
                    //new Claim(nameof(permissions), System.Text.Json.JsonSerializer.Serialize(permissions), JsonClaimValueTypes.JsonArray),
                })
            };
            #region Another way to send Roles and Permissions that belong user
            //if(roles != null )
            //{
            //    foreach (var role in roles)
            //    {
            //        descriptor.Subject.AddClaim(new Claim(ClaimTypes.Role, role));
            //    }
            //}

            //if(permissions != null)
            //{
            //    foreach (var permission in permissions)
            //    {
            //        descriptor.Subject.AddClaim(new Claim(Permissions.Type, permission));
            //    }
            //}
            #endregion

            var tokenHandler = new JwtSecurityTokenHandler();

            var createToken = tokenHandler.CreateToken(descriptor);
            var token = tokenHandler.WriteToken(createToken);

            return new JwtProviderResponse()
            {
                Token = token,
                ExpireIn = _jwtSetting.lifeTime
            };
        }

        public string? ValidateToken(string Token)
        {
            var handler = new JwtSecurityTokenHandler();
            var SymmetricSecurityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSetting.Key));

            try
            {
                handler.ValidateToken(Token, new TokenValidationParameters
                {
                    IssuerSigningKey = SymmetricSecurityKey,
                    ValidateIssuerSigningKey = true,
                    ValidateIssuer = false,
                    ValidateAudience = false,
                    ClockSkew = TimeSpan.Zero,
                },
                out SecurityToken validatedToken);

                var jwtToken = (JwtSecurityToken)validatedToken;

                var userId = jwtToken.Claims.First(claim => claim.Type == "nameid").Value;

                return userId;
            }
            catch
            {
                return null;
            }
        }
    }
}