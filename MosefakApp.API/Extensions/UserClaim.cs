namespace MosefakApp.API.Extensions
{
    public static class UserClaim
    {
        public static int GetUserId(this ClaimsPrincipal principal)
        {
            return Convert.ToInt32(principal.FindFirstValue(ClaimTypes.NameIdentifier));
        }
    }

}
