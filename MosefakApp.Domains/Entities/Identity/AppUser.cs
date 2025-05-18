namespace MosefakApp.Domains.Entities.Identity
{
    public class AppUser : IdentityUser<int>
    {
        public string FirstName { get; set; } = null!;
        public string LastName { get; set; } = null!;
        public Gender? Gender { get; set; }
        public Address? Address { get; set; } 
        public DateTime? DateOfBirth { get; set; }
        public string? ImagePath { get; set; } // when register will not ask him to enter image, but in profile settings can upload image
        public string? FirebaseUid { get; set; } // Stores the unique ID from Firebase Auth
        public string? FcmToken { get; set; }  // Accept FCM token from Flutter // Firebase Token
        public string? VerificationCode { get; set; }
        public DateTime? VerificationCodeExpiry { get; set; }
        public int VerificationAttempts { get; set; } = 0;
        public DateTime CreationTime { get; set; } = DateTime.UtcNow;
        public bool IsDeleted { get; set; } = false;
        public bool IsDisabled { get; set; } = false;
        
        [NotMapped]
        public int Age => DateOfBirth.HasValue ? (int)((DateTime.UtcNow - DateOfBirth.Value).TotalDays / 365.25) : 0;

        public UserType UserType { get; set; } = UserType.Patient;
    }
}
