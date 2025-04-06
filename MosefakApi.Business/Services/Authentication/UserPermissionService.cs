namespace MosefakApi.Business.Services.Authentication
{
    public class UserPermissionService : IUserPermissionService
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly RoleManager<AppRole> _roleManager;

        public UserPermissionService(UserManager<AppUser> userManager, RoleManager<AppRole> roleManager)
        {
            _userManager = userManager;
            _roleManager = roleManager;
        }

        public async Task<List<string>> GetPermissionsForUserAsync(int userId)
        {
            var user = await _userManager.FindByIdAsync(userId.ToString());
            if (user == null) return new List<string>();

            var roles = await _userManager.GetRolesAsync(user);

            var permissions = new HashSet<string>();

            foreach (var roleName in roles)
            {
                var role = await _roleManager.FindByNameAsync(roleName);
                if (role == null) continue;

                var claims = await _roleManager.GetClaimsAsync(role);
                foreach (var claim in claims)
                {
                    if (claim.Type == Permissions.Type)
                    {
                        permissions.Add(claim.Value);
                    }
                }
            }

            return permissions.ToList();
        }
    }

}
