using System.ComponentModel.DataAnnotations;

namespace MosefakApp.Core.Dtos.Authentication.Requests
{
    public class FirebaseLoginRequest
    {
        [Required]
        public string IdToken { get; set; } = null!;   // Firebase ID token

        public string? FcmToken { get; set; }           // Optional device token
    }
}