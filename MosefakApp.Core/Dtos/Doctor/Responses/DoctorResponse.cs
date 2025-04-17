namespace MosefakApp.Core.Dtos.Doctor.Responses
{
    public class DoctorResponse
    {
        public string Id { get; set; } = null!;
        public string FullName { get; set; } = null!;
        public string ImagePath { get; set; } = null!;
        public int NumberOfReviews { get; set; }
        public double Rating { get; set; }
        public int TotalYearsOfExperience { get; set; }
        public List<SpecializationResponse> Specializations { get; set; } = new();
    }
}
