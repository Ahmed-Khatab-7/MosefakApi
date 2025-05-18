namespace MosefakApp.Core.Dtos.User.Requests
{
    public class ConfirmEmailRequest
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; } = null!; // Changed from userId

        [Required]
        [StringLength(4, MinimumLength = 4, ErrorMessage = "The verification code must be 4 digits.")]
        [RegularExpression(@"^\d{4}$", ErrorMessage = "The verification code must be 4 digits.")]
        public string Code { get; set; } = null!;
    }
}
