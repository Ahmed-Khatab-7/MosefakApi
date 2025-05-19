namespace MosefakApp.Core.IServices.Authentication
{
    public interface IAuthenticationService
    {
        Task<LoginResponse> Login(LoginRequest request);
        Task Register(RegisterRequest registerRequest);
        Task<int> ConfirmEmailAsync(ConfirmEmailRequest request);
        Task ResendConfirmationEmail(ResendConfirmationEmailRequest request); // to get on code that will use it for ConfirmEmailAsync method to confirm-email
        Task ForgetPasswordAsync(ForgetPasswordRequest request);
        Task VerifyResetCodeAsync(VerifyResetCodeRequest request);
        Task ResetPasswordAsync(ResetPasswordRequest request); //  a password reset action does not require the user to be authenticated, as the user typically requests a password reset because they’ve lost access to their account
    }
}
