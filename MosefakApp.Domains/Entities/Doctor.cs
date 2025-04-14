namespace MosefakApp.Domains.Entities
{
    public class Doctor : BaseEntity
    {
        public int AppUserId { get; set; } // to get another info like(email,phone,age,etc) from another database by using AppUserId
        public string LicenseNumber { get; set; } = null!;
        public string AboutMe { get; set; } = null!;
        public int NumberOfReviews { get; set; } = 0; // denormlized peroprty for best performance  
        
        [NotMapped]
        public int TotalYearsOfExperience => CalculateTotalExperience();
        public ICollection<Review> Reviews { get; set; } = new List<Review>();
        public List<Specialization> Specializations { get; set; } = new List<Specialization>();
        public List<AppointmentType> AppointmentTypes { get; set; } = new List<AppointmentType>();
        public ICollection<Award> Awards { get; set; } = new HashSet<Award>();
        public ICollection<Experience> Experiences { get; set; } = new HashSet<Experience>();
        public ICollection<Education> Educations { get; set; } = new HashSet<Education>();
        public ICollection<Clinic> Clinics { get; set; } = new HashSet<Clinic>();
        public List<Appointment> Appointments { get; set; } = new List<Appointment>();
        private int CalculateTotalExperience()
        {
            return Experiences.Sum(exp =>
                (exp.CurrentlyWorkingHere ? DateTime.UtcNow.Year : exp.EndDate?.Year ?? DateTime.UtcNow.Year) - exp.StartDate.Year);
        }
    }

}
