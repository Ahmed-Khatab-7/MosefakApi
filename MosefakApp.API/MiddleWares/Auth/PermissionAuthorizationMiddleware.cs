namespace MosefakApp.API.MiddleWares.Auth
{
    public class PermissionAuthorizationMiddleware
    {
        private readonly RequestDelegate _next;

        public PermissionAuthorizationMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task Invoke(HttpContext context, IUserPermissionService permissionService)
        {
            var endpoint = context.GetEndpoint();

            // ✅ Skip permission checks for anonymous endpoints
            if (endpoint?.Metadata?.GetMetadata<AllowAnonymousPermissionAttribute>() != null)
            {
                await _next(context);
                return;
            }

            var token = context.Request.Headers["Authorization"].FirstOrDefault()?.Split(" ").Last();
            if (string.IsNullOrEmpty(token))
            {
                context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                await context.Response.WriteAsync("Missing token");
                return;
            }

            var userId = JwtHelper.GetUserIdFromToken(token); // your custom helper
            if (userId == null)
            {
                context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                await context.Response.WriteAsync("Invalid token");
                return;
            }

            var userPermissions = await permissionService.GetPermissionsForUserAsync(int.Parse(userId));

            var requiredPermission = endpoint?.Metadata?.GetMetadata<RequiredPermissionAttribute>()?.Permission;
            if (requiredPermission == null || userPermissions.Contains(requiredPermission))
            {
                await _next(context);
            }
            else
            {
                context.Response.StatusCode = StatusCodes.Status403Forbidden;
                await context.Response.WriteAsync("Access denied");
            }
        }

    }

}
