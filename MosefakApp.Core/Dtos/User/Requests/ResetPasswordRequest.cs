namespace MosefakApp.Core.Dtos.User.Requests
{
    public class ResetPasswordRequest
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; }

        [Required]
        [StringLength(4, MinimumLength = 4, ErrorMessage = "The verification code must be 4 digits.")]
        [RegularExpression(@"^\d{4}$", ErrorMessage = "The verification code must be 4 digits.")]
        public string code { get; set; } = null!; // Maintain your existing property name

        [Required]
        [StringLength(100, MinimumLength = 6)]
        public string NewPassword { get; set; } = null!;
    }
}
