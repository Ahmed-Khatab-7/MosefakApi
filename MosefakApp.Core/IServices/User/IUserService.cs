namespace MosefakApp.Core.IServices.User
{
    public interface IUserService
    {

        // For admin
        Task<(List<UserResponse> responses, int totalPages)> GetUsersAsync(bool IncludeDeleted = false, int pageNumber = 1, int pageSize = 10);
        Task<UserResponse> GetUserByIdAsync(int id);
        Task<UserResponse> CreateUserAsync(UserRequest request);
        Task<UserResponse> UpdateUserAsync(int id, UserRequest request);
        Task DeleteUserAsync(int id);
        Task UnLock(int id);

        // for user
        Task ChangeEmail(ChangeEmailRequest changeEmailRequest);
        Task ChangePasswordAsync(string userId, ChangePasswordRequest changePasswordRequest);
        Task<bool> UpdateFcmToken(int userId, UpdateFcmTokenDto model);

        Task<List<string>> GetAllFcmTokensAsync();
        Task<string?> GetFcmTokenByUserIdAsync(int userId);

    }
}
