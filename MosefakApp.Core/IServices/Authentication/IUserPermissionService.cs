namespace MosefakApp.Core.IServices.Authentication
{
    public interface IUserPermissionService
    {
        Task<List<string>> GetPermissionsForUserAsync(int userId);
    }
}
