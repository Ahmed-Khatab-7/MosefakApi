using MosefakApp.API.MiddleWares.Auth;

namespace MosefakApp.API.Extensions
{
    public static class MiddlewareExtensions
    {
        public static IApplicationBuilder UsePermissionAuthorization(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<PermissionAuthorizationMiddleware>();
        }
    }

}
