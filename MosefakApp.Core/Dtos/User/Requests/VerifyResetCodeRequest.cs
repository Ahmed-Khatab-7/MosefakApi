using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MosefakApp.Core.Dtos.User.Requests;
public class VerifyResetCodeRequest
{
    [Required]
    [EmailAddress]
    public string Email { get; set; } = null!;

    [Required]
    [StringLength(4, MinimumLength = 4, ErrorMessage = "Code must be 4 digits.")]
    [RegularExpression(@"^\d{4}$", ErrorMessage = "Code must be a 4-digit number.")]
    public string Code { get; set; } = null!;
}
