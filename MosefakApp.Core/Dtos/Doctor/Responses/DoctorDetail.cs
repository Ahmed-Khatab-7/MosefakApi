namespace MosefakApp.Core.Dtos.Doctor.Responses
{
    public class DoctorDetail
    {
        public string Id { get; set; } = null!;
        public string FullName { get; set; } = null!;
        public string ImagePath { get; set; } = null!;
        public string AboutMe { get; set; } = null!;
        public double Rating { get; set; } 
        public int NumberOfReviews { get; set; }
        public int TotalYearsOfExperience { get; set; }
        public long TotalPatinets { get; set; }
        public List<SpecializationResponse> Specializations { get; set; } = new();
        public List<ClinicResponse> Clinics { get; set; } = new();
        public List<ReviewResponse> Reviews { get; set; } = new();
        public List<AppointmentTypeResponse> AppointmentTypes { get; set; } = new();
        public List<EducationResponse> Educations { get; set; } = new();
        public List<AwardResponse> Awards { get; set; } = new();
        public List<ExperienceResponse> Experiences { get; set; } = new();
    }
}
