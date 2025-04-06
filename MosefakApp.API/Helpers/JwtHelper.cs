using System.IdentityModel.Tokens.Jwt;

namespace MosefakApp.API.Helpers
{
    public static class JwtHelper
    {
        public static string? GetUserIdFromToken(string token)
        {
            var handler = new JwtSecurityTokenHandler();
            var jwt = handler.ReadJwtToken(token);

            var userIdClaim = jwt.Claims.FirstOrDefault(c =>
                                        c.Type == "nameid" ||
                                        c.Type == ClaimTypes.NameIdentifier ||
                                        c.Type.EndsWith("/identity/claims/nameidentifier"));

            return userIdClaim?.Value;

        }
    }

}
