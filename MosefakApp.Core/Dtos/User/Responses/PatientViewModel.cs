using System.ComponentModel.DataAnnotations.Schema;

namespace MosefakApp.Core.Dtos.User.Responses
{
    public class PatientViewModel
    {
        public string Id { get; set; } = null!;
        public string FullName { get; set; } = null!;
        public string Email { get; set; } = null!;
        public string FirstName { get; set; } = null!;
        public string LastName { get; set; } = null!;
        public Gender? Gender { get; set; }
        public Address? Address { get; set; }
        public DateTime? DateOfBirth { get; set; }
        public string? ImagePath { get; set; } // when register will not ask him to enter image, but in profile settings can upload image
        public DateTime CreationTime { get; set; } = DateTime.UtcNow;
        public bool IsDisabled { get; set; } = false;
        public int Age {  get; set; }
    }
}
